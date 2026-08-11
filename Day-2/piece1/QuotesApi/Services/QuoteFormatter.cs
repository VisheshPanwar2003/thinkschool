namespace QuotesApi.Services;

public class QuoteFormatter : IQuoteFormatter
{
    public string Format(string author, string text)
    {
        return $"\"{text}\" — {author}";
    }
}