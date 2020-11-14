using HtmlAgilityPack;
using ScrapySharp.Network;
using System;

public struct NewsItemInfo
{
    public string title;
    public string annotation;
    public string sourceName;
    public string newsUrl;
    public string date;

    public NewsItemInfo(HtmlNode node)
    {
        this.title = string.Empty;
        this.annotation = string.Empty;
        this.sourceName = string.Empty;
        this.newsUrl = string.Empty;
        this.date = string.Empty;
    }

    public void Write()
    {
        Console.WriteLine("Title: |" + this.title + "|");
        Console.WriteLine("Annotation: |" + this.annotation + "|");
        Console.WriteLine("SourceName: |" + this.sourceName + "|");
        Console.WriteLine("NewsUrl: |" + this.newsUrl + "|");
        Console.WriteLine("Time: |" + this.date + "|");
    }

    public override string ToString()
    {
        return "NewsItemInfo[" + title + "]";
    }

    public void SetTitle(string _title)
    {
        this.title = _title;
    }

    public void SetAnnotation(string _annotation)
    {
        this.annotation = _annotation;
    }

    public void SetSourceName(string _sourceName)
    {
        this.sourceName = _sourceName;
    }

    public void SetNewsUrl(string _newsUrl)
    {
        this.newsUrl = _newsUrl;
    }

    public void SetDate(string _date)
    {
        this.date = _date;
    }

    public bool IsEmpty()
    {
        return string.IsNullOrEmpty(this.title) || string.IsNullOrEmpty(this.newsUrl) || string.IsNullOrEmpty(this.date);
    }

    public bool Equals(NewsItemInfo obj)
    {
        return this.title == obj.title && this.annotation == obj.annotation && this.newsUrl == obj.newsUrl
            && this.sourceName == obj.sourceName && this.date == obj.date;
    }
}