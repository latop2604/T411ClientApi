# T411ClientApi
T411Client Api is a simple .Net client for the french torrent tracker t411 web api.

[![Build status](https://ci.appveyor.com/api/projects/status/ck0kv1sjhvjpe34t?svg=true)](https://ci.appveyor.com/project/latop2604/t411clientapi)

Download T411Client Api from CodePlex or install using [NuGet](https://ci.appveyor.com/project/latop2604/t411clientapi)

[![Nuget page](http://download-codeplex.sec.s-msft.com/Download?ProjectName=t411clientapi&DownloadId=702702)](https://www.nuget.org/packages/T411ClientApi)

## Get started
Download the first .torrent file from TopMonth list
```c#
T411Client client = new T411Client("username", "password");
var torrents = client.GetTopMonth();

var torrent = torrents.First();
var torrentDetails = client.GetTorrentDetails(torrent.Id);
using (var ms = client.DownloadTorrent(torrentDetails.Id))
using (var fs = new FileStream("plop.torrent", FileMode.Create))
{
    ms.CopyTo(fs);
}
```

## Use Html parser instead of classic api

```c#
// register parser implementation
T411.Html.DependencyResolver.Register<T411.Html.IT411HtmlParser>(() => new T411.HtmlParser.T411HtmlParser());

var client = new T411.Html.T411Client("username", "password");

```
