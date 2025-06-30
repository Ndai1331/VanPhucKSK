window.addEventListener('click', function (event) {
    const dropdown = document.getElementById('dropdownMenu');
    if (dropdown && !dropdown.contains(event.target)) {
        DotNet.invokeMethodAsync('CoreAdminWeb', 'CloseDropdown');
    }
});
