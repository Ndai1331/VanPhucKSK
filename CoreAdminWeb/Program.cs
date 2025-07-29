using MudBlazor.Services;
using CoreAdminWeb.StateService;
using CoreAdminWeb.Model.Configuration;
using CoreAdminWeb.Http;
using CoreAdminWeb.DIInjections;
using CoreAdminWeb.Commons;
using CoreAdminWeb.Models;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.ResponseCompression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddMudServices();
builder.Services.AddSingleton<ModeStateService>();

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



builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        
// Add ApplicationDbContext for DRCARE_CORE database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(Configuration.GetConnectionString("UserDbConnectionString"));
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