// Print Helper for Medical Record
window.printMedicalForm = function() {
    const printContent = document.getElementById('medical-form-content');
    if (!printContent) {
        alert('Không tìm thấy nội dung để in');
        return;
    }
    
    // Tạo window mới để in
    const printWindow = window.open('', '_blank', 'width=800,height=600');
    if (!printWindow) {
        alert('Không thể mở cửa sổ in. Vui lòng kiểm tra popup blocker.');
        return;
    }
    
    // Lấy tất cả CSS styles từ trang hiện tại
    const styles = Array.from(document.styleSheets)
        .map(styleSheet => {
            try {
                return Array.from(styleSheet.cssRules)
                    .map(rule => rule.cssText)
                    .join('');
            } catch (e) {
                console.log('Không thể truy cập CSS rule:', e);
                return '';
            }
        })
        .join('');
    
    // CSS bổ sung cho print
    const printStyles = `
        body { 
            margin: 0; 
            padding: 10px; 
            font-family: 'Times New Roman', serif;
            font-size: 14px;
            line-height: 1.4;
            color: #000;
        }
        @page {
            size: A4;
            margin: 1cm;
        }
        .ksk-form {
            page-break-inside: avoid;
        }
        .ksk-table {
            page-break-inside: avoid;
        }
        .ksk-signature-row {
            page-break-inside: avoid;
        }
    `;
    
    // Tạo HTML cho window in
    printWindow.document.write(`
        <!DOCTYPE html>
        <html>
        <head>
            <title>Sổ Khám Sức Khỏe</title>
            <meta charset="utf-8">
            <style>
                ${styles}
                ${printStyles}
            </style>
        </head>
        <body>
            ${printContent.innerHTML}
        </body>
        </html>
    `);
    
    printWindow.document.close();
    
    // In sau khi load xong
    printWindow.onload = function() {
        printWindow.focus();
        printWindow.print();
        printWindow.close();
    };
};

// Alternative method - sử dụng CSS print styles để ẩn các phần không cần thiết
window.printMedicalFormCSS = function() {
    // Thêm CSS để ẩn tất cả trừ medical form content
    const printStyle = document.createElement('style');
    printStyle.id = 'temp-print-style';
    printStyle.innerHTML = `
        @media print {
            body * {
                visibility: hidden;
            }
            
            #medical-form-content,
            #medical-form-content * {
                visibility: visible;
            }
            
            #medical-form-content {
                position: absolute;
                left: 0;
                top: 0;
                width: 100%;
                height: 100%;
            }
            
            /* Ẩn các button và navigation khi in */
            .flex.justify-between.items-center.mb-6,
            button,
            .bg-red-100,
            .animate-spin {
                display: none !important;
            }
        }
    `;
    
    document.head.appendChild(printStyle);
    
    // Print
    window.print();
    
    // Xóa style sau khi in
    setTimeout(() => {
        const tempStyle = document.getElementById('temp-print-style');
        if (tempStyle) {
            tempStyle.remove();
        }
    }, 1000);
}; 