using CursorSubmissionApp.Services;
using CursorSubmissionApp.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.Configure<CursorAgentOptions>(builder.Configuration.GetSection("CursorAgentApi"));
builder.Services.AddHttpClient<CursorAgentClient>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();
