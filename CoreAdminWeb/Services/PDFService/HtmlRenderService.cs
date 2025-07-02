using System.Text;

namespace CoreAdminWeb.Services.PDFService
{
    /// <summary>
    /// HTML rendering service for PDF generation
    /// </summary>
    public class HtmlRenderService : IHtmlRenderService
    {
        /// <summary>
        /// Render medical record to HTML for PDF generation
        /// </summary>
        public string RenderMedicalRecordToPdf(MedicalRecordPdfModel model)
        {
            var html = new StringBuilder();

            html.Append(@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Sổ khám sức khỏe định kỳ</title>
    <style>
        body {
            font-family: 'Times New Roman', serif;
            font-size: 12px;
            line-height: 1.4;
            color: #000;
            background: white;
            margin: 0;
            padding: 20px;
        }
        .ksk-header {
            text-align: center;
            margin-bottom: 10px;
        }
        .ksk-header .quochoa {
            font-weight: bold;
            text-transform: uppercase;
        }
        .ksk-header .doclap {
            font-style: italic;
        }
        .ksk-header .mau-so {
            font-weight: bold;
            margin-bottom: 5px;
        }
        .ksk-title {
            text-align: center;
            font-weight: bold;
            text-transform: uppercase;
            font-size: 16px;
            margin: 10px 0;
        }
        .ksk-form {
            border: 2px solid #000;
            padding: 20px;
            margin: 0;
        }
        .ksk-row {
            display: flex;
            gap: 20px;
            margin-bottom: 15px;
        }
        .ksk-photo {
            width: 120px;
            height: 160px;
            border: 1px solid #000;
            display: flex;
            align-items: center;
            justify-content: center;
            font-size: 11px;
            text-align: center;
            flex-shrink: 0;
        }
        .ksk-info {
            flex: 1;
        }
        .ksk-info-row {
            display: flex;
            margin-bottom: 3px;
            align-items: center;
        }
        .ksk-info-label {
            min-width: 140px;
            font-weight: bold;
            font-size: 11px;
        }
        .ksk-info-value {
            flex: 1;
            border-bottom: 1px dotted #333;
            min-height: 16px;
            padding-left: 4px;
            font-size: 11px;
        }
        .ksk-note {
            font-size: 10px;
            margin: 10px 0;
        }
        .ksk-table {
            width: 100%;
            border-collapse: collapse;
            margin: 10px 0;
            font-size: 10px;
        }
        .ksk-table th, .ksk-table td {
            border: 1px solid #000;
            text-align: center;
            padding: 3px;
        }
        .ksk-table th {
            background: #f8f8f8;
            font-weight: bold;
        }
        .ksk-signature-row {
            display: flex;
            justify-content: space-between;
            margin-top: 20px;
        }
        .ksk-signature-box {
            width: 45%;
            text-align: center;
            font-size: 11px;
        }
        .section-title {
            font-weight: bold;
            margin: 15px 0 5px 0;
            font-size: 12px;
        }
        .specialty-row {
            font-style: italic;
            font-weight: normal;
        }
        .small-text {
            font-size: 10px;
        }
    </style>
</head>
<body>");

            // Header
            html.Append(@"
    <div class='ksk-header'>
        <div class='mau-so'>Mẫu số 03</div>
        <div>MẪU SỔ KHÁM SỨC KHỎE ĐỊNH KỲ</div>
        <div class='quochoa'>CỘNG HÒA XÃ HỘI CHỦ NGHĨA VIỆT NAM</div>
        <div class='doclap'>Độc lập - Tự do - Hạnh phúc</div>
        <div>-------------</div>
    </div>

    <div class='ksk-title'>SỔ KHÁM SỨC KHỎE ĐỊNH KỲ</div>

    <div class='ksk-form'>
        <!-- Patient Information -->
        <div class='ksk-row'>
            <div class='ksk-photo'>
                Ảnh 4x6 cm<br/>(dán ảnh hoặc scan ảnh)
            </div>
            <div class='ksk-info'>");

            // Patient info rows
            html.Append($@"
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>1. Họ và tên:</div>
                    <div class='ksk-info-value'>{model.PatientName}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>2. Giới tính:</div>
                    <div class='ksk-info-value'>{model.Gender}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>3. Sinh ngày:</div>
                    <div class='ksk-info-value'>{model.BirthDate}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>4. Số CCCD/CMND:</div>
                    <div class='ksk-info-value'>{model.IdNumber}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>5. Cấp ngày:</div>
                    <div class='ksk-info-value'>{model.IssuedDate}</div>
                    <span style='margin: 0 4px;'>tại</span>
                    <div class='ksk-info-value'>{model.IssuedPlace}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>6. Chỗ ở hiện tại:</div>
                    <div class='ksk-info-value'>{model.Address}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>7. Số điện thoại:</div>
                    <div class='ksk-info-value'>{model.Phone}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>8. Nghề nghiệp:</div>
                    <div class='ksk-info-value'>{model.Occupation}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>9. Nơi công tác:</div>
                    <div class='ksk-info-value'>{model.WorkPlace}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>10. Ngày vào làm:</div>
                    <div class='ksk-info-value'>{model.WorkStartDate}</div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>11. Nghề, công việc trước đây:</div>
                    <div class='ksk-info-value'></div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>12. Thời gian làm nghề:</div>
                    <div class='ksk-info-value'></div>
                    <span style='margin: 0 4px;'>tháng, từ ngày</span>
                    <div class='ksk-info-value'></div>
                    <span style='margin: 0 4px;'>đến ngày</span>
                    <div class='ksk-info-value'></div>
                </div>
                <div class='ksk-info-row'>
                    <div class='ksk-info-label'>13. Tiền sử bệnh, tật của gia đình:</div>
                    <div class='ksk-info-value'>{model.FamilyMedicalHistory}</div>
                </div>");

            html.Append(@"
            </div>
        </div>

        <!-- Note -->
        <div class='ksk-note'>
            <strong>* Lưu ý:</strong> Trường hợp đối tượng KSK có CCCD gắn chip hoặc có số định danh công dân đã thực hiện kết nối với cơ sở dữ liệu quốc gia, phần HÀNH CHÍNH nếu trên chỉ cần ghi mục (1) Họ và tên, (3) Ngày tháng năm sinh, (4) Số định danh công dân.
        </div>

        <!-- Disease History Table -->
        <table class='ksk-table'>
            <thead>
                <tr>
                    <th rowspan='2'>Tên bệnh</th>
                    <th colspan='2'>Phát hiện năm</th>
                </tr>
                <tr>
                    <th>Bệnh mãn tính</th>
                    <th>Bệnh nghề nghiệp</th>
                </tr>
            </thead>
            <tbody>
                <tr>
                    <td>Không</td>
                    <td>-</td>
                    <td>-</td>
                </tr>
            </tbody>
        </table>

        <!-- Signature Section -->
        <div class='ksk-signature-row'>
            <div class='ksk-signature-box'>
                Người lao động xác nhận<br/>
                <small>(Ký và ghi rõ họ, tên)</small><br/><br/><br/>
                <strong>");
            html.Append(model.PatientName);
            html.Append(@"</strong>
            </div>
            <div class='ksk-signature-box'>");
            html.Append($"... ngày {DateTime.Now.Day} tháng {DateTime.Now.Month} năm {DateTime.Now.Year}<br/>");
            html.Append(@"
                Người lập sổ KSK định kỳ<br/>
                <small>(Ký và ghi rõ họ, tên)</small><br/><br/><br/>
                <strong></strong>
            </div>
        </div>
    </div>");

            // Medical Examination Section
            if (model.ExamData != null || model.PhysicalData != null)
            {
                html.Append(@"
    <div class='ksk-form' style='margin-top: 20px;'>
        <div class='ksk-title'>KHÁM SỨC KHỎE ĐỊNH KỲ</div>");

                // Physical Examination
                if (model.PhysicalData != null)
                {
                    html.Append($@"
        <div class='section-title'>II. KHÁM THỂ LỰC</div>
        <div style='margin-left: 16px;'>
            <div>Chiều cao: {model.PhysicalData.Height} cm; Cân nặng: {model.PhysicalData.Weight} kg; Chỉ số BMI: {model.PhysicalData.BMI}</div>
            <div>Mạch: {model.PhysicalData.Pulse} lần/phút; Huyết áp: {model.PhysicalData.BloodPressure} mmHg</div>
            <div>Phân loại thể lực: {model.PhysicalData.Classification}</div>
        </div>");
                }

                // Laboratory Tests
                if (model.LabData != null)
                {
                    html.Append($@"
        <div class='section-title'>IV. KHÁM CẬN LÂM SÀNG</div>
        <div style='border: 1px solid #000; padding: 10px; margin: 10px 0;'>
            <div>* Xét nghiệm huyết học/sinh hóa/X.quang và các xét nghiệm khác khi có chỉ định của bác sỹ:</div>
            <div style='margin-left: 16px;'>
                <div>a) Kết quả: {model.LabData.Results}</div>
                <div>b) Đánh giá: {model.LabData.Evaluation}</div>
            </div>
        </div>");
                }

                // Conclusion
                if (model.ConclusionData != null)
                {
                    html.Append($@"
        <div class='section-title'>V. KẾT LUẬN</div>
        <div style='margin-bottom: 20px;'>
            <div><strong>1. Phân loại sức khỏe:</strong> {model.ConclusionData.HealthClassification}</div>
            <div><strong>2. Các bệnh, tật (nếu có):</strong> {model.ConclusionData.Diseases}</div>
            <div><strong>3. Đề nghị:</strong> {model.ConclusionData.Recommendations}</div>
        </div>

        <div class='ksk-signature-row'>
            <div style='flex: 1;'></div>
            <div class='ksk-signature-box'>
                <div><strong>NGƯỜI KẾT LUẬN</strong></div>
                <small>(Ký, ghi rõ họ tên và đóng dấu)</small>
                <div style='margin-bottom: 20px;'>{model.ConclusionData.ConclusionDate}</div>
                <div><strong>{model.ConclusionData.DoctorSignature}</strong></div>
                <div><strong>{model.ConclusionData.DoctorName}</strong></div>
            </div>
        </div>");
                }

                html.Append(@"
    </div>");
            }

            html.Append(@"
</body>
</html>");

            return html.ToString();
        }
    }
} 