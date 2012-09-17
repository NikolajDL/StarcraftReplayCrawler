using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading.Tasks.Schedulers;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace StarcraftReplayCrawler
{
    public class Downloader
    {

        private static readonly Dictionary<string, string> MIME = new Dictionary<string, string>() 
        {{"application/x-compressed","zip"},
         {"application/x-zip-compressed","zip"},
         {"application/zip","zip"},
         {"multipart/x-zip","zip"},
         {"application/x-rar-compressed","rar"}};

        private ReplayLinkCollection _downloadLinks;
        private int _concurrentThreads = 1;
        private log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public Downloader(int concurrentThreads = 1)
        {
            _concurrentThreads = concurrentThreads;
        }

        public void Download(ReplayLinkCollection downloadLinks)
        {
            _downloadLinks = downloadLinks;

            Stopwatch sw = new Stopwatch();
            sw.Start();
            log.Info("Starting download of replays from source: " + downloadLinks.SourceName);

            if (Directory.Exists(downloadLinks.SourceName))
            {
                log.Info("    Deleting existing replay directory: " + downloadLinks.SourceName);
                Directory.Delete(downloadLinks.SourceName, true);
            }
            log.Info("    Creating directory for replays: " + downloadLinks.SourceName);
            Directory.CreateDirectory(downloadLinks.SourceName);

            LimitedConcurrencyTaskScheduler lcts = new LimitedConcurrencyTaskScheduler(_concurrentThreads);
            TaskFactory factory = new TaskFactory(lcts);

            int numberOfLinks = _downloadLinks.ReplayUrls.Count();
            log.Debug("    Number of links to download: " + numberOfLinks);
            Task[] tasks = new Task[numberOfLinks];
            int index = 0;
            log.Debug("    Starting async tasks with maximum " + _concurrentThreads + " concurrent threads.");
            while (index < numberOfLinks)
            {
                int value = index;
                tasks[value] = factory.StartNew(() =>
                {
                    var page = value;
                    log.Info("    Downloading link " + page);
                    DownloadSingle(page);
                    log.Info("    Completed link " + page);
                });
                index++;
            }

            Task.WaitAll(tasks);
            sw.Stop();
            log.Info("Finished downloading " + numberOfLinks + " replays in " + sw.Elapsed.TotalSeconds + " seconds.");

        }

        private void DownloadSingle(int id)
        {
            var url = _downloadLinks.ReplayUrls.ElementAt(id);
            var filename = _downloadLinks.SourceName + id;

            byte[] result;
            byte[] buffer = new byte[4096];

            WebRequest wr = WebRequest.Create(url);
            log.Debug("        Created webrequest.");
            try
            {
                using (WebResponse response = wr.GetResponse())
                {
                    log.Debug("        Performed GetResponse.");

                    log.Debug("        Assesing file extension.");
                    string extension = "dat";
                    if (MIME.ContainsKey(response.ContentType))
                        extension = MIME[response.ContentType];
                    else
                    {
                        switch (_downloadLinks.GameType)
                        {
                            case GameType.Starcraft:
                            case GameType.StarcraftBroodWar:
                                extension = "rep";
                                break;
                            case GameType.StarcraftII:
                                extension = "sc2replay";
                                break;
                        }
                    }
                    log.Debug("            Extension is: ." + extension);

                    log.Debug("        Try getting filename from response headers");
                    filename = response.Headers["Content-Disposition"];
                    if (String.IsNullOrEmpty(filename))
                    {
                        log.Debug("        Headers empty. Getting filename from response uri.");
                        filename = Path.GetFileName(HttpUtility.UrlDecode(response.ResponseUri.AbsoluteUri));
                    }
                    if (String.IsNullOrEmpty(filename))
                    {
                        log.Debug("        Headers and response uri empty. Using default filename.");
                        filename = _downloadLinks.SourceName;
                    }
                    log.Debug("            Filename found is: " + filename);

                    using (Stream responseStream = response.GetResponseStream())
                    {
                        log.Debug("        Getting response stream to read from.");
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            log.Debug("        Creating memory stream to read into.");
                            int count = 0;
                            log.Debug("        Begin reading bytes into memory stream...");
                            do
                            {
                                count = responseStream.Read(buffer, 0, buffer.Length);
                                memoryStream.Write(buffer, 0, count);

                            } while (count != 0);
                            log.Debug("        Finished reading bytes.");

                            result = memoryStream.ToArray();

                            log.Debug("        Opening filestream with Write access and FileMode.Create");
                            using (FileStream file = new FileStream(_downloadLinks.SourceName + "/" + filename + "(" + id + ")" + "." + extension, FileMode.Create, FileAccess.Write))
                            {
                                log.Debug("        Preparing to write bytes from memory stream to " + filename + "." + extension);
                                file.Write(result, 0, result.Length);
                                file.Flush();
                                log.Debug("        Done writing to file.");
                            }
                        }
                    }
                }
            }
            catch(Exception e)
            {
                log.Error(e.Message, e);
            }
        }

    }
}
