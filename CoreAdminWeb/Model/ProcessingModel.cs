using CoreAdminWeb.Enums;

namespace CoreAdminWeb.Model
{
    public class ProcessingModel
    {
        public string ProcessId { get; set; } = Guid.NewGuid().ToString();
        public object? Value { get; set; }
        public TrangThaiXuLyNen Status { get; set; } = TrangThaiXuLyNen.Processing;
        public dynamic? AdditionalParams { get; set; }
    }
}
