namespace TestJob.Api.Models;

public class ElementEntity
{
    public long Id { get; init; }
    public string AttributeValue { get; init; } = string.Empty;
    public string HtmlContent { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
