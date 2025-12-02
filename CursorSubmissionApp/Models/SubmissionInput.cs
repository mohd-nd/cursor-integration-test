using System.ComponentModel.DataAnnotations;

namespace CursorSubmissionApp.Models;

public class SubmissionInput
{
    [Required]
    [Display(Name = "Prompt Message")]
    [StringLength(2000, ErrorMessage = "Prompt is too long (2000 char limit).")]
    public string PromptMessage { get; set; } = string.Empty;

    [Required]
    [Url]
    [Display(Name = "Repository URL")]
    public string RepositoryUrl { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Repository Branch")]
    [StringLength(200, ErrorMessage = "Branch name too long (200 char limit).")]
    public string Branch { get; set; } = string.Empty;
}
