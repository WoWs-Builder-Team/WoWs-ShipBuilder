using System.Collections.Generic;

namespace WoWsShipBuilder.UI.Updater
{
    public class GithubApiResponse
    {
        public string? HtmlUrl { get; set; }

        public string? TagName { get; set; }

        public string? Name { get; set; }

        public string? Body { get; set; }

        public List<Assets>? Assets { get; set; }
    }

    public class Assets
    {
        public string? BrowserDownloadUrl { get; set; }
    }
}
