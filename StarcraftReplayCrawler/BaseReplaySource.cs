using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftReplayCrawler
{
    public abstract class BaseReplaySource : IReplaySource
    {
        public virtual GameType GameType { get; set; }
        public virtual string SourceName { get; set; }
        public virtual string SourceUrl { get; set; }
    }
}
