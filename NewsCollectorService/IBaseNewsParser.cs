namespace NewsParsingUtils
{
    public interface IBaseNewsParser
    {
        bool StartParsing();
        NewsItemInfo[] GetNewsItems();
        string GetName();
        string GetCategory();
        string GetUrl();
    }
}