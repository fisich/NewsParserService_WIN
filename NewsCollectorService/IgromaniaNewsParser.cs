using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Web;

namespace NewsParsingUtils
{
    class IgromaniaNewsParser : IBaseNewsParser
    {
        ScrapingBrowser web;
        List<NewsItemInfo> newsItems;
        const string sourceName = "Игромания";
        const string sourceUrl = "https://www.igromania.ru/news/game/";
        const string category = "Игровая индустрия";
        public IgromaniaNewsParser()
        {
            web = new ScrapingBrowser();
            web.IgnoreCookies = false;
            web.Encoding = System.Text.Encoding.UTF8;
            newsItems = new List<NewsItemInfo>();
        }

        public bool StartParsing()
        {
            newsItems.Clear();
            WebPage page;
            try
            {
                page = web.NavigateToPage(new Uri(sourceUrl));
            }
            catch
            {
                return false;
            }
            HtmlNode node = page.Html.SelectSingleNode("//div[@class='aubl_cont']");
            int count = 0;
            foreach (var child in node.ChildNodes)
            {
                if (count > 8)
                    break;
                string url = "https://" + new Uri(sourceUrl).Host + child.SelectSingleNode(".//a[@class='aubli_img']").GetAttributeValue("href", "");
                newsItems.Add(ParseWebPage(url));
                count++;
            }
            return true;
        }

        public NewsItemInfo[] GetNewsItems()
        {
            return newsItems.ToArray();
        }

        public string GetName()
        {
            return sourceName;
        }

        public string GetCategory()
        {
            return category;
        }

        public string GetUrl()
        {
            return sourceUrl;
        }

        public NewsItemInfo ParseWebPage(string url)
        {
            WebPage page = web.NavigateToPage(new Uri(url));
            NewsItemInfo newsItem = new NewsItemInfo();
            newsItem.SetTitle(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//h1[@class='page_news_ttl haveselect']").InnerText.Replace("|", "")).Trim());
            newsItem.SetAnnotation(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@class='universal_content clearfix']").FirstChild.InnerText).Trim());
            newsItem.SetNewsUrl(url);
            newsItem.SetSourceName(sourceName);
            DateTime time = DateTime.Parse(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@class='page_news_info clearfix']").ChildNodes[1].InnerText.Replace("|", "").Trim() + ":00"));
            newsItem.SetDate(time.ToString("yyyy-M-d H:mm:ss"));
            return newsItem;
        }
    }
}
