# Blazor adaptation and demo

The first adapter version targets SSR and Interactive Server. Browser WebAssembly execution has not been validated; its ASP.NET Core framework reference does not fit a standalone client dependency graph. The optional Blazor adapter uses immutable catalogs with a localization service scoped to an HTTP request or an Interactive Server circuit. Desktop adapters and the legacy static facade keep their existing behavior. Web hosts own HTTP culture negotiation, cookies, and navigation; the adapter owns the scope's text selection and refresh notification.

## Implementation boundaries

- [Core catalog](../../src/Essential.Culture/Essential.Culture/LocalizationCatalog.cs) parses a stream, JSON string, or explicitly requested file into an immutable catalog. It has no Blazor dependency.
- [Blazor registration](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizationServiceCollectionExtensions.cs) stores completed configuration and catalogs once, then registers the Blazor [ILocalizationService](../../src/Essential.Culture/Essential.Culture.Blazor/ILocalizationService.cs) as scoped.
- [LocalizedComponentBase](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedComponentBase.cs) subscribes to that scope's Changed event, schedules rendering through InvokeAsync, and unsubscribes on disposal. Derived initialization overrides must call the base implementation.
- [LocalizedText](../../src/Essential.Culture/Essential.Culture.Blazor/LocalizedText.cs) accepts Key, optional CatalogId, Arguments, and literal Fallback. Its output is encoded text.
- [The independent demo](../../demo/Gallery.Essential.Culture.Blazor/Gallery.Essential.Culture.Blazor.csproj) defaults to the adapter and generator NuGet packages. Explicit -p:UseLocalCulture=true selects source projects for development. It uses the .NET 10 Web SDK's ASP.NET Core framework reference and adds no new fonts, scripts or icon dependencies. See [NuGet release and integration](nuget-release-integration.md).

Application and feature text belongs in the host's catalogs. Use separate catalog identities for separate text owners; do not add a host's business vocabulary to the adapter. The demo registers its own catalog as Demo.

## Register catalogs at startup

The [demo Program](../../demo/Gallery.Essential.Culture.Blazor/Program.cs) reads its own embedded Culture.json using the logical resource name Demo.Texts.json:

    using var stream = typeof(Program).Assembly
        .GetManifestResourceStream("Demo.Texts.json")
        ?? throw new InvalidOperationException("The embedded demo catalog is missing.");
    var catalog = LocalizationCatalog.Load(stream);

    builder.Services.AddCultureBlazor(options => options
        .AddCatalog("Demo", catalog)
        .SetDefaultCatalog("Demo")
        .SetDefaultCulture("en-US")
        .AddSupportedCultures("en-US", "zh-CN"));

LocalizationCatalog.FromJson also supports an in-memory document. AddCatalog identities are case-sensitive and unique. Every catalog entry must have the same language set, contain the catalog's fallback culture, and preserve composite-format placeholder indices across translations.

Without an explicit initializer, the adapter takes the current request/circuit's ambient CurrentUICulture and CurrentCulture at scope creation. Unsupported ambient UI cultures fall back to the configured default; an invariant ambient format culture uses the default format culture. The demo deliberately uses this default after standard RequestLocalization middleware.

A host can instead use InitializeWith(Func<IServiceProvider, LocalizationSelection>) to initialize each scope from its own selection service before rendering. An explicit initializer must provide a supported UI culture and a valid named formatting culture. Avoid resolving mutable user selection once at startup or keeping it in a singleton.

## Render and switch within a circuit

The [Home page](../../demo/Gallery.Essential.Culture.Blazor/Components/Pages/Home.razor) inherits LocalizedComponentBase and renders both direct Parse results and LocalizedText:

    @using TextKey = ArkheideSystem.Essential.Culture.Demo.Blazor.Texts.Key
    @inherits LocalizedComponentBase

    <h1>@Localization.Parse(TextKey.App_Title)</h1>
    <LocalizedText Key="@TextKey.Format_Sample"
                   CatalogId="Demo"
                   Arguments="@formatArguments" />

    @code {
        private readonly object?[] formatArguments =
            [12345.67m, new DateTime(2026, 10, 4)];

        private void UseChinese() => Localization.SetCulture("zh-CN");
        private void UseSeparateFormats() =>
            Localization.SetCulture("zh-CN", "pt-BR");
    }

Raw keys and generated Key.* tokens are both accepted. The demo explicitly references the existing Generator source project as an analyzer and passes its embedded catalog as an AdditionalFile. CompilerVisibleProperty exposes the generator configuration, and the generated Key class lives in the independent ArkheideSystem.Essential.Culture.Demo.Blazor.Texts namespace. Its TextKey alias is used throughout the page; no XAML helpers are generated. Parse uses the default catalog; ParseFrom selects a named catalog. TryParse/TryParseFrom distinguish a missing key without throwing for absence. A valid missing key is returned unchanged by Parse. Localization.SetCulture changes only the current service scope, raises Changed when the selection changes, and does not write a cookie or modify process-wide ambient cultures. Omitting the format culture makes it follow the selected UI culture.

The demo's Chinese/Brazilian button proves that translated wording and composite number/date formatting can differ. The fixed amount and date are sample inputs, not operational data.

## HTTP initialization and persistent selection

The demo configures standard RequestLocalizationOptions before endpoint execution:

- UI language allowlist: en-US and zh-CN.
- Formatting allowlist: en-US, zh-CN, and pt-BR.
- Default: en-US.
- Providers, in order: CookieRequestCultureProvider and AcceptLanguageHeaderRequestCultureProvider. Query-string culture selection is not enabled.

The initial static [App document](../../demo/Gallery.Essential.Culture.Blazor/Components/App.razor) sets html lang from the request's scoped service before the first interactive render. Home also sets main lang from the live circuit selection. The outer static html attribute is refreshed on a full HTTP reload; circuit-only switches update main and the visible text without rewriting the outer static document.

The ordinary POST form at /culture disables enhanced navigation so the redirect performs a full reload and starts a new circuit. It includes AntiforgeryToken, uiCulture, formatCulture, and a local returnUrl. The endpoint:

1. Requires a form request and validates IAntiforgery before reading and applying selections.
2. Rejects cultures outside the explicit UI/format allowlists.
3. Uses the framework's RedirectHttpResult.IsLocalUrl check and Results.LocalRedirect; absolute and network-path return URLs are rejected.
4. Writes CookieRequestCultureProvider.DefaultCookieName using its standard MakeCookieValue and a RequestCulture with independent formatting/UI values.
5. Uses an HTTP-only, SameSite=Lax, one-year cookie, with Secure enabled for HTTPS requests, then redirects to the local page.

The demo runs at the site's root path. Deploying under a path base requires adjusting the base URL, form/return paths, and cookie path as host configuration.

Cookie persistence is host behavior. The adapter does not expose HttpContext or browser-cookie access as part of its localization service. Existing circuits retain their own live state after another tab saves a cookie. New requests in the same browser profile share the new cookie; separate browser profiles have separate cookies.

## Catalog packaging and running

The demo's [Culture.json](../../demo/Gallery.Essential.Culture.Blazor/Culture.json) is independent of demo/Culture.json used by desktop examples. Its project removes the file from Content/None and embeds it as Demo.Texts.json. EssentialCultureAutoInclude and EssentialCultureAutoCreate are false. EssentialCultureGeneratorEnabled is true and EssentialCultureXamlFramework is none, enabling token generation without automatic file creation or copying. No external Culture.json is copied or published, and no shared desktop catalog is linked into the web demo.

From the repository root with the .NET 10 SDK installed:

    dotnet run --project demo/Gallery.Essential.Culture.Blazor

The http launch profile uses http://localhost:5196 and does not open a browser automatically. Stop the host with Ctrl+C. This command runs only the web demo; the full demo solution also contains platform-specific desktop examples.

## Manual verification checklist

1. Start with no request-localization cookie. Request the page with an en-US or zh-CN browser preference and inspect the response source: initial html lang, title, greeting, and form selection should agree with the negotiated UI culture. A request with neither supported preference nor a cookie should use en-US.
2. Click English and Chinese. The heading, PageTitle, direct Parse text, LocalizedText samples, and main lang should refresh without a document reload. The amount/date should follow the selected formatting culture.
3. Click Chinese text + Brazilian formats. Confirm Chinese wording and zh-CN UI selection remain, while the format selection is pt-BR and the fixed amount uses Brazilian punctuation. The outer html lang remains its initial request value until a full reload.
4. Reload after a circuit-only change. Confirm it restores the cookie/header request selection rather than persisting that circuit-only choice.
5. Submit the persistence form with zh-CN UI and pt-BR formatting. Confirm a full document reload, a standard .AspNetCore.Culture cookie, initial html lang="zh-CN", Chinese text, and Brazilian formats. Repeat with English and reload again to verify persistence.
6. Open two separate browser profiles or a regular/private window. Switch the live culture in one and confirm the other's current circuit and cookie are unaffected. In two tabs of one profile, saving a cookie should affect a later reload in the other tab while leaving its existing circuit unchanged.
7. Using browser developer tools or an HTTP client with a valid antiforgery cookie/token, submit an unsupported uiCulture or formatCulture and an absolute/network-path returnUrl. Each must return 400 without changing the localization cookie or redirecting externally.
8. Omit or corrupt the antiforgery token, retaining valid cultures. The POST must return 400 and must not write the localization cookie. GET /culture must not change the selection.
9. Publish the demo and inspect output: its translations should be embedded in the application assembly, with no loose Culture.json, no linked desktop Culture.json, and normal app.css/static Blazor assets present.
10. In a component used for adapter validation, exercise a missing key with LocalizedText.Fallback, switch cultures repeatedly, then dispose the component. Literal fallback should remain encoded, and disposed components should release their Changed subscription.

## Verification status

Verified from the modified source on 2026-10-04:

- Culture solution Release build (now opened through Essential.slnx) with TreatWarningsAsErrors=true: zero warnings and errors.
- All 89 tests passed: 53 Core/desktop binding tests, 21 Blazor tests, 11 Generator tests and 4 WinUI marker tests. The markers do not exercise real WinUI windows.
- Blazor tests cover independent concurrent scopes, same tokens in separate catalogs, independent formatting, allowed/custom cultures, immutable startup configuration, encoded output, renderer refresh from background notifications, and queued refresh/disposal cleanup.
- All five demos built with UseLocalCulture=true and zero warnings/errors. This selects the modified source while version 1.3.0 remains unpublished.
- The Blazor demo was published and tested in Production using an installed Edge browser in headless automation, without Computer Use. Fresh browser contexts verified initial negotiated SSR language, separate live circuits, translated page titles/content, independent Brazilian formatting, Cookie persistence after reload, cross-profile isolation, and existing-tab isolation until its next reload. No page exceptions occurred.
- HTTP tests rejected unsupported UI/format selections, absolute/network-path redirects, and missing antiforgery tokens with status 400 and no localization Cookie writes.
- Six version 1.3.0 NuGet packages were created, including Arkheide.Essential.Culture.Blazor. A consumer using only PackageReference and the .NET 10 ASP.NET Core framework resolved independent scoped translations from the packaged API.
- No loose Culture.json exists in the web build/publish output. The demo catalog remains embedded. Existing README and human docs were not changed.

An initial verification launch incorrectly ran an unpublished build DLL in Production, where development static web assets were unavailable. That host was stopped and the real published output was tested successfully. Normal development uses the supplied dotnet run profile; Production deployment uses published output.

VersionPrefix is 1.3.0, the new project is included in the library and demo solution entries, and CI package validation/publishing lists include the sixth package. No Git commit, remote push or NuGet publication was performed. The adapter contains no reference to Flourish or a business application. Its current supported host modes are SSR and Interactive Server; WebAssembly/Auto need a separately verified client-compatible dependency graph.

## Repository migration on 2026-10-05

The checkout is now Essential and the six Culture libraries are grouped under src/Essential.Culture. Root/module/test/demo references and CI were updated without changing translation identities or runtime behavior. Existing human guides were moved into the module and their Demo/license links adjusted. The 89 tests, five demos and six packages passed again after migration. See [solution organization](solution-organization.md) and [directory tree](1_architecture.md).
