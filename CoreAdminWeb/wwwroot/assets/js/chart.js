function clearChart(selector) {
    const chartContainer = document.querySelector(selector);
    if (chartContainer) {
        chartContainer.innerHTML = "";
    }
}

function initSimpleBarChart(selector, series, labels, colors, horizontal = true, isShowlegend = false, isDistribute = false) {
    console.log('Init chart ' + selector);
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
            enabled: true,
            offsetX: -6,
            style: {
                fontSize: '12px',
                colors: ['#fff']
            },
            formatter: (val) => {
                if (!isNaN(val) && val !== '' && val !== null) {
                    return Number(val).toLocaleString('en-EN');
                }
                return val;
            }
        },
        colors: colors,
        xaxis: {
            categories: labels,
            labels: {
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            }
        },
        yaxis: {
            labels: {
                show: true,
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            },
            opposite: false,
            reversed: false,
        },
        grid: {
            borderColor: "#e8e8e8",
        },
        plotOptions: {
            bar: {
                distributed: isDistribute,
                horizontal: horizontal,
                dataLabels: {
                    position: 'top',
                },
            }
        },
        fill: {
            opacity: 0.8,
        },
        tooltip: {
            y: {
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            },
            x: {
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            }
        }
    };

    if (horizontal) {
        chartOptions.plotOptions.barHeight = '100%';
        chartOptions.plotOptions.dataLabels = {
            position: 'left'
        };
    }

    if (isShowlegend) {
        chartOptions.legend = {
            position: 'top',
            horizontalAlign: 'left',
            offsetX: 40
        };
    }

    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}

function initLineChart(selector, series, labels, colors) {
    console.log('Init chart ' + selector);
    clearChart(selector);
    let chartOptions = {
        series: series,
        chart: {
            height: 350,
            type: 'line',
            zoom: {
                enabled: false
            },
            toolbar: {
                show: false
            }
        },
        colors: colors,
        dataLabels: {
            enabled: false,
        },
        stroke: {
            curve: 'smooth'
        },
        grid: {
            borderColor: '#e7e7e7'
        },
        markers: {
            size: 1
        },
        xaxis: {
            categories: labels,
            labels: {
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            }
        },
        yaxis: {
            labels: {
                show: true,
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            },
            opposite: false,
            reversed: false,
        },
        legend: {
            position: 'top',
            horizontalAlign: 'center',
            floating: true,
        },
        tooltip: {
            y: {
                formatter: (val) => {
                    if (!isNaN(val) && val !== '' && val !== null) {
                        return Number(val).toLocaleString('en-US');
                    }
                    return val;
                }
            }
        }
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}

function initAreaChart(selector, series, labels, colors) {
    console.log('Init chart ' + selector);
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
    console.log('Init chart ' + selector);
    clearChart(selector);
    let chartOptions = {
        series: series,
        chart: {
            height: 300,
            type: 'pie',
        },
        labels: labels
    };
    let chart = new ApexCharts(
        document.querySelector(selector),
        chartOptions
    );

    chart.render();
}
