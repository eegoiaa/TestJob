using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AngleSharp;
using Dapper;
using Npgsql;
using TestJob.Api.Models;

namespace TestJob.Api.Services;

public class ProcessingService : IProcessingService
{
    private static readonly Regex s_emailRegex = new(
        @"[a-zA-Z0-9._%+\-]+@[a-zA-Z0-9.\-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private readonly string _connectionString;

    public ProcessingService(Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Postgres")
            ?? throw new InvalidOperationException("Postgres connection string is not configured.");
    }

    public async Task<ProcessResponse> ProcessAsync(ProcessRequest request, CancellationToken ct = default)
    {
        try
        {
            var url = DecodeBase64Url(request.UrlB64);
            var html = DecodeBase64Page(request.PageB64);

            var browsingContext = BrowsingContext.New(Configuration.Default);
            var document = await browsingContext.OpenAsync(req => req.Content(html), ct);

            var elements = document.QuerySelectorAll(request.Selector);
            var elementsAttrList = new List<string>(elements.Length);
            var entitiesToInsert = new List<ElementEntity>(elements.Length);

            foreach (var element in elements)
            {
                var attrValue = element.GetAttribute(request.Attribute) ?? string.Empty;
                elementsAttrList.Add(attrValue);

                entitiesToInsert.Add(new ElementEntity
                {
                    AttributeValue = attrValue,
                    HtmlContent = element.ToHtml()
                });
            }

            await SaveElementsAsync(entitiesToInsert, ct);

            var emailsList = ExtractEmails(html);
            var decryptedPlainText = DecryptText(request.EncryptedTextBytesB64, request.KeyBytesB64);

            return new ProcessResponse
            {
                IsError = 0,
                Url = url,
                ElementsCount = elements.Length,
                EmailsCount = emailsList.Count,
                DecryptedPlainText = decryptedPlainText,
                ElementsAttrList = elementsAttrList,
                EmailsList = emailsList
            };
        }
        catch (ProcessingException ex)
        {
            return ProcessResponse.Error(ex.ErrorCode, ex.Message);
        }
        catch (Exception ex)
        {
            return ProcessResponse.Error("INTERNAL_ERROR", ex.Message);
        }
    }

    private static string DecodeBase64Url(string base64)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch (FormatException ex)
        {
            throw new ProcessingException("BASE64_URL_ERROR", $"Failed to decode url_b64: {ex.Message}", ex);
        }
    }

    private static string DecodeBase64Page(string base64)
    {
        try
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch (FormatException ex)
        {
            throw new ProcessingException("BASE64_PAGE_ERROR", $"Failed to decode page_b64: {ex.Message}", ex);
        }
    }

    private static string DecryptText(string encryptedB64, string keyB64)
    {
        byte[] keyBytes;
        byte[] encryptedBytes;
        try
        {
            keyBytes = Convert.FromBase64String(keyB64);
            encryptedBytes = Convert.FromBase64String(encryptedB64);
        }
        catch (FormatException ex)
        {
            throw new ProcessingException("BASE64_CRYPTO_ERROR", $"Failed to decode crypto parameters: {ex.Message}", ex);
        }

        try
        {
            return DecryptAesEcb(encryptedBytes, keyBytes);
        }
        catch (Exception ex) when (ex is not ProcessingException)
        {
            throw new ProcessingException("DECRYPTION_ERROR", $"AES decryption failed: {ex.Message}", ex);
        }
    }

    private async Task SaveElementsAsync(List<ElementEntity> entities, CancellationToken ct)
    {
        const string sql = """
            INSERT INTO elements (attribute_value, html_content)
            VALUES (@AttributeValue, @HtmlContent)
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync(ct);
        await connection.ExecuteAsync(new CommandDefinition(sql, entities, cancellationToken: ct));
    }

    private static List<string> ExtractEmails(string html)
    {
        var matches = s_emailRegex.Matches(html);
        var emails = new List<string>(matches.Count);
        foreach (Match match in matches)
        {
            emails.Add(match.Value);
        }
        return emails;
    }

    private static string DecryptAesEcb(byte[] encryptedBytes, byte[] keyBytes)
    {
        if (encryptedBytes == null || encryptedBytes.Length == 0)
        {
            throw new ArgumentException("Encrypted bytes cannot be null or empty", nameof(encryptedBytes));
        }

        if (keyBytes == null || keyBytes.Length == 0)
        {
            throw new ArgumentException("Key bytes cannot be null or empty", nameof(keyBytes));
        }

        if (keyBytes.Length != 32)
        {
            throw new ArgumentException($"Invalid key length: {keyBytes.Length} bytes. AES-256 requires 32 bytes.", nameof(keyBytes));
        }

        using var aes = Aes.Create();
        aes.Mode = CipherMode.ECB;
        aes.Padding = PaddingMode.None;
        aes.Key = keyBytes;

        using var decryptor = aes.CreateDecryptor();
        byte[] decryptedBytes = decryptor.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);

        return Encoding.UTF8.GetString(decryptedBytes).TrimEnd('\0');
    }
}
