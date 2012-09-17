using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Xml;

namespace StarcraftReplayCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if(args.Length<=0)
                Environment.Exit(10022);

            if (String.IsNullOrWhiteSpace(args[0]))
                Environment.Exit(10022);
            string xmlFileUrl = args[0];

            int crawlerThreads = 8;
            if (args.Length >= 2 && !int.TryParse(args[1], out crawlerThreads))
                Environment.Exit(10022);

            int downloadThreads = crawlerThreads;
            if (args.Length >= 3 && !int.TryParse(args[2], out downloadThreads))
                Environment.Exit(10022);

            SimpleReplaySource replaySource = SimpleReplaySource.CreateFromXMLFile(xmlFileUrl);

            Crawler crawler = new Crawler(crawlerThreads);
            Downloader dl = new Downloader(downloadThreads);

            var links = crawler.Crawl(replaySource);
            dl.Download(links);
            
            Console.WriteLine("Done!");
        }



        /*
        
        static void TestingDownloaderAndXml(string url)
        {
            SimpleReplaySource replaySource = SimpleReplaySource.CreateFromXMLFile(url);

            Crawler crawler = new Crawler(8);
            Downloader dl = new Downloader(8);
            var links = crawler.Crawl(replaySource);

            dl.Download(links);
        }
        static void TestingDownloader()
        {
            SimpleReplaySource replaySource = new SimpleReplaySource
            {
                SourceName = "reps.ru",
                GameType = GameType.Starcraft,
                SourceUrl = "http://reps.ru/replays.php?type=duel&page={0}",
                DownloadUrlFormat = "http://reps.ru/replays.php?replay=get&id={0}",
                DownloadUrlPrefix = "http://reps.ru/",
                SourceReplayUrlXPathSearch = @"//a[contains(@href,'replays.php')" +
                                                " and contains(@href,'replay=comment')]",
                ReplayIDQueryKey = "id",
                IsPaged = true,
                PageFunction = PagingFunctions.IncrementalPaging,
                Pages = 2
            };

            Crawler crawler = new Crawler(8);
            Downloader dl = new Downloader(8);
            var links = crawler.Crawl(replaySource);

            dl.Download(links);
        }
        static void TestSimpleReplaySource3()
        {
            SimpleReplaySource replaySource = new SimpleReplaySource
            {
                SourceName = "reps.ru",
                GameType = GameType.Starcraft,
                SourceUrl = "http://reps.ru/replays.php?type=duel&page={0}",
                DownloadUrlFormat = "http://reps.ru/replays.php?replay=get&id={0}",
                DownloadUrlPrefix = "http://reps.ru/",
                SourceReplayUrlXPathSearch = @"//a[contains(@href,'replays.php')" +
                                                " and contains(@href,'replay=comment')]",
                ReplayIDQueryKey = "id",
                IsPaged = true,
                PageFunction = PagingFunctions.IncrementalPaging,
                Pages = 130
            };

            Crawler crawler = new Crawler(8);
            var links = crawler.Crawl(replaySource);

            // Write to file
            var stream = File.CreateText(replaySource.SourceName + ".txt");
            foreach (var replay in links.ReplayUrls)
                stream.WriteLine(replay);

            stream.Flush();
            stream.Close();
        }
        static void TestSimpleReplaySource2()
        {
            SimpleReplaySource replaySource = new SimpleReplaySource
            {
                SourceName = "GosuGamers.net",
                GameType = GameType.Starcraft,
                SourceUrl = "http://www.gosugamers.net/starcraft/replays.php?&start={0}",
                DownloadUrlFormat = "http://www.gosugamers.net/starcraft/admin/a_replays.php?dl={0}",
                DownloadUrlPrefix = "http://www.gosugamers.net/starcraft/",
                SourceReplayUrlXPathSearch = @"//a[contains(@href,'admin/a_replays.php')" +
                                                " and contains(@href,'dl')]",
                ReplayIDQueryKey = "dl",
                IsPaged = true,
                PageFunction = PagingFunctions.StartIndexPaging,
                Pages = 80,
                PageSize = 100
            };

            Crawler crawler = new Crawler(8);
            var links = crawler.Crawl(replaySource);

            // Write to file
            var stream = File.CreateText(replaySource.SourceName + ".txt");
            foreach (var replay in links.ReplayUrls)
                stream.WriteLine(replay);

            stream.Flush();
            stream.Close();
        }
        static void TestSimpleReplaySource()
        {
            SimpleReplaySource replaySource = new SimpleReplaySource
            {
                SourceName = "GameReplays.org",
                GameType = GameType.StarcraftII,
                SourceUrl = "http://www.gamereplays.org/starcraft2/replays.php?game=33&show=expert_replays&st={0}",
                DownloadUrlFormat = "http://www.gamereplays.org/starcraft2/replays.php?game=33&show=download&&id={0}",
                SourceReplayUrlXPathSearch = @"//a[contains(@href,'www.gamereplays.org/starcraft2/replays.php')" +
                                                " and contains(@href,'download')" +
                                                " and contains(@href,'game=33')]",
                ReplayIDQueryKey = "id",
                IsPaged = true,
                PageFunction = PagingFunctions.StartIndexPaging,
                Pages = 112,
                PageSize = 30
            };

            Crawler crawler = new Crawler(8);
            var links = crawler.Crawl(replaySource);

            // Write to file
            var stream = File.CreateText(replaySource.SourceName + ".txt");
            foreach (var replay in links.ReplayUrls)
                stream.WriteLine(replay);

            stream.Flush();
            stream.Close();
        }

        /**/
    }
}
