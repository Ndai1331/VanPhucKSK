using Blazored.LocalStorage;
using CoreAdminWeb.Commons;
using CoreAdminWeb.DIInjections;
using CoreAdminWeb.Http;
using CoreAdminWeb.Hubs;
using CoreAdminWeb.Model.Configuration;
using CoreAdminWeb.Models;
using CoreAdminWeb.Services.Imports;
using CoreAdminWeb.StateService;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Configure Blazor Server with increased limits for large data transfer
builder.Services.AddServerSideBlazor(options =>
{
    // Increase timeout for long-running operations
    options.DetailedErrors = builder.Environment.IsDevelopment();
})
.AddHubOptions(options =>
{
    // Increase SignalR message size limit for large HTML content
    options.MaximumReceiveMessageSize = 100 * 1024 * 1024; // 100MB
    options.StreamBufferCapacity = 20;
    options.ClientTimeoutInterval = TimeSpan.FromMinutes(2);
    options.HandshakeTimeout = TimeSpan.FromSeconds(30);
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.MaximumParallelInvocationsPerClient = 2;

    // Enable detailed errors in development
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
});

builder.Services.AddMudServices();
builder.Services.AddSingleton<ModeStateService>();
builder.Services.AddSingleton<ImportSoKhamSucKhoeService>();

// Add Blazored.LocalStorage before other services that depend on it
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

// Load appsettings based on environment
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.AddServices();
builder.Services.Configure<DrCoreApi>(builder.Configuration.GetSection("DrCoreApi"));
builder.Services.Configure<FTPConfig>(builder.Configuration.GetSection("FTPConfig"));

// Configure HttpClient with base URL - for scoped HttpClientService (SECURE - replaces RequestClient)
builder.Services.AddHttpClient("DrCoreApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(60);
});

// Configure static clients for public/non-auth endpoints
builder.Services.AddHttpClient("DrCoreApiPublic", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"] ?? string.Empty);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Configure static client for local API endpoints
builder.Services.AddHttpClient("LocalApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["LocalApi:BaseUrl"] ?? "https://localhost:7078/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Load base URL
GlobalConstant.BaseUrl = builder.Configuration["DrCoreApi:BaseUrl"] ?? "https://core.hpte.vn/";



builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

// Add ApplicationDbContext for DRCARE_CORE database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("UserDbConnectionString"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.Configure<IISServerOptions>(options =>
{
    options.AutomaticAuthentication = false;
});
// Add CORS policy to allow any origin
builder.Services.AddCors(options =>
{
    options.AddPolicy("ExposeResponseHeaders", policy =>
    {
        policy.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
    });
});


builder.Services.AddResponseCompression(options =>
{
    options.Providers.Add<GzipCompressionProvider>();
});
builder.Services
    .AddControllers();



var app = builder.Build();

// Initialize static clients for public/non-auth endpoints (SAFE - no token sharing)
var publicHttpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("DrCoreApiPublic");
PublicRequestClient.Initialize(publicHttpClient, builder.Configuration);

// Initialize LocalRequestClientService for local API calls
var localHttpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("LocalApi");
CoreAdminWeb.Http.LocalRequestClientService.Initialize(localHttpClient, builder.Configuration["LocalApi:BaseUrl"] ?? "https://localhost:7078/api/");

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

// Add CORS middleware
app.UseCors("ExposeResponseHeaders");

app.MapBlazorHub();
app.MapRazorPages();
// Add Controllers mapping
app.MapControllers();
app.MapFallbackToPage("/_Host");
app.MapHub<ImportProgressHub>("/importProgressHub");
await app.RunAsync();