using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace T411
{
    using System.Linq;

    [System.Diagnostics.DebuggerDisplay("{Uid} / {Token}")]
    public class AuthResult
    {
        public string Uid { get; set; }
        public string Token { get; set; }
    }

    public enum Privacy
    {
        Low,
        Normal,
        Strong
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Name}")]
    public class Torrent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string RewriteName { get; set; }
        public int Seeders { get; set; }
        public int Leechers { get; set; }
        public int Comments { get; set; }
        public int IsVerified { get; set; }
        public DateTime Added { get; set; }
        public long Size { get; set; }
        [JsonProperty("times_completed")]
        public int TimesCompleted { get; set; }

        public int Owner { get; set; }
        public string CategoryName { get; set; }
        public string CategoryImage { get; set; }
        public string Username { get; set; }
        public Privacy? Privacy { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Name}")]
    public class TorrentDetails
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Category { get; set; }
        public string Categoryname { get; set; }
        public string CategoryImage { get; set; }
        public string RewriteName { get; set; }
        public int Owner { get; set; }
        public string Username { get; set; }
        public Privacy? Privacy { get; set; }
        public string Description { get; set; }
        public bool IsVerified { get; set; }
        public Dictionary<string, string> Terms { get; set; }

        public TorrentDetails()
        {
            Terms = new Dictionary<string, string>();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Query} / {Offset}:{Limit}:{Total}")]
    public class QueryResult
    {
        public string Query { get; set; }
        public int Offset { get; set; }
        public int Limit { get; set; }
        public int Total { get; set; }
        public List<Torrent> Torrents { get; set; }

        public QueryResult()
        {
            Torrents = new List<Torrent>();
        }
    }

    public enum SortOrder
    {
        Asc,
        Desc,
    }

    public enum SortColumn
    {
        Category,
        Name,
        Comments,
        Added,
        Size,
        TimesCompleted,
        Seeders,
        Leechers,
    }

    public class QueryOptions
    {

        public int Offset { get; set; }
        public int Limit { get; set; }
        public List<int> CategoryIds { get; set; }
        public List<Term> Terms { get; set; }
        public SortColumn? SortColumn { get; set; }
        public SortOrder? SortOrder { get; set; }

        public QueryOptions()
        {
            Terms = new List<Term>();
            CategoryIds = new List<int>();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Username}")]
    public class UserDetails
    {
        public string Username { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public string Avatar { get; set; }
        public long Downloaded { get; set; }
        public long Uploaded { get; set; }
    }


    [System.Diagnostics.DebuggerDisplay("{Code} / {Error}")]
    public class ErrorResult
    {
        public string Error { get; set; }
        public int Code { get; set; }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Pid} / {Name}")]
    public class Category
    {
        public int Id { get; set; }
        public int Pid { get; set; }
        public string Name { get; set; }

        public Dictionary<int,Category> Cats { get; set; }

        public Category()
        {
            Cats = new Dictionary<int, Category>();
        }
    }

    [System.Diagnostics.DebuggerDisplay("{Id} / {Type} / {Mode}")]
    public class TermType
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public Mode Mode { get; set; }
        public Dictionary<int, string> Terms { get; set; }

        public List<Term> GetTerms
        {
            get
            {
                return Terms?.Select(term => new Term { TermTypeId = Id, Id = term.Key, Name = term.Value }).ToList();
            }
        }

        public TermType()
        {
            Terms = new Dictionary<int, string>();
        }
    }

    public class Term
    {
        public int Id { get; set; }
        public int TermTypeId { get; set; }
        public string Name { get; set; }
    }

    public class TermCategory
    {
        public int Id { get; set; }
        public List<TermType> TermTypes { get; set; }

        public TermCategory()
        {
            TermTypes = new List<TermType>();
        }
    }

    public enum Mode { Single, Multi }
}
