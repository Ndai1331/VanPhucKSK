/*
Template Name: CoreAdminWeb - Admin & Dashboard Template
Author: SRBThemes
Version: 2.3.0
File: Flatpickr init Js File
*/




import flatpickr from "flatpickr";

flatpickr("#basicExample", {
    enableTime: true,
    dateFormat: "d-m-Y",
});
flatpickr(".date-picker", {
    dateFormat: "d/m/Y",
    allowInput: true,
    enableTime: false,
    locale: "vi",
    firstDayOfWeek: 1
});