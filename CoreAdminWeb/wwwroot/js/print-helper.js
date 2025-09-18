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
        * {
				box-sizing: border-box;
			}
			html,
			body {
				height: 100%;
			}
			body {
				margin: 0;
				font-family: Arial, Helvetica, sans-serif;
				font-size: 10pt;
				color: #111;
				-webkit-print-color-adjust: exact;
				print-color-adjust: exact;
			}
			@page {
				size: A4;
				margin: 0;
			}
			.page {
				width: 190mm; /* A4 width (210) - default margins (10 each side) */
				min-height: 267mm; /* A4 height (297) - default margins (10 top/bottom) */
				margin: 15mm auto;
				page-break-after: always;
			}
			.mb-2 {
				margin-bottom: 0.5rem;
			}
			.mb-5 {
				margin-bottom: 2rem;
			}
			.mt-0 {
				margin-top: 0rem;
			}
			.mt-1 {
				margin-top: 0.25rem;
			}
			.mt-2 {
				margin-top: 0.5rem;
			}
			.mt-3 {
				margin-top: 1rem;
			}
			.ml-2 {
				margin-left: 0.5rem;
			}
			.right {
				text-align: right;
			}
			.center {
				text-align: center;
			}
			.bold {
				font-weight: bold;
			}
			.italic {
				font-style: italic;
			}
			.nowrap {
				white-space: nowrap;
			}
			.v-middle {
				vertical-align: middle;
			}

			.header {
				text-align: center;
				margin-bottom: 6px;
			}
			.header .line1 {
				font-weight: bold;
				text-transform: uppercase;
			}
			.header .line2 {
				font-weight: bold;
			}
			.header .underline {
				width: 120px;
				height: 1px;
				background: #111;
				margin: 2px auto 0;
			}
			.title {
				text-align: center;
				font-weight: bold;
				text-transform: uppercase;
			}

			.two-col-table {
				width: 100%;
				border-collapse: collapse;
				table-layout: fixed;
				border: none;
			}
			.two-col-table td {
				vertical-align: top;
				border: none;
			}
			.two-col-left {
				width: 35mm;
			}
			.photo {
				border: 1px solid #111;
				width: 30mm;
				height: 40mm;
				margin: 0 auto;
				text-align: center;
				padding: 3px;
				display: table;
			}
			.photo span {
				display: table-cell;
				vertical-align: middle;
			}

			.row {
				display: block;
				margin: 3px 0;
				margin-bottom: 0.25rem;
			}
			.inline {
				display: inline-block;
			}

			.cb {
				display: inline-block;
				width: 13px;
				height: 13px;
				border: 1px solid #111;
				vertical-align: middle;
				margin: 0 3px;
			}

			.cb.checked::after,
			.cb[checked]::after {
				content: "\\00D7"; /* dấu tick */
				font-size: 20px;
				line-height: 13px;
				text-align: center;
				display: block;
				color: #000;
				font-weight: bold;
			}

			table {
				width: 100%;
				border-collapse: collapse;
				margin-top: 6px;
			}
      table.auto {
        width: auto;
        display: inline-table;
      }
			th,
			td {
				border: 1px solid #222;
				padding: 4px 6px;
				vertical-align: top;
			}
			th {
				text-align: center;
				font-weight: bold;
			}
			.td-tight td {
				padding-top: 2px;
				padding-bottom: 2px;
			}
      .outer-table.no-border td,
      .outer-table.no-border th,
      .no-border {
				border: none;
			}

			.signature-box {
				text-align: center;
			}

			.dotted-ruled {
				display: inline-block;
				border-bottom: 1px dotted currentColor;
				line-height: 1em;
				height: 1em;
				vertical-align: baseline;
				min-width: 5px;
				width: auto;
				padding-left: 0.35rem;
			}
			.dotted-ruled::after {
				content: "\\200b";
			}
			.dot-wrap {
				display: inline-block;
			}
			.dot-wrap .dotted-ruled {
				width: 100%;
			}

			.dotted-ruled sup {
				position: relative;
				top: -0.4em;
				font-size: 0.75em;
				line-height: 1;
			}

			.gap {
				min-width: 4mm;
				display: inline-block;
			}

			.short-line {
				width: 150px;
				border: none;
				border-top: 1px solid #000;
				margin: 4px 0;
			}

			tr,
			td,
			th {
				page-break-inside: avoid;
			}
			.fixed-width {
				 width: 150px;
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
            * {
				box-sizing: border-box;
			}
			html,
			body {
				height: 100%;
			}
			body {
				margin: 0;
				font-family: Arial, Helvetica, sans-serif;
				font-size: 10pt;
				color: #111;
				-webkit-print-color-adjust: exact;
				print-color-adjust: exact;
			}
			@page {
				size: A4;
				margin: 0;
			}
			.page {
				width: 190mm; /* A4 width (210) - default margins (10 each side) */
				min-height: 267mm; /* A4 height (297) - default margins (10 top/bottom) */
				margin: 15mm auto;
				page-break-after: always;
			}
			.mb-2 {
				margin-bottom: 0.5rem;
			}
			.mb-5 {
				margin-bottom: 2rem;
			}
			.mt-0 {
				margin-top: 0rem;
			}
			.mt-1 {
				margin-top: 0.25rem;
			}
			.mt-2 {
				margin-top: 0.5rem;
			}
			.mt-3 {
				margin-top: 1rem;
			}
			.ml-2 {
				margin-left: 0.5rem;
			}
			.right {
				text-align: right;
			}
			.center {
				text-align: center;
			}
			.bold {
				font-weight: bold;
			}
			.italic {
				font-style: italic;
			}
			.nowrap {
				white-space: nowrap;
			}
			.v-middle {
				vertical-align: middle;
			}

			.header {
				text-align: center;
				margin-bottom: 6px;
			}
			.header .line1 {
				font-weight: bold;
				text-transform: uppercase;
			}
			.header .line2 {
				font-weight: bold;
			}
			.header .underline {
				width: 120px;
				height: 1px;
				background: #111;
				margin: 2px auto 0;
			}
			.title {
				text-align: center;
				font-weight: bold;
				text-transform: uppercase;
			}

			.two-col-table {
				width: 100%;
				border-collapse: collapse;
				table-layout: fixed;
				border: none;
			}
			.two-col-table td {
				vertical-align: top;
				border: none;
			}
			.two-col-left {
				width: 35mm;
			}
			.photo {
				border: 1px solid #111;
				width: 30mm;
				height: 40mm;
				margin: 0 auto;
				text-align: center;
				padding: 3px;
				display: table;
			}
			.photo span {
				display: table-cell;
				vertical-align: middle;
			}

			.row {
				display: block;
				margin: 3px 0;
				margin-bottom: 0.25rem;
			}
			.inline {
				display: inline-block;
			}

			.cb {
				display: inline-block;
				width: 13px;
				height: 13px;
				border: 1px solid #111;
				vertical-align: middle;
				margin: 0 3px;
			}

			.cb.checked::after,
			.cb[checked]::after {
				content: "\\00D7"; /* dấu tick */
				font-size: 20px;
				line-height: 13px;
				text-align: center;
				display: block;
				color: #000;
				font-weight: bold;
			}

			table {
				width: 100%;
				border-collapse: collapse;
				margin-top: 6px;
			}
      table.auto {
        width: auto;
        display: inline-table;
      }
			th,
			td {
				border: 1px solid #222;
				padding: 4px 6px;
				vertical-align: top;
			}
			th {
				text-align: center;
				font-weight: bold;
			}
			.td-tight td {
				padding-top: 2px;
				padding-bottom: 2px;
			}
      .outer-table.no-border td,
      .outer-table.no-border th,
      .no-border {
				border: none;
			}

			.signature-box {
				text-align: center;
			}

			.dotted-ruled {
				display: inline-block;
				border-bottom: 1px dotted currentColor;
				line-height: 1em;
				height: 1em;
				vertical-align: baseline;
				min-width: 5px;
				width: auto;
				padding-left: 0.35rem;
			}
			.dotted-ruled::after {
				content: "\\200b";
			}
			.dot-wrap {
				display: inline-block;
			}
			.dot-wrap .dotted-ruled {
				width: 100%;
			}

			.dotted-ruled sup {
				position: relative;
				top: -0.4em;
				font-size: 0.75em;
				line-height: 1;
			}

			.gap {
				min-width: 4mm;
				display: inline-block;
			}

			.short-line {
				width: 150px;
				border: none;
				border-top: 1px solid #000;
				margin: 4px 0;
			}

			tr,
			td,
			th {
				page-break-inside: avoid;
			}
			.fixed-width {
				 width: 150px;
			}
        `;
        const htmlContent = `
            <!DOCTYPE html>
            <html>
            <head>
                <title>Sổ Khám Sức Khỏe</title>
                <meta charset="utf-8">
				<meta name="viewport" content="width=device-width, initial-scale=1" />
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
