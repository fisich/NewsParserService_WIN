using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Web;

namespace NewsParsingUtils
{
    class HabrNewsParser : IBaseNewsParser
    {
        ScrapingBrowser web;
        List<NewsItemInfo> newsItems;
        const string sourceName = "Хабр";
        const string sourceUrl = "https://habr.com/ru/news/";
        const string category = "IT";
        public HabrNewsParser()
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
            HtmlNode node = page.Html.SelectSingleNode("//ul[@class='content-list content-list_posts shortcuts_items']");
            int count = 0;
            foreach (var child in node.ChildNodes)
            {
                if (child.Name == "#text")
                {
                    continue;
                }
                if (count > 8)
                    break;
                var result = ParseNewsPanel(child);
                if (result.IsEmpty())
                {
                    continue;
                }
                newsItems.Add(result);
                count++;
            }
            return true;
        }

        public NewsItemInfo ParseNewsPanel(HtmlNode node)
        {
            NewsItemInfo newsItem = new NewsItemInfo();
            HtmlNode temp = node.SelectSingleNode(".//a[@class='post__title_link']");
            if (temp == null)
            {
                return newsItem;
            }
            newsItem.SetTitle(HttpUtility.HtmlDecode(temp.InnerText).Trim());
            temp = node.SelectSingleNode(".//div[@class='post__text post__text-html  post__text_v1 ']");
            if (temp == null)
            {
                temp = node.SelectSingleNode(".//div[@class='post__text post__text-html  post__text_v2 ']");
            }
            newsItem.SetAnnotation(HttpUtility.HtmlDecode(temp.InnerText).Trim());
            newsItem.SetNewsUrl(HttpUtility.HtmlDecode(node.SelectSingleNode(".//a[@class='post__title_link']").GetAttributeValue("href", "").Trim()));
            newsItem.SetSourceName(sourceName);
            newsItem.SetDate(GetTimeFromString(HttpUtility.HtmlDecode(node.SelectSingleNode(".//span[@class='post__time']").InnerText)));
            return newsItem;
        }

        public string GetTimeFromString(string time)
        {
            if (time.Contains("сегодня"))
            {
                time = DateTime.Now.ToString("yyyy-M-d ") + time.Substring(time.IndexOf(" в ") + 3).Trim() + ":00";
            }
            else if (time.Contains("вчера"))
            {
                time = DateTime.Now.AddDays(-1).ToString("yyyy-M-d ") + time.Substring(time.IndexOf(" в ") + 3).Trim() + ":00";
            }
            else
            {
                string[] words = time.Split(' ');
                time = DateTime.Now.Year + "-" + DateTime.ParseExact(words[1].ToString(), "MMMM", CultureInfo.CurrentCulture).Month.ToString() + "-" + words[0]
                    + " " + words[words.Length - 1] + ":00";
            }
            return time;
        }
    }
}