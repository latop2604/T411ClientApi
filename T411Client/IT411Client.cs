using System.Collections.Generic;
using System.IO;

namespace T411
{
    using System.Threading.Tasks;

    public interface IT411Client
    {
        int UserId { get; }
        Task<List<Torrent>> GetTop100Async();
        Task<List<Torrent>> GetTopTodayAsync();
        Task<List<Torrent>> GetTopWeekAsync();
        Task<List<Torrent>> GetTopMonthAsync();
        Task<TorrentDetails> GetTorrentDetailsAsync(int id);
        Stream DownloadTorrent(int id);
        Task<UserDetails> GetUserDetailsAsync(int id);
        Task<QueryResult> GetQueryAsync(string query);
        Task<QueryResult> GetQueryAsync(string query, QueryOptions options);
        Task<Dictionary<int, Category>> GetCategoryAsync();
        Task<List<TermCategory>> GetTermsAsync();
        Task<List<Torrent>> GetBookmarksAsync();
        Task<int> CreateBookmarkAsync(int id);
        Task<int> DeleteBookmarkAsync(IEnumerable<int> bookmarkIds);
    }
}