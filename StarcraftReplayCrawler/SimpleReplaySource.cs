using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace StarcraftReplayCrawler
{
    public class SimpleReplaySource : BaseReplaySource, IReplaySource
    {
        public string SourceReplayUrlXPathSearch { get; set; }
        public bool IsPaged { get; set; }
        public string PageQueryFormat { get; set; }
        public string ReplayIDQueryKey { get; set; }
        public string DownloadUrlFormat { get; set; }
        public string DownloadUrlPrefix { get; set; }
        public Func<SimpleReplaySource, int, int> PageFunction { get; set; }
        public int Pages { get; set; }
        public int PageSize { get; set; }

        public static SimpleReplaySource CreateFromXMLFile(string url)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(url);

            var root = doc.SelectSingleNode("//SimpleReplaySource");
            SimpleReplaySource result = new SimpleReplaySource();

            result.SourceName = root["SourceName"].InnerText;
            result.SourceUrl = root["SourceUrl"].InnerText;

            switch (root["GameType"].InnerText)
            { 
                case "Starcraft":           result.GameType = GameType.Starcraft; break;
                case "StarcraftBroodWar":   result.GameType = GameType.StarcraftBroodWar; break;
                case "StarcraftII":         result.GameType = GameType.StarcraftII; break;
            }

            result.SourceReplayUrlXPathSearch = root["SourceReplayUrlXPathSearch"].InnerText;
            result.IsPaged = bool.Parse(root["IsPaged"].InnerText);

            if(root["PageQueryFormat"]!=null)
                result.PageQueryFormat = root["PageQueryFormat"].InnerText;
            result.ReplayIDQueryKey = root["ReplayIDQueryKey"].InnerText;
            result.DownloadUrlFormat = root["DownloadUrlFormat"].InnerText;
            if (root["DownloadUrlPrefix"] != null)
                result.DownloadUrlPrefix = root["DownloadUrlPrefix"].InnerText;

            switch (root["PageFunction"].InnerText)
            {
                case "IncrementalPaging":   result.PageFunction = PagingFunctions.IncrementalPaging; break;
                case "StartIndexPaging":    result.PageFunction = PagingFunctions.StartIndexPaging; break;
                default:                    result.PageFunction = PagingFunctions.IncrementalPaging; break;
            }

            if (root["Pages"] != null)
                result.Pages = int.Parse(root["Pages"].InnerText);
            if (root["PageSize"] != null)
                result.PageSize = int.Parse(root["PageSize"].InnerText);

            return result;
        }
    }
}
