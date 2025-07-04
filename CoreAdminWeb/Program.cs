using MudBlazor.Services;
using CoreAdminWeb.StateService;
using CoreAdminWeb.Model.Configuration;
using CoreAdminWeb.RequestHttp;
using CoreAdminWeb.DIInjections;
using CoreAdminWeb.Commons;
using Blazored.LocalStorage;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSingleton<ModeStateService>();

// Add Blazored.LocalStorage before other services that depend on it
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddServices();
builder.Services.Configure<DrCoreApi>(builder.Configuration.GetSection("DrCoreApi"));

// Configure HttpClient with base URL - for scoped HttpClientService (SECURE - replaces RequestClient)
builder.Services.AddHttpClient<CoreAdminWeb.Services.Http.HttpClientService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure static clients for public/non-auth endpoints
builder.Services.AddHttpClient("DrCoreApiPublic", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});
// Load base URL
GlobalConstant.BaseUrl = builder.Configuration["DrCoreApi:BaseUrl"] ?? "https://core.hpte.vn/";

var app = builder.Build();

// Initialize static clients for public/non-auth endpoints (SAFE - no token sharing)
var publicHttpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("DrCoreApiPublic");
PublicRequestClient.Initialize(publicHttpClient, builder.Configuration);

// NOTE: RequestClient (with auth) has been completely removed to prevent token sharing security issue

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts(); 
}
else
{
    // In development, show detailed error pages.
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapRazorPages();
app.MapFallbackToPage("/_Host");
app.Run();