function clearChart(selector) {
    const chartContainer = document.querySelector(selector);
    if (chartContainer) {
        chartContainer.innerHTML = "";
    }
}

function initSimpleBarChart(selector, series, labels, colors, horizontal = true) {
    clearChart(selector);
    let chartOptions = {
        series: series,
        chart: {
            height: 300,
            type: "bar",
            zoom: {
                enabled: false,
            },
            toolbar: {
                show: false,
            },
        },
        dataLabels: {
            enabled: false,
        },
        stroke: {
            show: true,
            width: 1,
        },
        colors: colors,
        xaxis: {
            categories: labels,
            axisBorder: {
                color: "#e0e6ed",
            },
        },
        yaxis: {
            opposite: false,
            reversed: false,
        },
        grid: {
            borderColor: "#e8e8e8",
        },
        plotOptions: {
            bar: {
                horizontal: horizontal,
            },
        },
        fill: {
            opacity: 0.8,
        },
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}

function initLineBarChart(selector, series, labels, colors) {
    clearChart(selector);
    let chartOptions = {
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
        series: series,
        dataLabels: {
            enabled: false,
        },
        stroke: {
            show: true,
            curve: "smooth",
            width: 3,
            lineCap: "square",
        },
        dropShadow: {
            enabled: true,
            opacity: 0.8,
            blur: 10,
            left: -7,
            top: 22,
        },
        colors: colors,
        markers: {
            discrete: [
                {
                    seriesIndex: 0,
                    dataPointIndex: 4,
                    fillColor: "#323a46",
                    strokeColor: "#fff",
                    size: 6,
                },
                {
                    seriesIndex: 1,
                    dataPointIndex: 5,
                    fillColor: "#A8C5DA",
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
            tickAmount: 5,
            labels: {
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
            borderColor: "#e8e8e8",
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
        }
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}

function initAreaChart(selector, series, labels, colors) {
    clearChart(selector);
    let chartOptions = {
        series: series,
        chart: {
            type: "area",
            height: 300,
            zoom: {
                enabled: false,
            },
            toolbar: {
                show: false,
            },
        },
        colors: colors,
        dataLabels: {
            enabled: false,
        },
        stroke: {
            width: 2,
            curve: "smooth",
        },
        xaxis: {
            axisBorder: {
                color: "#e0e6ed",
            },
        },
        yaxis: {
            opposite: false,
            labels: {
                offsetX: 0,
            },
        },
        labels: labels,
        legend: {
            horizontalAlign: "left",
        },
        grid: {
            borderColor: "#e8e8e8",
        },
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}

function initPieChart(selector, series, labels, colors) {
    clearChart(selector);
    let chartOptions = {
        series: series,
        chart: {
            height: 300,
            type: 'pie',
        },
        labels: labels,
            responsive: [{
                breakpoint: 480,
                options: {
                    chart: {
                        width: 200
                    },
                    legend: {
                        position: 'bottom'
                    }
                }
            }]
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}
