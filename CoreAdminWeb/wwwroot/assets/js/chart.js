function clearChart(selector) {
    const chartContainer = document.querySelector(selector);
    if (chartContainer) {
        chartContainer.innerHTML = "";
    }
}

function formatCurrency(val) {
    if (val === null || val === '' || isNaN(val)) return val;
    let num = Number(val);

    const sign = num < 0 ? "-" : "";
    const absNum = Math.abs(num);

    const units = [
        { value: 1e15, symbol: 'Q' }, // Quadrillion
        { value: 1e12, symbol: 'T' }, // Trillion
        { value: 1e9, symbol: 'B' }, // Billion
        { value: 1e6, symbol: 'M' }, // Million
        { value: 1e3, symbol: 'K' }  // Thousand
    ];

    for (const element of units) {
        if (absNum >= element.value) {
            let result = absNum / element.value;
            return sign + result.toFixed(3).replace(/\.?0+$/, '') + element.symbol;
        }
    }
    return sign + absNum.toString();
}

function contrastTextColor(bg) {
    const rgb = toRGB(bg);
    if (!rgb) return '#000';

    const toLinear = (c) => {
        c /= 255;
        return c <= 0.03928 ? c / 12.92 : Math.pow((c + 0.055) / 1.055, 2.4);
    };
    const R = toLinear(rgb.r), G = toLinear(rgb.g), B = toLinear(rgb.b);
    const L = 0.2126 * R + 0.7152 * G + 0.0722 * B;

    const contrastWhite = (1.05) / (L + 0.05);
    const contrastBlack = (L + 0.05) / 0.05;

    return contrastWhite >= contrastBlack ? '#FFFFFF' : '#000000';
}

function toRGB(color) {
    if (!color) return null;
    const c = color.trim();

    let m = c.match(/^#([0-9a-f]{3})$/i);
    if (m) {
        const [r, g, b] = m[1].split('').map(x => parseInt(x + x, 16));
        return { r, g, b };
    }
    m = c.match(/^#([0-9a-f]{6})$/i);
    if (m) {
        const r = parseInt(m[1].slice(0, 2), 16);
        const g = parseInt(m[1].slice(2, 4), 16);
        const b = parseInt(m[1].slice(4, 6), 16);
        return { r, g, b };
    }
    m = c.match(/^rgba?\((\d+)\s*,\s*(\d+)\s*,\s*(\d+)/i);
    if (m) return { r: +m[1], g: +m[2], b: +m[3] };

    return null;
}

function normalizeColorsOptions(colorsParam) {
    const isArray = Array.isArray(colorsParam);
    const isRangesObj = colorsParam && typeof colorsParam === 'object' && Array.isArray(colorsParam.ranges);
    const isRangesArray = Array.isArray(colorsParam) && colorsParam.length && typeof colorsParam[0] === 'object' && 'from' in colorsParam[0] && 'to' in colorsParam[0];

    let rootColors = undefined; 
    let barColorRanges = undefined;
    let dataLabelColors = ['#000'];

    if (isArray && !isRangesArray) {
        rootColors = colorsParam;
        dataLabelColors = colorsParam.map(contrastTextColor);
    } else if (isRangesObj || isRangesArray) {
        const ranges = isRangesObj ? colorsParam.ranges : colorsParam;
        barColorRanges = ranges.map(r => ({
            from: typeof r.from === 'number' ? r.from : -1e308,
            to: typeof r.to === 'number' ? r.to : 1e308,
            color: r.color || '#999'
        }));
        dataLabelColors = ['#000'];
    }

    return { rootColors, barColorRanges, dataLabelColors };
}

function initSimpleBarChart(selector, series, labels, colors, horizontal = true, isShowlegend = false, isDistribute = false) {
    console.log('Init chart ' + selector);
    clearChart(selector);
    const { barColorRanges, dataLabelColors } = normalizeColorsOptions(colors);
    console.log(barColorRanges, dataLabelColors);
    let chartOptions = {
        series: series,
        chart: {
            height: 350,
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
            offsetX: 0,
            style: {
                fontSize: '12px',
                colors: dataLabelColors
            },
            formatter: (val) => formatCurrency(val)
        },
        ...(Array.isArray(colors) ? { colors } : {}),
        xaxis: {
            categories: labels,
            labels: {
                formatter: (val) => formatCurrency(val)
            }
        },
        yaxis: {
            labels: {
                show: true,
                formatter: (val) => formatCurrency(val)
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
                ...(barColorRanges
                    ? { colors: { ranges: barColorRanges } }
                    : {})
            }
        },
        fill: {
            opacity: 0.8,
        },
        tooltip: {
            y: {
                formatter: (val) => formatCurrency(val)
            },
            x: {
                formatter: (val) => formatCurrency(val)
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

    const dataLabelColors = colors?.map(contrastTextColor) ?? ['#000'];

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
            style: {
                fontSize: '12px',
                colors: dataLabelColors
            },
            formatter: (val) => formatCurrency(val)
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
                formatter: (val) => formatCurrency(val)
            }
        },
        yaxis: {
            labels: {
                show: true,
                formatter: (val) => formatCurrency(val)
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
                formatter: (val) => formatCurrency(val)
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

    const dataLabelColors = colors?.map(contrastTextColor) ?? ['#000'];

    let chartOptions = {
        series: series,
        chart: {
            type: "area",
            height: 350,
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
            style: {
                fontSize: '12px',
                colors: dataLabelColors
            },
            formatter: (val) => formatCurrency(val)
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

    const dataLabelColors = colors?.map(contrastTextColor) ?? ['#000'];

    console.log(dataLabelColors)

    let chartOptions = {
        series: series,
        chart: {
            height: 350,
            type: 'pie',
        },
        labels: labels,
        stroke: {
            width: 0
        },
        dataLabels: {
            //formatter: function (val, opts) {
            //    const value = opts.w.config.series[opts.seriesIndex];
            //    return formatCurrency(value);
            //},
            style: {
                colors: dataLabelColors,
            }
        },
        legend: {
            position: 'top'
        },
        tooltip: {
            enabled: true,
            y: {
                formatter: function (val) {
                    return formatCurrency(val);
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
