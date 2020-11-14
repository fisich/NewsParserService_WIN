using HtmlAgilityPack;
using ScrapySharp.Network;
using System;
using System.Collections.Generic;
using System.Web;

namespace NewsParsingUtils
{
    class NJCarNewsParser : IBaseNewsParser
    {
        ScrapingBrowser web;
        List<NewsItemInfo> newsItems;
        const string sourceName = "NJCar";
        const string sourceUrl = "https://www.njcar.ru/news/";
        const string category = "Авто";
        public NJCarNewsParser()
        {
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            web = new ScrapingBrowser();
            web.IgnoreCookies = false;
            web.Encoding = System.Text.CodePagesEncodingProvider.Instance.GetEncoding(1251);
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
            HtmlNode node = page.Html.SelectSingleNode("//table[@class='feedback']");
            int count = 0;
            node.ChildNodes.RemoveAt(0);
            HtmlNodeCollection childNodes = node.ChildNodes;
            foreach (var child in node.ChildNodes)
            {
                if (count > 8)
                {
                    break;
                }
                if (count == 0)
                {
                    count++;
                    continue;
                }
                if(child.Name == "#text")
                {
                    continue;
                }
                newsItems.Add(ParseWebPage("https://" + new Uri(sourceUrl).Host + child.ChildNodes[1].ChildNodes[1].GetAttributeValue("href", "")));
                count++;
            }
            return true;
        }
        public NewsItemInfo ParseWebPage(string url)
        {
            WebPage page = web.NavigateToPage(new Uri(url));
            NewsItemInfo newsItem = new NewsItemInfo();
            HtmlNode node = page.Html.SelectSingleNode("//div[@class='page_content']");
            newsItem.SetTitle(HttpUtility.HtmlDecode(node.ChildNodes[3].InnerText).Trim());
            newsItem.SetAnnotation(HttpUtility.HtmlDecode(page.Html.SelectSingleNode("//div[@class='gray']").ChildNodes[1].InnerText).Trim());
            newsItem.SetNewsUrl(url);
            newsItem.SetSourceName(sourceName);
            string temp = HttpUtility.HtmlDecode(node.ChildNodes[5].InnerText).Trim();
            DateTime time = DateTime.Parse(temp.Substring(temp.IndexOf(",") + 1) + ":00");
            newsItem.SetDate(time.ToString("yyyy-M-d H:mm:ss"));
            return newsItem;
        }
    }
}
