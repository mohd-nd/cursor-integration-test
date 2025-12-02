using CursorSubmissionApp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.IO;
using System.Net.Http.Headers;

namespace CursorSubmissionApp.Services;

public class CursorAgentClient
{
    private const string ApiKeyHeader = "X-API-Key";

    private readonly HttpClient _httpClient;
    private readonly CursorAgentOptions _options;
    private readonly ILogger<CursorAgentClient> _logger;

    public CursorAgentClient(HttpClient httpClient, IOptions<CursorAgentOptions> options, ILogger<CursorAgentClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<CursorAgentResponse> SubmitAsync(SubmissionInput input, IEnumerable<IFormFile> attachments, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.BaseUrl) || string.IsNullOrWhiteSpace(_options.SubmitEndpoint))
        {
            return new CursorAgentResponse(false, "Cursor Cloud Agent API details are not configured yet.");
        }

        var endpoint = new Uri(new Uri(_options.BaseUrl), _options.SubmitEndpoint);
        using var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        using var content = new MultipartFormDataContent();

        content.Add(new StringContent(input.PromptMessage), "prompt");
        content.Add(new StringContent(input.RepositoryUrl), "repositoryUrl");
        content.Add(new StringContent(input.Branch), "branch");

        foreach (var file in attachments ?? Array.Empty<IFormFile>())
        {
            if (file.Length == 0)
            {
                continue;
            }

            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;
            var fileContent = new StreamContent(memoryStream);
            fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType ?? "application/octet-stream");
            content.Add(fileContent, "attachments", file.FileName);
        }

        request.Content = content;

        if (!string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            request.Headers.TryAddWithoutValidation(ApiKeyHeader, _options.ApiKey);
        }

        try
        {
            var response = await _httpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync();
            var message = response.IsSuccessStatusCode
                ? "Request accepted by Cursor Cloud Agent API."
                : $"Cursor Cloud Agent API rejected the request ({(int)response.StatusCode}).";

            if (!string.IsNullOrWhiteSpace(responseBody))
            {
                message += $" Details: {responseBody}";
            }

            return new CursorAgentResponse(response.IsSuccessStatusCode, message);
        }
        catch (TaskCanceledException ex) when (cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Cursor Cloud Agent submission cancelled by client.");
            return new CursorAgentResponse(false, "Submission cancelled.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cursor Cloud Agent submission failed.");
            return new CursorAgentResponse(false, "Failed to contact Cursor Cloud Agent API. Check server logs for details.");
        }
    }
}
