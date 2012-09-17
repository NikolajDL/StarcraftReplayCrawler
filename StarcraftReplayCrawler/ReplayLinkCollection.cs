using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftReplayCrawler
{
    public class ReplayLinkCollection
    {
        public GameType GameType { get; private set; }
        public string SourceName { get; private set; }

        private List<int> _listofdownloadids = new List<int>();
        private string _downloadUrlFormat;
        public IEnumerable<string> ReplayUrls
        {
            get
            {
                foreach (var replayId in _listofdownloadids.Distinct())
                {
                    yield return String.Format(_downloadUrlFormat, replayId);
                }
            }
        }

        public ReplayLinkCollection(string name, GameType gametype, List<int> downloadids, string downloadUrlFormat)
        {
            SourceName = name;
            GameType = gametype;
            _listofdownloadids = downloadids;
            _downloadUrlFormat = downloadUrlFormat;
        }
    }
}
