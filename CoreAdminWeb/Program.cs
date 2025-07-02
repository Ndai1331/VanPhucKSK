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
builder.Services.AddRazorComponents();  // Add this line if using Blazor Server

// Add Blazored.LocalStorage before other services that depend on it
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();

builder.Services.AddServices();
builder.Services.Configure<DrCoreApi>(builder.Configuration.GetSection("DrCoreApi"));
// Configure HttpClient with base URL
builder.Services.AddHttpClient("DrCoreApi", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("DrCoreApiPublic", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApi:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient("DrCoreApiReport", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["DrCoreApiReport:BaseUrl"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Load base URL
GlobalConstant.BaseUrl = builder.Configuration["DrCoreApi:BaseUrl"] ?? "https://core.hpte.vn/";

var app = builder.Build();

var httpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("DrCoreApi");
RequestClient.Initialize(httpClient);
var publicHttpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("DrCoreApiPublic");
PublicRequestClient.Initialize(publicHttpClient);
var reportHttpClient = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient("DrCoreApiReport");
ReportRequestClient.Initialize(reportHttpClient);

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

// Add test PDF endpoint
app.MapGet("/test-pdf", async (CoreAdminWeb.Services.PDFService.IPdfService pdfService, ILogger<Program> logger) =>
{
    try
    {
        logger.LogInformation("=== Starting iText7 PDF test endpoint ===");
        
        // Medical form HTML content for iText7 testing
        var htmlContent = @"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <title>Test iText7 Medical Form</title>
    <style>
        body {
            font-family: 'Times New Roman', serif;
            font-size: 14px;
            line-height: 1.4;
            color: #000;
            margin: 20px;
        }
        .ksk-header {
            text-align: center;
            margin-bottom: 20px;
            border-bottom: 2px solid #000;
            padding-bottom: 10px;
        }
        .ksk-header .quochoa {
            font-weight: bold;
            text-transform: uppercase;
            font-size: 16px;
        }
        .ksk-header .doclap {
            font-style: italic;
            font-size: 14px;
        }
        .ksk-title {
            text-align: center;
            font-weight: bold;
            text-transform: uppercase;
            font-size: 18px;
            margin: 20px 0;
        }
        .ksk-form {
            border: 2px solid #000;
            padding: 20px;
            margin: 20px 0;
        }
        .info-table {
            width: 100%;
            border-collapse: collapse;
            margin: 15px 0;
        }
        .info-table td {
            border: 1px solid #000;
            padding: 10px;
            vertical-align: top;
        }
        .info-label {
            font-weight: bold;
            background-color: #f0f0f0;
            width: 30%;
        }
        .signature-section {
            margin-top: 40px;
            text-align: center;
        }
        .success-badge {
            background-color: #d4edda;
            color: #155724;
            padding: 10px;
            border: 1px solid #c3e6cb;
            border-radius: 5px;
            margin: 20px 0;
        }
    </style>
</head>
<body>
    <div class='ksk-header'>
        <div>Mẫu số 03</div>
        <div>MẪU SỔ KHÁM SỨC KHỎE ĐỊNH KỲ</div>
        <div class='quochoa'>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
        <div class='doclap'>Độc lập - Tự do - Hạnh phúc</div>
    </div>
    
    <div class='ksk-title'>SỔ KHÁM SỨC KHỎE ĐỊNH KỲ - TEST iTEXT7</div>
    
    <div class='success-badge'>
        <strong>✅ SUCCESS:</strong> PDF được tạo bằng iText7 - Không cần browser!
    </div>
    
    <div class='ksk-form'>
        <table class='info-table'>
            <tr>
                <td class='info-label'>1. Họ và tên:</td>
                <td>Nguyễn Văn Test iText7</td>
            </tr>
            <tr>
                <td class='info-label'>2. Giới tính:</td>
                <td>Nam</td>
            </tr>
            <tr>
                <td class='info-label'>3. Nghề nghiệp:</td>
                <td>Kỹ sư Phần mềm</td>
            </tr>
            <tr>
                <td class='info-label'>4. Nơi công tác:</td>
                <td>Công ty Công nghệ ABC</td>
            </tr>
            <tr>
                <td class='info-label'>5. Số điện thoại:</td>
                <td>0912345678</td>
            </tr>
            <tr>
                <td class='info-label'>6. Ngày khám:</td>
                <td>" + DateTime.Now.ToString("dd/MM/yyyy HH:mm") + @"</td>
            </tr>
            <tr>
                <td class='info-label'>7. Hệ thống PDF:</td>
                <td>iText7 Manual Generation</td>
            </tr>
            <tr>
                <td class='info-label'>8. Ưu điểm:</td>
                <td>Nhanh, ổn định, không cần Chrome browser</td>
            </tr>
        </table>
        
        <div style='margin-top: 20px; padding: 15px; background-color: #f8f9fa; border-left: 4px solid #007bff;'>
            <h4>🚀 Thông tin kỹ thuật:</h4>
            <ul>
                <li>✅ Hỗ trợ tiếng Việt hoàn chỉnh</li>
                <li>✅ CSS styling được áp dụng</li>
                <li>✅ Table layout hoạt động tốt</li>
                <li>✅ Fonts Times New Roman</li>
                <li>✅ Không dependency trên browser</li>
            </ul>
        </div>
    </div>
    
    <div class='signature-section'>
        <p><strong>Bác sĩ khám:</strong> __________________</p>
        <p><strong>Ngày ký:</strong> " + DateTime.Now.ToString("dd/MM/yyyy") + @"</p>
        <p style='margin-top: 30px; font-style: italic; color: #666;'>
            (PDF được tạo bằng iText7 Community - ASP.NET Core 9)
        </p>
    </div>
</body>
</html>";

        logger.LogInformation("Đang tạo PDF với iText7...");
        
        // Generate PDF using iText7
        var pdfBytes = pdfService.GeneratePdfFromHtml(htmlContent, "test-itext7-medical.pdf");
        
        logger.LogInformation("✅ PDF đã được tạo thành công với iText7!");
        logger.LogInformation($"📊 Kích thước file: {pdfBytes.Length} bytes ({pdfBytes.Length / 1024.0:F1} KB)");
        
        // Return PDF as download
        return Results.File(pdfBytes, "application/pdf", $"test-itext7-medical-{DateTime.Now:yyyyMMdd-HHmmss}.pdf");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "❌ Lỗi khi tạo PDF với iText7 từ endpoint test");
        logger.LogError($"Chi tiết lỗi: {ex}");
        
        return Results.Problem($"Lỗi khi tạo PDF với iText7: {ex.Message}");
    }
});

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();