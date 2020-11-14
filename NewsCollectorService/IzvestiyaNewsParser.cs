using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace NewsParsingUtils
{
    class IzvestiyaNewsParser : IBaseNewsParser
    {
        ScrapingBrowser web;
        List<NewsItemInfo> newsItems;
        const string sourceName = "Известия";
        const string sourceUrl = "https://iz.ru/feed";
        const string category = "Новости мира";
        public IzvestiyaNewsParser()
        {
            web = new ScrapingBrowser();
            web.IgnoreCookies = false;
            web.Encoding = System.Text.Encoding.UTF8;
            newsItems = new List<NewsItemInfo>();
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
            HtmlNode node = page.Html.SelectSingleNode("//div[@class='lenta_news__day']");
            int count = 0;
            HtmlNodeCollection childNodes = node.ChildNodes;
            childNodes.RemoveAt(0);
            childNodes.RemoveAt(0);
            foreach (var child in childNodes)
            {
                if (count > 8)
                    break;
                if(child.Name == "#text")
                {
                    continue;
                }
                newsItems.Add(ParseWebPage("https://" + new Uri(sourceUrl).Host + child.ChildNodes[1].GetAttributeValue("href","")));
                count++;
            }
            return true;
        }

        public NewsItemInfo ParseWebPage(string url)
        {
            WebPage page = web.NavigateToPage(new Uri(url));
            NewsItemInfo newsItem = new NewsItemInfo();
            newsItem.SetTitle(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//h1[@class='m-t-10 ']").InnerText).Trim());
            newsItem.SetAnnotation(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@itemprop='articleBody']").ChildNodes[1].ChildNodes[0].InnerText).Trim());
            newsItem.SetNewsUrl(url);
            newsItem.SetSourceName(sourceName);
            string temp = HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@class='article_page__left__top__time__label']").InnerText).Trim();
            newsItem.SetDate(GetTimeFromString(temp));
            return newsItem;
        }

        public string GetTimeFromString(string time)
        {
            time = time.Replace(",", "");
            DateTime date = DateTime.Parse(time);
            time = date.ToString("yyyy-M-d H:mm:ss");
            return time;
        }
    }
}
