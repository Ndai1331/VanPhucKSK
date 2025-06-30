

// Function to toggle the dropdown menu
function ToggleDropdown() {
    var dropdown = document.getElementById("dropdownMenu");
    dropdown.classList.toggle("open");

    // Add an event listener to close the dropdown if clicking outside of it
    document.addEventListener('click', handleOutsideClick);
}

// Function to close the dropdown when pressing the Escape key
function CloseDropdownWithEscape(event) {
    if (event.key === "Escape") {
        CloseDropdown();
    }
}

// Function to close the dropdown
function CloseDropdown() {
    var dropdown = document.getElementById("dropdownMenu");
    dropdown.classList.remove("open");

    // Remove the event listener once dropdown is closed
    document.removeEventListener('click', handleOutsideClick);
}

// Function to handle clicks outside of the dropdown
function handleOutsideClick(event) {
    var dropdown = document.getElementById("dropdownMenu");

    // Check if the click is outside the dropdown
    if (!dropdown.contains(event.target)) {
        CloseDropdown();
    }
}

window.toggleSidebar = function () {
    debugger
    const body = document.querySelector('body');
    body.classList.toggle('toggle-sidebar');
};

window.removeToggleSidebarClass = function () {
    debugger
    const body = document.querySelector('body');
    body.classList.remove('toggle-sidebar');
};


function toggleSidebar() {
    debugger
    const body = document.querySelector('body');
    body.classList.toggle('toggle-sidebar');
}

function removeToggleSidebarClass() {
    debugger
    const body = document.querySelector('body');
    body.classList.remove('toggle-sidebar');
}

window.getDroppedFiles = async (event) => {
    event.preventDefault();
    const fileInput = document.getElementById('fileInput');
    fileInput.files = event.dataTransfer.files;
    fileInput.dispatchEvent(new Event('change', { bubbles: true }));
    return true; // Indicate success
};

window.createObjectURL = (file) => {
    return URL.createObjectURL(file);
};

window.revokeObjectURL = (url) => {
    URL.revokeObjectURL(url);
};

window.getImageDimensions = async (file) => {
    return new Promise((resolve) => {
        const img = new Image();
        img.src = URL.createObjectURL(file);
        img.onload = () => {
            resolve({ width: img.width, height: img.height });
            URL.revokeObjectURL(img.src);
        };
        img.onerror = () => resolve({ width: 0, height: 0 });
    });
};




