using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;

namespace T411
{
    using System.Linq;
    using System.Threading.Tasks;

    public class T411Client : IT411Client
    {
        private static readonly Regex BadOccurBefore = new Regex(@"\},([0-9]+)(,\{)?");
        private static readonly Regex BadOccurAfter = new Regex(@"([0-9]+),\{");

        private readonly string _username;
        private readonly string _password;

        private string _token;

        public static string BaseAddress { get; set; } = "https://api.t411.ch/";

        private int _userId = -1;
        public int UserId
        {
            get
            {
                if (_userId == -1)
                {
                    if (_token == null)
                        throw new InvalidOperationException("Null token");
                    string[] part = _token.Split(new[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (part.Length != 3)
                        throw new InvalidOperationException("Invalide token format");

                    if (!int.TryParse(part[0], out _userId))
                        throw new InvalidOperationException("Invalide token user id format");
                }
                return _userId;
            }
        }

        public T411Client(string username, string password)
        {
            _username = username;
            _password = password;
        }

        private string GetToken()
        {
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                {
                    Dictionary<string, string> dico = new Dictionary<string, string>();
                    dico.Add("username", _username);
                    dico.Add("password", _password);

                    var task = client.PostAsync("/auth", new FormUrlEncodedContent(dico));
                    HttpResponseMessage response = task.Result;
                    var tokResult = response.Content.ReadAsStringAsync().Result;
                    var tokObj = JsonConvert.DeserializeObject<AuthResult>(tokResult);
                    string token = tokObj.Token;
                    return token;
                }
            }
        }

        /// <summary>
        /// Load user's token if is null
        /// </summary>
        public void EnsuresInitToken()
        {
            if(_token == null)
            {
                _token = GetToken();
            }
        }

        public Task<List<Torrent>> GetTop100Async() =>  GetTorrentsAsync("/torrents/top/100");
        public Task<List<Torrent>> GetTopTodayAsync() => GetTorrentsAsync("/torrents/top/today");
        public Task<List<Torrent>> GetTopWeekAsync() => GetTorrentsAsync("/torrents/top/week");
        public Task<List<Torrent>> GetTopMonthAsync() => GetTorrentsAsync("/torrents/top/month");

        private Task<List<Torrent>> GetTorrentsAsync(string uri) => GetResponseAsync<List<Torrent>>(new Uri(uri, UriKind.Relative));

        public Task<TorrentDetails> GetTorrentDetailsAsync(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/details/{0}", id);
            return GetResponseAsync<TorrentDetails>(new Uri(uri, UriKind.Relative));
        }

        public Stream DownloadTorrent(int id)
        {
            EnsuresInitToken();

            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/download/{0}", id);

            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    requestMessage.Headers.TryAddWithoutValidation("Authorization", _token);

                    using (var response = client.SendAsync(requestMessage).Result)
                    {
                        MemoryStream ms = null;
                        try
                        {
                            ms = new MemoryStream();

                            using (var msLocal = new MemoryStream())
                            {
                                response.Content.CopyToAsync(msLocal).Wait();
                                msLocal.Position = 0;
                                StreamReader sr = new StreamReader(msLocal);

                                string data = sr.ReadToEnd();
                                if (data.StartsWith("{\"error\":", StringComparison.OrdinalIgnoreCase))
                                {
                                    ErrorResult error = JsonConvert.DeserializeObject<ErrorResult>(data);
                                    throw ErrorCodeException.CreateFromErrorCode(error);
                                }
                                msLocal.Position = 0;
                                msLocal.CopyTo(ms);
                            }

                            ms.Position = 0;
                            return ms;
                        }
                        catch
                        {
                            ms?.Dispose();
                            throw;
                        }
                    }
                }
            }
        }

        public Task<UserDetails> GetUserDetailsAsync(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/users/profile/{0}", id);
            return GetResponseAsync<UserDetails>(new Uri(uri, UriKind.Relative));
        }

        public Task<QueryResult> GetQueryAsync(string query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}", query);
            return GetResponseAsync<QueryResult>(new Uri(uri, UriKind.Relative), QueryRawDataCleaner);
        }

        public Task<QueryResult> GetQueryAsync(string query, QueryOptions options)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/torrents/search/{0}?{1}", query.Replace(" ", "%20"), ToQueryString(options));
            return GetResponseAsync<QueryResult>(new Uri(uri, UriKind.Relative), QueryRawDataCleaner);
        }

        private static string ToQueryString(QueryOptions options)
        {
            List<string> parameters = new List<string>();
            if (options.Offset > 0)
            {
                parameters.Add("offset=" + options.Offset);
            }
            if (options.Limit > 0)
            {
                parameters.Add("limit=" + options.Limit);
            }
            if (options.CategoryIds != null && options.CategoryIds.Count > 0)
            {
                parameters.AddRange(options.CategoryIds.Select(categoryId => "cid=" + categoryId));
            }
            if (options.Terms != null)
            {
                parameters.AddRange(options.Terms.Select(term => string.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0}][]={1}", term.TermTypeId, term.Id)));
            }

            return string.Join("&", parameters);
        }

        /// <summary>
        /// Used to remove invalid items in search's query torrent result list like "...},34545,{...".
        /// </summary>
        /// <param name="data">The raw data string</param>
        /// <returns>The cleaned raw data</returns>
        private static string QueryRawDataCleaner(string data) => BadOccurAfter.Replace(BadOccurBefore.Replace(data, @"}$2"), @"{");
        

        public Task<Dictionary<int, Category>> GetCategoryAsync()
        {
            const string Uri = "/categories/tree";
            return GetResponseAsync<Dictionary<int, Category>>(new Uri(Uri, UriKind.Relative));
        }

        public async Task<List<TermCategory>> GetTermsAsync()
        {
            const string Uri = "/terms/tree";
            Dictionary<int, Dictionary<int, TermType>> result = await GetResponseAsync<Dictionary<int, Dictionary<int, TermType>>>(new Uri(Uri, UriKind.Relative));

            List<TermCategory> list = new List<TermCategory>();
            if (result != null)
            {
                foreach (var categoryItem in result)
                {
                    TermCategory termCategory = new TermCategory();
                    termCategory.Id = categoryItem.Key;
                    termCategory.TermTypes = new List<TermType>();
                    if (categoryItem.Value != null)
                    {
                        foreach (var termItem in categoryItem.Value)
                        {
                            TermType termType = termItem.Value;
                            termType.Id = termItem.Key;
                            termCategory.TermTypes.Add(termType);
                        }
                    }
                    list.Add(termCategory);
                }
            }

            return list;
        }

        private Task<T> GetResponseAsync<T>(Uri uri) where T : class
        {
            var result = GetResponseAsync<T>(uri, null);

            if (result == null)
                throw new InvalidDataException("The query return no result");

            return result;
        }

        private Task<T> GetNotNullableResponseAsync<T>(Uri uri) where T : struct
        {
            return GetResponseAsync<T>(uri, null);
        }

        private async Task<T> GetResponseAsync<T>(Uri uri, Func<string,string> rawDataCallback)
        {
            string data = await GetRawResponseAsync(uri);


            if (data == null)
                throw new InvalidDataException("The query return no data");

            if (data.StartsWith("{\"error\":", StringComparison.OrdinalIgnoreCase))
            {
                ErrorResult error = JsonConvert.DeserializeObject<ErrorResult>(data);
                throw ErrorCodeException.CreateFromErrorCode(error);
            }

            if (rawDataCallback != null)
            {
                data = rawDataCallback(data);
            }

            try
            {
                T result = JsonConvert.DeserializeObject<T>(data);
                return result;

            }
            catch (Exception ex)
            {
                throw new InvalidDataException(data, ex);
            }
        }

        private async Task<string> GetRawResponseAsync(Uri uri)
        {
            EnsuresInitToken();
            using (var handler = new HttpClientHandler())
            {
                if (handler.SupportsAutomaticDecompression)
                {
                    handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                }
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                {
                    requestMessage.Headers.TryAddWithoutValidation("Authorization", _token);

                    using (HttpResponseMessage response = await client.SendAsync(requestMessage))
                    {
                        string result = await response.Content.ReadAsStringAsync();
                        return result;
                    }
                }
            }
        }

        public Task<List<Torrent>> GetBookmarksAsync()
        {
            return GetResponseAsync<List<Torrent>>(new Uri("/bookmarks", UriKind.Relative));
        }

        public Task<int> CreateBookmarkAsync(int id)
        {
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/save/{0}", id);
            return GetNotNullableResponseAsync<int>(new Uri(uri, UriKind.Relative));
        }

        public Task<int> DeleteBookmarkAsync(IEnumerable<int> bookmarkIds)
        {
            string ids = string.Join(",", bookmarkIds);
            string uri = string.Format(System.Globalization.CultureInfo.InvariantCulture, "/bookmarks/delete/{0}", ids);
            return GetNotNullableResponseAsync<int>(new Uri(uri, UriKind.Relative));
        }
    }
}
