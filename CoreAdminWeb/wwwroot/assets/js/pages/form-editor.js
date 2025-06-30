window.applyFormat = (textArea, format) => {
    const editor = textArea;
    if (format === 'bold') {
        document.execCommand('bold');
    } else if (format === 'italic') {
        document.execCommand('italic');
    } else if (format === 'underline') {
        document.execCommand('underline');
    }
    // Implement other formatting options like justify, insert lists, etc.
};
