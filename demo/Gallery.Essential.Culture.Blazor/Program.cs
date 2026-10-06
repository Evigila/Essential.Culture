using System.Globalization;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Essential.Culture.Demo.Blazor.Components;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

string[] uiCultures = ["en-US", "zh-CN"];
string[] formatCultures = ["en-US", "zh-CN", "pt-BR"];

using var catalogStream = typeof(Program).Assembly.GetManifestResourceStream("Demo.Texts.json")
    ?? throw new InvalidOperationException("The embedded demo catalog is missing.");
var catalog = LocalizationCatalog.Load(catalogStream);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddAntiforgery();
builder.Services.AddCultureBlazor(options => options
    .AddCatalog("Demo", catalog)
    .SetDefaultCatalog("Demo")
    .SetDefaultCulture("en-US")
    .AddSupportedCultures(uiCultures));
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("en-US");
    options.SupportedCultures = formatCultures.Select(CultureInfo.GetCultureInfo).ToArray();
    options.SupportedUICultures = uiCultures.Select(CultureInfo.GetCultureInfo).ToArray();
    options.RequestCultureProviders =
    [
        new CookieRequestCultureProvider(),
        new AcceptLanguageHeaderRequestCultureProvider()
    ];
});

var app = builder.Build();

app.UseRequestLocalization();
app.UseAntiforgery();

app.MapPost("/culture", async Task<IResult> (HttpContext context, IAntiforgery antiforgery) =>
{
    if (!context.Request.HasFormContentType)
    {
        return Results.BadRequest("A form submission is required.");
    }

    try
    {
        await antiforgery.ValidateRequestAsync(context);
    }
    catch (AntiforgeryValidationException)
    {
        return Results.BadRequest("The antiforgery token is missing or invalid.");
    }

    var form = await context.Request.ReadFormAsync();
    var uiCulture = form["uiCulture"].ToString();
    var formatCulture = form["formatCulture"].ToString();
    var returnUrl = form["returnUrl"].ToString();

    if (!uiCultures.Contains(uiCulture, StringComparer.Ordinal)
        || !formatCultures.Contains(formatCulture, StringComparer.Ordinal))
    {
        return Results.BadRequest("Select an allowed UI and formatting culture.");
    }

    if (string.IsNullOrWhiteSpace(returnUrl) || !RedirectHttpResult.IsLocalUrl(returnUrl))
    {
        return Results.BadRequest("A local return URL is required.");
    }

    context.Response.Cookies.Append(
        CookieRequestCultureProvider.DefaultCookieName,
        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(formatCulture, uiCulture)),
        new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = context.Request.IsHttps,
            IsEssential = true,
            Path = "/",
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });

    return Results.LocalRedirect(returnUrl);
});

app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();

app.Run();

public partial class Program { }
