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
            vertical-align: top;
        }
        .ls-doctor-cell * {
            display: block !important;
            margin: 3px auto !important;
            width: 100% !important;
        }
        .ls-doctor-cell img {
            display: block !important;
            margin: 0 auto 8px auto !important;
            width: auto !important;
            max-width: 100px !important;
        }
        .ls-doctor-cell .signature-image {
            display: block !important;
            margin: 0 auto 8px auto !important;
            width: auto !important;
            max-width: 100px !important;
        }
        .signature-image {
            display: block !important;
            margin: 0 auto 5px auto !important;
        }
        .signature-text {
            display: block !important;
            margin: 5px auto !important;
        }
        .signature-container {
            display: block;
            text-align: center;
            margin: 10px 0;
        }
        .signature-container .signature-image {
            display: block;
            margin: 0 auto 5px auto;
        }
        .signature-container + div {
            margin-top: 5px;
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
        /* Force line break after signature images in table cells */
        .ls-doctor-cell .signature-image + * {
            display: block !important;
            margin-top: 10px !important;
        }
        .ls-doctor-cell br {
            display: block !important;
            height: 8px !important;
        }
        .signature-wrapper {
            display: block !important;
            margin: 0 auto 8px auto !important;
            text-align: center !important;
        }
        .signature-wrapper + * {
            display: block !important;
            margin-top: 8px !important;
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
// Helper function to get content length
window.getMedicalFormContentLength = function() {
    try {
        const printContent = document.getElementById('medical-form-content');
        if (!printContent || !printContent.innerHTML) {
            return 0;
        }
        return printContent.innerHTML.length;
    } catch (error) {
        console.error('Error getting content length:', error);
        return 0;
    }
};

// Helper function to get just innerHTML without CSS
window.getMedicalFormInnerHTML = function() {
    try {
        const printContent = document.getElementById('medical-form-content');
        if (!printContent) {
            console.error('Element not found: medical-form-content');
            return '';
        }
        return printContent.innerHTML || '';
    } catch (error) {
        console.error('Error in getMedicalFormInnerHTML:', error);
        return '';
    }
};

// Chunked version for large content
window.getMedicalFormHtmlChunk = function(chunkIndex, chunkSize) {
    try {
        const printContent = document.getElementById('medical-form-content');
        if (!printContent || !printContent.innerHTML) {
            return '';
        }
        
        const fullContent = printContent.innerHTML;
        const start = chunkIndex * chunkSize;
        const end = start + chunkSize;
        
        if (start >= fullContent.length) {
            return '';
        }
        
        // For first chunk, include HTML head
        if (chunkIndex === 0) {
            const htmlHead = `<!DOCTYPE html><html><head><title>Sổ Khám Sức Khỏe</title><meta charset="utf-8"><style>/* CSS styles here */</style></head><body>`;
            const chunk = fullContent.substring(start, end);
            return htmlHead + chunk;
        }
        
        // For last chunk, include closing tags
        const chunk = fullContent.substring(start, end);
        if (end >= fullContent.length) {
            return chunk + '</body></html>';
        }
        
        return chunk;
    } catch (error) {
        console.error('Error in getMedicalFormHtmlChunk:', error);
        return '';
    }
};

window.getMedicalFormHtml = function() {
    try 
    {
        
        const printContent = document.getElementById('medical-form-content');
        if (!printContent) {
            console.error('Element not found: medical-form-content');
            return '';
        }
        
        // Kiểm tra xem element có innerHTML không
        if (!printContent.innerHTML) {
            console.error('Element has no innerHTML');
            return '';
        }

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
                vertical-align: top;
            }
            .ls-doctor-cell * {
                display: block !important;
                margin: 3px auto !important;
                width: 100% !important;
            }
            .ls-doctor-cell img {
                display: block !important;
                margin: 0 auto 8px auto !important;
                width: auto !important;
                max-width: 100px !important;
            }
            .ls-doctor-cell .signature-image {
                display: block !important;
                margin: 0 auto 8px auto !important;
                width: auto !important;
                max-width: 100px !important;
            }
            .signature-image {
                display: block !important;
                margin: 0 auto 5px auto !important;
            }
            .signature-text {
                display: block !important;
                margin: 5px auto !important;
            }
            .signature-container {
                display: block;
                text-align: center;
                margin: 10px 0;
            }
            .signature-container .signature-image {
                display: block;
                margin: 0 auto 5px auto;
            }
            .signature-container + div {
                margin-top: 5px;
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
            /* Force line break after signature images in table cells */
            .ls-doctor-cell .signature-image + * {
                display: block !important;
                margin-top: 10px !important;
            }
            .ls-doctor-cell br {
                display: block !important;
                height: 8px !important;
            }
            .signature-wrapper {
                display: block !important;
                margin: 0 auto 8px auto !important;
                text-align: center !important;
            }
            .signature-wrapper + * {
                display: block !important;
                margin-top: 8px !important;
            }
        `;
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
                ${printContent.innerHTML}
            </body>
            </html>
        `;

        console.log('a', htmlContent);
        return htmlContent;
    } catch (error) {
        console.error('Error in getMedicalFormHtml:', error);
        return '';
    }
};
