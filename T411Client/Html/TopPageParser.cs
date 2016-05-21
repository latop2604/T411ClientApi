using System.Collections.Generic;

namespace T411.Html
{
    public static class TopPageParser
    {
        public static List<Torrent> ParseTopToday(string page)
        {
            var parser = DependencyResolver.Resolve<IT411HtmlParser>();
            return parser.ParseTopToday(page);
        }
        public static QueryResult ParseQueryResult(string page)
        {
            var parser = DependencyResolver.Resolve<IT411HtmlParser>();
            return parser.ParseQueryResult(page);
        }
        public static TorrentDetails ParseTorrentDetails(string page)
        {
            var parser = DependencyResolver.Resolve<IT411HtmlParser>();
            return parser.ParseTorrentDetails(page);
        }
    }
}
