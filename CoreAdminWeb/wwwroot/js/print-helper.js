// Print Helper for Medical Record
window.printMedicalForm = function() {
    const printContent = document.getElementById('medical-form-content');
    if (!printContent) {
        alert('Không tìm thấy nội dung để in');
        console.log('Element not found: medical-form-content');
        return;
    }
    
    console.log('Found medical form content:', printContent);
    
    // Tạo window mới để in
    const printWindow = window.open('', '_blank', 'width=800,height=600');
    if (!printWindow) {
        alert('Không thể mở cửa sổ in. Vui lòng kiểm tra popup blocker.');
        return;
    }
    
    // CSS đơn giản cho print - không lấy từ stylesheet để tránh lỗi
    const printStyles = `
        body { 
            margin: 20px; 
            padding: 0; 
            font-family: 'Times New Roman', serif;
            font-size: 14px;
            line-height: 1.4;
            color: #000;
            background: #fff;
        }
        @page {
            size: A4;
            margin: 1cm;
        }
        .ksk-header-container {
            max-width: 900px;
            margin: 0 auto;
            display: flex;
            flex-direction: row;
            align-items: center;
            margin-bottom: 10px;
            gap: 15px;
        }
        .ksk-header-logo {
            flex-shrink: 0;
        }
        .header-logo {
            width: 150px;
            height: 150px;
            object-fit: contain;
            margin-right: 10px;
        }
        .ksk-header {
            flex: 1;
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
            font-size: 18px;
            font-weight: bold;
            margin: 20px 0;
        }
        .ksk-form {
            width: 100%;
            margin-bottom: 30px;
        }
        .ksk-row {
            display: flex;
            margin-bottom: 20px;
        }
        .ksk-photo {
            width: 120px;
            height: 160px;
            border: 1px solid #000;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 20px;
            flex-shrink: 0;
        }
        .ksk-info {
            flex: 1;
        }
        .ksk-info-row {
            display: flex;
            margin-bottom: 8px;
            align-items: center;
        }
        .ksk-info-label {
            font-weight: bold;
            margin-right: 8px;
            white-space: nowrap;
        }
        .ksk-info-value {
            border-bottom: 1px solid #000;
            min-width: 100px;
            padding: 2px 4px;
            margin-right: 4px;
        }
        .ksk-note {
            font-style: italic;
            margin: 20px 0;
            padding: 10px;
            border: 1px solid #000;
        }
        .ksk-table-wrap {
            margin: 20px 0;
        }
        .ksk-table {
            width: 100%;
            border-collapse: collapse;
            border: 1px solid #000;
        }
        .ksk-table th,
        .ksk-table td {
            border: 1px solid #000;
            padding: 8px;
            text-align: center;
            vertical-align: middle;
        }
        .ksk-table th {
            background-color: #f5f5f5;
            font-weight: bold;
        }
        .ls-specialty {
            text-align: left;
            font-weight: bold;
        }
        .ls-label {
            text-align: right;
            padding-right: 10px;
        }
        .ls-cell-small {
            text-align: left;
            padding: 6px;
        }
        .ls-doctor-cell {
            text-align: center;
            min-width: 100px;
        }
        .ksk-signature-row {
            display: flex;
            justify-content: space-between;
            margin: 0;
        }
        .ksk-signature-box {
            text-align: center;
            width: 45%;
            border: none;
            padding: 10px;
            min-height: 100px;
        }
        .font-bold {
            font-weight: bold;
        }
        .text-center {
            text-align: center;
        }
        .mb-2, .mb-4, .mb-6 {
            margin-bottom: 8px;
        }
        .ml-4 {
            margin-left: 16px;
        }
        .text-sm {
            font-size: 12px;
        }
    `;
    
    // Tạo HTML cho window in
    const htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>Sổ Khám Sức Khỏe</title>
            <meta charset="utf-8">
            <style>
                ${printStyles}
            </style>
        </head>
        <body>
            ${printContent.innerHTML}
        </body>
        </html>
    `;
    
    console.log('Print HTML content length:', htmlContent.length);
    
    printWindow.document.write(htmlContent);
    printWindow.document.close();
    
    // In sau khi load xong
    printWindow.onload = function() {
        printWindow.focus();
        setTimeout(() => {
            printWindow.print();
            printWindow.close();
        }, 500);
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

// Function để lấy HTML content cho PDF export
window.getMedicalFormHtml = function() {
    try {
        console.log('getMedicalFormHtml called');
        
        const printContent = document.getElementById('medical-form-content');
        if (!printContent) {
            console.error('Element not found: medical-form-content');
            return '';
        }
        
        console.log('Print content found:', printContent);
        
        // Kiểm tra xem element có innerHTML không
        if (!printContent.innerHTML) {
            console.error('Element has no innerHTML');
            return '';
        }

        // Clone element để tối ưu hóa ảnh base64
        const clonedContent = printContent.cloneNode(true);
        
        // Tối ưu hóa ảnh base64 đơn giản - thay thế ảnh lớn bằng placeholder
        const images = clonedContent.querySelectorAll('img.signature-image');
        console.log('Found signature images:', images.length);
        
        images.forEach((img, index) => {
            try {
                if (img.src && img.src.startsWith('data:image') && img.src.length > 50000) {
                    // Nếu ảnh quá lớn (>50KB), thay thế bằng placeholder đơn giản
                    console.log(`Replacing large image ${index + 1} (${img.src.length} chars)`);
                    img.src = 'data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMTAwIiBoZWlnaHQ9IjUwIiB2aWV3Qm94PSIwIDAgMTAwIDUwIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxyZWN0IHdpZHRoPSIxMDAiIGhlaWdodD0iNTAiIGZpbGw9IiNmOGY5ZmEiIHN0cm9rZT0iI2RlZTJlNiIgc3Ryb2tlLXdpZHRoPSIxIi8+PHRleHQgeD0iNTAiIHk9IjMwIiB0ZXh0LWFuY2hvcj0ibWlkZGxlIiBmaWxsPSIjNmM3NTdkIiBmb250LXNpemU9IjEyIj5DaMOqIGvDvTwvdGV4dD48L3N2Zz4=';
                } else if (img.src && img.src.startsWith('data:image')) {
                    console.log(`Keeping image ${index + 1} (${img.src.length} chars)`);
                }
            } catch (error) {
                console.warn(`Error processing image ${index}:`, error);
            }
        });

        console.log('Images optimization completed');
    // CSS cho PDF - tương tự như print styles
    const pdfStyles = `
        body { 
            margin: 20px; 
            padding: 0; 
            font-family: 'Times New Roman', serif;
            font-size: 14px;
            line-height: 1.4;
            color: #000;
            background: #fff;
        }
        @page {
            size: A4;
            margin: 1cm;
        }
        .ksk-header-container {
            max-width: 900px;
            margin: 0 auto;
            display: flex;
            flex-direction: row;
            align-items: center;
            margin-bottom: 10px;
            gap: 15px;
        }
        .ksk-header-logo {
            flex-shrink: 0;
        }
        .header-logo {
            width: 150px;
            height: 150px;
            object-fit: contain;
            margin-right: 10px;
        }
        .ksk-header {
            flex: 1;
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
            font-size: 18px;
            font-weight: bold;
            margin: 20px 0;
        }
        .ksk-form {
            width: 100%;
            margin-bottom: 30px;
        }
        .ksk-row {
            display: flex;
            margin-bottom: 20px;
        }
        .ksk-photo {
            width: 120px;
            height: 160px;
            border: 1px solid #000;
            display: flex;
            align-items: center;
            justify-content: center;
            margin-right: 20px;
            flex-shrink: 0;
        }
        .ksk-info {
            flex: 1;
        }
        .ksk-info-row {
            display: flex;
            margin-bottom: 8px;
            align-items: center;
        }
        .ksk-info-label {
            font-weight: bold;
            margin-right: 8px;
            white-space: nowrap;
        }
        .ksk-info-value {
            border-bottom: 1px solid #000;
            min-width: 100px;
            padding: 2px 4px;
            margin-right: 4px;
        }
        .ksk-note {
            font-style: italic;
            margin: 20px 0;
            padding: 10px;
            border: 1px solid #000;
        }
        .ksk-table-wrap {
            margin: 20px 0;
        }
        .ksk-table {
            width: 100%;
            border-collapse: collapse;
            border: 1px solid #000;
        }
        .ksk-table th,
        .ksk-table td {
            border: 1px solid #000;
            padding: 8px;
            text-align: center;
            vertical-align: middle;
        }
        .ksk-table th {
            background-color: #f5f5f5;
            font-weight: bold;
        }
        .ls-specialty {
            text-align: left;
            font-weight: bold;
        }
        .ls-label {
            text-align: right;
            padding-right: 10px;
        }
        .ls-cell-small {
            text-align: left;
            padding: 6px;
        }
        .ls-doctor-cell {
            text-align: center;
            min-width: 100px;
        }
        .ksk-signature-row {
            display: flex;
            justify-content: space-between;
            margin: 0px;
        }
        .ksk-signature-box {
            text-align: center;
            width: 45%;
            border: none;
            padding: 10px;
            min-height: 100px;
        }
        .font-bold {
            font-weight: bold;
        }
        .text-center {
            text-align: center;
        }
        .mb-2, .mb-4, .mb-6 {
            margin-bottom: 8px;
        }
        .ml-4 {
            margin-left: 16px;
        }
        .text-sm {
            font-size: 12px;
        }
    `;
    
    // Tạo HTML hoàn chỉnh cho PDF
    const htmlContent = `
        <!DOCTYPE html>
        <html>
        <head>
            <title>Sổ Khám Sức Khỏe</title>
            <meta charset="utf-8">
            <style>
                ${pdfStyles}
            </style>
        </head>
        <body>
            ${clonedContent.innerHTML}
        </body>
        </html>
    `;
        console.log('HTML content length:', htmlContent.length);
        console.log('Optimized HTML with', images.length, 'signature images');
        console.log('Optimized htmlContent', htmlContent);
        return htmlContent;
    } catch (error) {
        console.error('Error in getMedicalFormHtml:', error);
        return '';
    }
};

// // Function để download file
// window.downloadFile = function(dataUrl, filename) {
//     try {
//         console.log('downloadFile called with filename:', filename);
//         console.log('Data URL length:', dataUrl.length);
        
//         // Tạo element anchor để download
//         const link = document.createElement('a');
//         link.href = dataUrl;
//         link.download = filename;
//         link.style.display = 'none';
        
//         // Thêm vào DOM, click và remove
//         document.body.appendChild(link);
//         link.click();
//         document.body.removeChild(link);
        
//         console.log('Download triggered successfully');
//     } catch (error) {
//         console.error('Error in downloadFile:', error);
//         alert('Lỗi khi tải file: ' + error.message);
//     }
// }; 