function initializeDatePicker() {
    flatpickr(".date-picker", {
        dateFormat: "d/m/Y",
        allowInput: true,
        enableTime: false,
        locale: "vi",
        onChange: function(selectedDates, dateStr, instance) {
            // Format date as dd/MM/yyyy
            const date = selectedDates[0];
            const formattedDate = date ? `${date.getDate().toString().padStart(2, '0')}/${(date.getMonth() + 1).toString().padStart(2, '0')}/${date.getFullYear()}` : '';
            
            // Update input value and trigger change for Blazor binding
            instance.input.value = formattedDate;
            instance.input.dispatchEvent(new Event('change', { bubbles: true }));
        },
        parseDate: function(datestr, format) {
            // Parse date from dd/MM/yyyy format
            if (datestr) {
                const parts = datestr.split('/');
                if (parts.length === 3) {
                    return new Date(parts[2], parts[1] - 1, parts[0]);
                }
            }
            return null;
        }
    });
}

// For the example page
flatpickr("#basicExample", {
    enableTime: true,
    dateFormat: "d-m-Y",
});

window.initializeDatePicker = initializeDatePicker; 

