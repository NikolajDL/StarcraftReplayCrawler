using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftReplayCrawler
{
    public interface IReplaySource
    {
        GameType GameType { get; set; }
        string SourceName { get; set; }
        string SourceUrl { get; set; }
    }
}
