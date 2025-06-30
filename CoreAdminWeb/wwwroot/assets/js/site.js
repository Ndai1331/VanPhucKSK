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

// Function to get clicked page number from data attribute
function getClickedPageNumber() {
    // Get the clicked element
    var clickedElement = event.target;
    // Get the data-page attribute
    return clickedElement.getAttribute('data-page');
}

function initBassicLineChart(element, serries, labels, colors) {

    // Chart Widget 5
    var chartOptions = {
        chart: {
            height: 300,
            type: "area",
            fontFamily: "Inter, sans-serif",
            zoom: {
                enabled: false,
            },
            toolbar: {
                show: false,
            },
        },
        series: [{
            name: "Số đợt kiểm tra",
            data: serries
        }],
        dataLabels: {
            enabled: false,
        },
        stroke: {
            show: true,
            curve: "smooth",
            width: 2,
            lineCap: "square",
        },
        dropShadow: {
            enabled: false,
        },
        colors: colors,
        markers: {
            discrete: [
                {
                    seriesIndex: 0,
                    dataPointIndex: 4,
                    fillColor: "#6a69f5",
                    strokeColor: "#fff",
                    size: 6,
                },
            ],
        },
        labels: labels,
        xaxis: {
            axisBorder: {
                show: false,
            },
            axisTicks: {
                show: false,
            },
            crosshairs: {
                show: true,
            },
            labels: {
                offsetX: 0,
                offsetY: 5,
                style: {
                    fontSize: "12px",
                    cssClass: "apexcharts-xaxis-title",
                },
            },
        },
        yaxis: {
            tickAmount: 12,
            labels: {
                formatter: ((value) => {
                    return value;
                }),
                offsetX: -10,
                offsetY: 0,
                style: {
                    fontSize: "12px",
                    cssClass: "apexcharts-yaxis-title",
                },
            },
            opposite: false,
        },
        grid: {
            borderColor: "#e0e6ed",
            strokeDashArray: 7,
            xaxis: {
                lines: {
                    show: false,
                },
            },
            yaxis: {
                lines: {
                    show: true,
                },
            },
            padding: {
                top: 0,
                right: 0,
                bottom: 0,
                left: 0,
            },
        },
        legend: {
            show: false,
        },
        tooltip: {
            marker: {
                show: true,
            },
            x: {
                show: false,
            },
        },
        fill: {
            type: "gradient",
            gradient: {
                shadeIntensity: 1,
                inverseColors: !1,
                opacityFrom: 0,
                opacityTo: 0,
                stops: [100, 100],
            },
        },
    };
    var chart = new ApexCharts(document.querySelector(element), chartOptions);
    chart.render();
}

function initBassicBarChart(element, serries, labels, colors) {

// barchart
var barchart = {
    series: [{
        name: "Số lượng cơ sở",
        data: serries
    }],
    chart: {
        type: "bar",
        height: 300,
        stacked: false,
        toolbar: {
            show: false,
        },
        zoom: {
            enabled: false,
        },
    },
    dataLabels: {
        enabled: false,
    },
    colors: colors,
    plotOptions: {
        bar: {
            horizontal: false,
            columnWidth: "25%",
            distributed: true,
        },
    },
    xaxis: {
        categories: labels,
    },
    legend: {
        show: false,
    },
    fill: {
        opacity: 1,
    },
};

    var chart = new ApexCharts(document.querySelector(element), barchart);
    chart.render();
}
