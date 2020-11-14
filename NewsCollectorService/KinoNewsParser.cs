using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Web;

namespace NewsParsingUtils
{
    class KinoNewsParser : IBaseNewsParser
    {
        ScrapingBrowser web;
        List<NewsItemInfo> newsItems;
        const string sourceName = "KINONEWS";
        const string sourceUrl = "https://www.kinonews.ru/news/";
        const string category = "Кино";
        public KinoNewsParser()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            web = new ScrapingBrowser();
            web.IgnoreCookies = false;
            web.Encoding = System.Text.CodePagesEncodingProvider.Instance.GetEncoding(1251);
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
            HtmlNode node = page.Html.SelectSingleNode("//div[@class='block-page-new']");
            int count = 0;
            HtmlNodeCollection childs = node.SelectNodes(".//div[@class='relative shiftup10']");
            foreach (var child in childs)
            {
                if (count > 8)
                    break;
                string url = "https://" + new Uri(sourceUrl).Host + child.SelectSingleNode(".//h3").FirstChild.GetAttributeValue("href", "");
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
            NewsItemInfo newsItem = new NewsItemInfo();
            WebPage page;
            try
            {
                page = web.NavigateToPage(new Uri(url));
            }
            catch
            {
                return newsItem;
            }
            newsItem.SetTitle(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//h1[@class='new']").InnerText).Trim());
            newsItem.SetAnnotation(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@itemprop='articleBody']").InnerText).Trim());
            newsItem.SetNewsUrl(url);
            newsItem.SetSourceName(sourceName);
            DateTime time = DateTime.Parse(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@class='datem']").InnerText.Trim() + ":00"));
            newsItem.SetDate(time.ToString("yyyy-M-d H:mm:ss"));
            return newsItem;
        }
    }
}
