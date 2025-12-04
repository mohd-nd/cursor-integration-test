using CursorSubmissionApp.Models;
using CursorSubmissionApp.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace CursorSubmissionApp.Pages;

public class IndexModel : PageModel
{
    private const int MaxAttachmentCount = 5;
    private const long MaxAttachmentSize = 8 * 1024 * 1024; // 8 MB per file

    private readonly CursorAgentClient _cursorAgentClient;

    public IndexModel(CursorAgentClient cursorAgentClient)
    {
        _cursorAgentClient = cursorAgentClient;
    }

    [BindProperty]
    public SubmissionInput Input { get; set; } = new();

    [BindProperty]
    public List<IFormFile> Attachments { get; set; } = new();

    public CursorAgentResponse? ApiResponse { get; private set; }

    public bool Submitted => ApiResponse is not null;

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateAttachments();

        if (!ModelState.IsValid)
        {
            return Page();
        }

        ApiResponse = await _cursorAgentClient.SubmitAsync(Input, Attachments, HttpContext.RequestAborted);
        return Page();
    }

    private void ValidateAttachments()
    {
        if (Attachments?.Count > MaxAttachmentCount)
        {
            ModelState.AddModelError(nameof(Attachments), $"You can upload up to {MaxAttachmentCount} images.");
        }

        foreach (var file in Attachments ?? Enumerable.Empty<IFormFile>())
        {
            if (file.Length > MaxAttachmentSize)
            {
                ModelState.AddModelError(nameof(Attachments), $"{file.FileName} is larger than {MaxAttachmentSize / (1024 * 1024)} MB.");
            }
        }
    }
}
