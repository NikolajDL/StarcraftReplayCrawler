using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.Web;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading.Tasks.Schedulers;

namespace StarcraftReplayCrawler
{
    public class Crawler
    {
        private int _concurrentThreads = 1;
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SimpleReplaySource _source;
        private List<int> _listofdownloadids;

        public Crawler(int concurrentThreads = 1)
        {
            _concurrentThreads = concurrentThreads;
            ServicePointManager.DefaultConnectionLimit = _concurrentThreads * 2;
        }

        public ReplayLinkCollection Crawl(SimpleReplaySource source)
        {

            _listofdownloadids = new List<int>();
            _source = source;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            log.Info("Starting crawling of source: " + _source.SourceName);

            LimitedConcurrencyTaskScheduler lcts = new LimitedConcurrencyTaskScheduler(_concurrentThreads);
            TaskFactory factory = new TaskFactory(lcts);

            int numberOfPages = _source.Pages;
            log.Debug("    Number of pages to crawl: " + numberOfPages);
            Task[] tasks = new Task[numberOfPages];
            int index = 0;
            log.Debug("    Starting async tasks with maximum " + _concurrentThreads + " concurrent threads.");
            while (index < numberOfPages)
            {
                int value = index;
                tasks[value] = factory.StartNew(() =>
                {
                    var page = value;
                    log.Info("    Processing page " + page);
                    ProcessPage(page);
                    log.Info("    Completed page " + page);
                });
                index++;
            }

            Task.WaitAll(tasks);
            sw.Stop();
            log.Info("Finished crawling " + numberOfPages + " pages, resulting in " + _listofdownloadids.Distinct().Count() + " replay download links in " + sw.Elapsed.TotalSeconds + " seconds.");


            return new ReplayLinkCollection(_source.SourceName, _source.GameType, _listofdownloadids, _source.DownloadUrlFormat);
        }

        private void ProcessPage(int page)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            var pageValue = _source.PageFunction(_source, page);

            var html = GetWebText(String.Format(_source.SourceUrl, pageValue));

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);

            var nodes = doc.DocumentNode.SelectNodes(_source.SourceReplayUrlXPathSearch);
            if (nodes != null && nodes.Count > 0)
            {
                foreach (HtmlNode link in nodes)
                {
                    var uri = new Uri(_source.DownloadUrlPrefix + link.GetAttributeValue("href", "")).Query;
                    uri = HttpUtility.HtmlDecode(uri);
                    var id = HttpUtility.ParseQueryString(uri)[_source.ReplayIDQueryKey];
                    if (!string.IsNullOrEmpty(id))
                        _listofdownloadids.Add(int.Parse(id));
                }
            }
            sw.Stop();
            if(nodes!=null)
                log.Debug("        Found " + nodes.Count + " link nodes on page in " + sw.Elapsed.TotalSeconds + " seconds.");
            else
                log.Debug("        Found no link nodes on page in " + sw.Elapsed.TotalSeconds + " seconds.");
        }

        private string GetWebText(string url)
        {
            log.Debug("        Retrieving page content.");
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse response = request.GetResponse() as HttpWebResponse;

            log.Debug("        Response code " + response.StatusCode);

            Stream stream = response.GetResponseStream();


            StreamReader reader = new StreamReader(stream);
            string htmlText = reader.ReadToEnd();

            response.Close();
            stream.Close();
            reader.Close();
            return htmlText;
        }
    }

}
