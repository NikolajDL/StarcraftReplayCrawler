using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StarcraftReplayCrawler
{
    public sealed class PagingFunctions
    {
        public static Func<SimpleReplaySource, int, int> IncrementalPaging = (replaySource, page) => { return page; };
        public static Func<SimpleReplaySource, int, int> StartIndexPaging = (replaySource, page) => { return page * replaySource.PageSize; };
    }
}
