using System.Collections.Generic;

namespace T411.Html
{
    public interface IT411HtmlParser
    {
        List<Torrent> ParseTopToday(string page);
        QueryResult ParseQueryResult(string page);

        TorrentDetails ParseTorrentDetails(string page);
    }
}
