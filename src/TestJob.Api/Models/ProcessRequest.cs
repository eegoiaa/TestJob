namespace TestJob.Api.Models;

public class ProcessRequest
{
    public string Selector { get; init; } = string.Empty;
    public string Attribute { get; init; } = string.Empty;
    public string UrlB64 { get; init; } = string.Empty;
    public string EncryptedTextBytesB64 { get; init; } = string.Empty;
    public string KeyBytesB64 { get; init; } = string.Empty;
    public string PageB64 { get; init; } = string.Empty;
}
