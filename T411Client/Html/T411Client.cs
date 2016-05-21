using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Net.Http;

namespace T411.Html
{
    using System.Linq;
    using System.Threading.Tasks;

    public class T411Client : IT411Client
    {
        private readonly string _username;
        private readonly string _password;
        private readonly CookieContainer _cookieContainer = new CookieContainer();

        private bool _isLogin = false;

        public int UserId { get; private set; }
        public static string BaseAddress { get; set; } = "http://www.t411.ch/";

        public T411Client(string username, string password)
        {
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Load user's token if is null
        /// </summary>
        public void EnsuresLogin()
        {
            if (!_isLogin)
            {
                Login();
                _isLogin = true;
            }
        }

        private async Task<List<Torrent>> GetTopAsync(string uri)
        {
            string page = await GetRawResponseAsync(uri);
            var result = TopPageParser.ParseTopToday(page);
            return result;
        }

        public Task<List<Torrent>> GetTop100Async()
        {
            return GetTopAsync("/top/100/");
        }

        public Task<List<Torrent>> GetTopTodayAsync()
        {
            return GetTopAsync("/top/today/");
        }

        public Task<List<Torrent>> GetTopWeekAsync()
        {
            return GetTopAsync("/top/week/");
        }

        public Task<List<Torrent>> GetTopMonthAsync()
        {
            return GetTopAsync("/top/month/");
        }

        public async Task<TorrentDetails> GetTorrentDetailsAsync(int id)
        {
            string page = await GetRawResponseAsync("/torrents/?id=" + id);
            var result = TopPageParser.ParseTorrentDetails(page);
            result.Id = id;
            return result;
        }

        public Stream DownloadTorrent(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<UserDetails> GetUserDetailsAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<QueryResult> GetQueryAsync(string query)
        {
            return GetQueryAsync(query, new QueryOptions
            {
                Limit = 50,
                Offset = 0,
            });
        }

        public async Task<QueryResult> GetQueryAsync(string query, QueryOptions options)
        {
            string uri = $"/torrents/search/?search={query.Replace(" ", "%20")}&{ToQueryString(options)}";
            string page = await GetRawResponseAsync(uri);
            var result = TopPageParser.ParseQueryResult(page);
            result.Total = options.Limit * 20;
            result.Limit = options.Limit;
            result.Offset = options.Offset;
            result.Query = query;
            return result;
        }

        private static string ToQueryString(QueryOptions options)
        {
            List<string> parameters = new List<string>();
            if (options.Offset >= 0 && options.Limit > 0)
            {
                parameters.Add("page=" + (options.Offset / options.Limit));
            }
            if (options.CategoryIds != null && options.CategoryIds.Count > 0)
            {
                parameters.AddRange(options.CategoryIds.Select(categoryId => "subcat=" + categoryId));
            }

            if (options.SortOrder == SortOrder.Desc)
            {
                parameters.Add("type=desc");
            }
            else if ((options.SortOrder == SortOrder.Asc))
            {
                parameters.Add("type=asc");
            }

            parameters.Add(CreateSortParameter(options));

            return string.Join("&", parameters);
        }

        private static string CreateSortParameter(QueryOptions options)
        {
            switch (options.SortColumn)
            {
                case SortColumn.Category: return "order=category";
                case SortColumn.Name: return "order=name";
                case SortColumn.Comments: return "order=comments";
                case SortColumn.Added: return "order=added";
                case SortColumn.Size: return "order=size";
                case SortColumn.TimesCompleted: return "order=times_completed";
                case SortColumn.Seeders: return "order=seeders";
                case SortColumn.Leechers: return "order=leechers";
                default:
                    throw new InvalidOperationException();
            }
        }

        public Task<Dictionary<int, Category>> GetCategoryAsync()
        {
            var categories = new Dictionary<int, Category>
            {
                { 210, new Category { Id = 210, Name = "Film/Vidéo", Cats = new Dictionary<int, Category>
                    {
                        { 631, new Category { Id = 631, Pid = 210, Name = "Film"}                 },
                        { 433, new Category { Id = 433, Pid = 210, Name = "Série TV"}             },
                        { 455, new Category { Id = 455, Pid = 210, Name = "Animation"}            },
                        { 637, new Category { Id = 637, Pid = 210, Name = "Animation Série"}      },
                        { 633, new Category { Id = 633, Pid = 210, Name = "Concert"}              },
                        { 634, new Category { Id = 634, Pid = 210, Name = "Documentaire"}         },
                        { 639, new Category { Id = 639, Pid = 210, Name = "Emission TV"}          },
                        { 635, new Category { Id = 635, Pid = 210, Name = "Spectacle"}            },
                        { 636, new Category { Id = 636, Pid = 210, Name = "Sport"}                },
                        { 402, new Category { Id = 402, Pid = 210, Name = "Vidéo-clips"}         },
                    }}
                },
                { 395, new Category { Id = 395, Name = "Audio", Cats = new Dictionary<int, Category>
                    {
                        { 400, new Category { Id = 400, Pid = 395, Name = "Karaoke"}              },
                        { 623, new Category { Id = 623, Pid = 395, Name = "Musique"}              },
                        { 403, new Category { Id = 403, Pid = 395, Name = "Samples"}              },
                        { 642, new Category { Id = 642, Pid = 395, Name = "Podcast Radio"}        },
                    }}
                },
                { 404, new Category { Id = 404, Name = "eBook", Cats = new Dictionary<int, Category>
                    {
                        { 405, new Category { Id = 405, Pid = 404, Name = "Audio"}                },
                        { 406, new Category { Id = 406, Pid = 404, Name = "Bds"}                  },
                        { 407, new Category { Id = 407, Pid = 404, Name = "Comics"}               },
                        { 408, new Category { Id = 408, Pid = 404, Name = "Livres"}               },
                        { 409, new Category { Id = 409, Pid = 404, Name = "Mangas"}               },
                        { 410, new Category { Id = 410, Pid = 404, Name = "Presse"}               },
                    }}
                },
                { 340, new Category { Id = 340, Name = "Emulation", Cats = new Dictionary<int, Category>
                    {
                        { 342, new Category { Id = 342, Pid = 340, Name = "Emulateurs"}           },
                        { 344, new Category { Id = 344, Pid = 340, Name = "Roms"}                 },
                    }}
                },
                { 624, new Category { Id = 624, Name = "Jeu vidéo", Cats = new Dictionary<int, Category>
                    {
                        { 239, new Category { Id = 239, Pid = 624, Name = "Linux"}                },
                        { 245, new Category { Id = 245, Pid = 624, Name = "MacOS"}                },
                        { 246, new Category { Id = 246, Pid = 624, Name = "Windows"}              },
                        { 309, new Category { Id = 309, Pid = 624, Name = "Microsoft"}            },
                        { 307, new Category { Id = 307, Pid = 624, Name = "Nintendo"}             },
                        { 308, new Category { Id = 308, Pid = 624, Name = "Sony"}                 },
                        { 626, new Category { Id = 626, Pid = 624, Name = "Smartphone"}           },
                        { 628, new Category { Id = 628, Pid = 624, Name = "Tablette"}             },
                        { 630, new Category { Id = 630, Pid = 624, Name = "Autre"}                },
                    }}
                },
                { 392, new Category { Id = 392, Name = "GPS", Cats = new Dictionary<int, Category>
                    {
                        { 391, new Category { Id = 391, Pid = 392, Name = "Applications"}         },
                        { 393, new Category { Id = 393, Pid = 392, Name = "Cartes"}               },
                        { 394, new Category { Id = 394, Pid = 392, Name = "Divers"}               },
                    }}
                },
                { 233, new Category { Id = 233, Name = "Application", Cats = new Dictionary<int, Category>
                    {
                        { 234, new Category { Id = 234, Pid = 233, Name = "Linux"}                },
                        { 235, new Category { Id = 235, Pid = 233, Name = "MacOS"}                },
                        { 236, new Category { Id = 236, Pid = 233, Name = "Windows"}              },
                        { 625, new Category { Id = 625, Pid = 233, Name = "Smartphone"}           },
                        { 627, new Category { Id = 627, Pid = 233, Name = "Tablette"}             },
                        { 638, new Category { Id = 638, Pid = 233, Name = "Formation"}            },
                        { 629, new Category { Id = 629, Pid = 233, Name = "Autre"}                },
                    }}
                },
            };

            return Task.Factory.StartNew(() => categories);
        }

        public Task<List<TermCategory>> GetTermsAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<List<Torrent>> GetBookmarksAsync()
        {
            throw new System.NotImplementedException();
        }

        public Task<int> CreateBookmarkAsync(int id)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> DeleteBookmarkAsync(IEnumerable<int> bookmarkIds)
        {
            throw new System.NotImplementedException();
        }

        private async Task<string> GetRawResponseAsync(string uri)
        {
            using (var handler = CreateHttpClientHandler())
            {
                handler.CookieContainer = _cookieContainer;

                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Get, uri))
                using (HttpResponseMessage response = await client.SendAsync(requestMessage))
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return result;
                }
            }
        }

        private HttpClientHandler CreateHttpClientHandler()
        {
            Contract.Ensures(Contract.Result<HttpClientHandler>() != null);
            var handler = new HttpClientHandler();
            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            }
            handler.CookieContainer = _cookieContainer;
            return handler;
        }

        private void Login()
        {
            const string Uri = "/users/login/";

            using (var handler = CreateHttpClientHandler())
            {
                using (var client = new HttpClient(handler) { BaseAddress = new Uri(BaseAddress) })
                using (HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Post, Uri))
                {
                    requestMessage.Content = new FormUrlEncodedContent(new Dictionary<string, string>
                    {
                        { "login", _username },
                        { "password", _password },
                        { "remember", "1" },
                    });

                    using (var response = client.SendAsync(requestMessage).Result)
                    {
                        var result = response.Content.ReadAsStringAsync().Result ?? string.Empty;

                        string discriminator = $"<span>{_username} (<a href=\"/users/logout/\" title=\"D&#233;connexion\" class=\"logout\">D&#233;connexion</a>)</span>";

                        bool isAuthenticated = result.IndexOf(discriminator, StringComparison.OrdinalIgnoreCase) != -1;
                        if (!isAuthenticated)
                        {
                            throw new ErrorCodeException("Failed to login user");
                        }
                    }
                }
            }
        }
    }
}
