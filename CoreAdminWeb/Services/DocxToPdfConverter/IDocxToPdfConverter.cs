namespace CoreAdminWeb.Services.DocxToPdfConverter
{
    public interface IDocxToPdfConverter
    {
        Task<byte[]> ConvertFileAsync(string inputDocxPath, CancellationToken ct = default);
    }
}
