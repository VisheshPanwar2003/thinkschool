namespace QuotesApi.Services;

public interface IQuoteFormatter
{
    string Format(string author, string text);
}