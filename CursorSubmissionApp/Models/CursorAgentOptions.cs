namespace CursorSubmissionApp.Models;

public class CursorAgentOptions
{
    public string BaseUrl { get; set; } = string.Empty;
    public string SubmitEndpoint { get; set; } = string.Empty;
    public string? ApiKey { get; set; }
}
