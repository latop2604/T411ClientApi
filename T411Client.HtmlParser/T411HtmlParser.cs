namespace T411.HtmlParser
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    using CsQuery;

    using T411;
    using T411.Html;

    public class T411HtmlParser : IT411HtmlParser
    {
        private static readonly CultureInfo EnUs = CultureInfo.GetCultureInfo("en-US");
        private static readonly CultureInfo FrFr = CultureInfo.GetCultureInfo("fr-FR");

        public List<Torrent> ParseTopToday(string page)
        {
            if (page == null)
                return new List<Torrent>();

            var dom = CQ.Create(page);
            var result = ReadTorrentsResult(dom);

            return result;
        }

        public QueryResult ParseQueryResult(string page)
        {
            QueryResult queryResult = new QueryResult();
            if (page == null)
            {
                return queryResult;
            }

            var dom = CQ.Create(page);
            var result = ReadTorrentsResult(dom);
            queryResult.Torrents = result;

            return queryResult;
        }

        public TorrentDetails ParseTorrentDetails(string page)
        {
            if (page == null)
                throw new ArgumentNullException(nameof(page));

            TorrentDetails details = new TorrentDetails();

            var dom = CQ.Create(page);
            details.Description = dom.Select("div.block.description article").Html();
            details.Name = dom.Select("div.accordion div:nth-of-type(1) table tr:nth-of-type(1) td").Text();
            details.Categoryname = dom.Select("div.accordion div:nth-of-type(1) table tr:nth-of-type(3) td").Text();
            details.Category = MapCategory(details.Categoryname);
            details.Privacy = Privacy.Normal;
            details.Owner = 0;
            details.Username = dom.Select("div.accordion div:nth-of-type(1) table tr:nth-of-type(6) td").Text();
            details.IsVerified = dom.Select("div.block.description .torrent-status.verify").Any();

            return details;
        }

        private static int MapCategory(string name)
        {
            name = name ?? string.Empty;
            switch (name.ToLowerInvariant())
            {
                case "film":
                    return 631;
                case "concert":
                    return 633;
                case "musique":
                    return 623;
                case "série tv":
                    return 433;
                case "animation":
                    return 637;
                default:
                    return 0;
            }
        }

        private List<Torrent> ReadTorrentsResult(CQ dom)
        {
            var blocks = dom.Select("div.content div.block table.results tbody tr");


            List<Torrent> result = new List<Torrent>(blocks.Length);

            for (int i = 0; i < blocks.Length; i++)
            {
                try
                {
                    var cq = dom.Select("div.content div.block table.results tbody tr").Eq(i);
                    var torrent = ParseBlock(cq);
                    result.Add(torrent);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
#if DEBUG
                    Debugger.Break();
#endif
                }
            }
            return result;
        }

        private Torrent ParseBlock(CQ block)
        {
            var linkCatId = block.Find("td:nth-of-type(1) > a");
            string hrefCat = linkCatId.Attr("href");
            var startIndex = "/torrents/search/?subcat=".Length;
            var strCatId = hrefCat.Substring(startIndex, hrefCat.Length - startIndex);
            int catId = int.Parse(strCatId);


            var titleLink = block.Find("td:nth-of-type(2) > a:nth-of-type(1)").First();
            string title = titleLink.Attr("title") ?? titleLink.Text();

            bool isVerified = block.Find("td:nth-child(2) > a:nth-child(1) > span.up").Any();

            var linkId = block.Find("td:nth-of-type(3) > a.nfo").First();
            string hrefId = linkId.Attr("href");

            int id = 0;
            if (!string.IsNullOrEmpty(hrefId))
            {
                startIndex = "/torrents/nfo/?id=".Length;
                var strId = hrefId.Substring(startIndex, hrefId.Length - startIndex);
                id = int.Parse(strId);
            }

            var sizeLink = block.Find("td:nth-of-type(6)").First();
            var strSize = sizeLink.Text();
            var size = GetSize(strSize);

            int comment = GetNumber(block, 4);
            int complete = GetNumber(block, 7);
            int seeders = GetNumber(block, 8);
            int leechers = GetNumber(block, 9);

            var dateSelector = block.Find("td:nth-of-type(2) > dl dd:nth-of-type(1)").First();
            string hrefDate = dateSelector.Text().Replace(" (+00:00)", "");
            DateTime date = DateTime.ParseExact(hrefDate, "yyyy-MM-dd HH:mm:ss", FrFr);

            return new Torrent
            {
                Id = id,
                Name = title,
                Size = size,
                Category = catId,
                Added = date,
                TimesCompleted = complete,
                Seeders = seeders,
                Leechers = leechers,
                Comments = comment,
                IsVerified = isVerified ? 1 : 0
            };
        }

        private static int GetNumber(CQ block, int column)
        {
            var balise = block.Find("td:nth-of-type(" + column + ")").First();
            var text = balise.Text();
            var number = int.Parse(text);
            return number;
        }

        private static long GetSize(string strSize)
        {
            if (string.IsNullOrEmpty(strSize))
            {
                return 0;
            }

            long unitFactor = 1;

            if (strSize.EndsWith("KB", StringComparison.OrdinalIgnoreCase))
            {
                unitFactor = 1024;
            }
            else if (strSize.EndsWith("MB", StringComparison.OrdinalIgnoreCase))
            {
                unitFactor = 1024 * 1024;
            }
            else if (strSize.EndsWith("GB", StringComparison.OrdinalIgnoreCase))
            {
                unitFactor = 1024 * 1024 * 1024;
            }
            else if (strSize.EndsWith("TB", StringComparison.OrdinalIgnoreCase))
            {
                unitFactor = 1099511627776;
            }

            long size = (long)double.Parse(strSize.Substring(0, strSize.Length - 3), EnUs) * unitFactor;
            return size;
        }
    }
}
