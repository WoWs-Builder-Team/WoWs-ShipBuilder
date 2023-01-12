/**
 * @description Setup global charts configurations.
 */
export function SetupGlobalChartConfig(aspectRatio)
{
    let defaults = Chart.defaults;
    defaults.responsive = true;
    defaults.maintainAspectRatio = true;
    defaults.datasets.spanGaps = true;
    defaults.animation = false;
    defaults.aspectRatio = aspectRatio;
    
    //colors settings
    defaults.color = '#a9a9a9';
    defaults.borderColor = '#696969';

    //title settings
    defaults.plugins.title.display = true;
    defaults.plugins.title.font.size = 30; 
    
    //tooltip settings
    defaults.interaction.mode = 'nearest';
    defaults.interaction.intersect = true;
    defaults.interaction.axis = 'x';
    defaults.interaction.displayColors = true;
    defaults.plugins.tooltip.caretPadding = 20;
    defaults.plugins.tooltip.caretSize = 10;
    defaults.plugins.tooltip.usePointStyle = true;
    defaults.plugins.tooltip.boxPadding = 5;
    
    //point settings
    defaults.elements.point.radius = 0;
    defaults.elements.point.hitRadius = 5;
    defaults.elements.point.hoverRadius = 5;
    
    //line settings
    defaults.elements.line.borderJoinStyle = "bevel";
    
    const autocolors = window['chartjs-plugin-autocolors'];
    Chart.register(autocolors);
    
}

/**
 * @description Get a color from a predefined list based on an index. Loops every 14.
 * @param {Array<string>} index - An index to get the color of
 */
function GetColor(index)
{
    const colors =["#ef6fcc", "#62ce75", "#f53a4c", "#11ccdc", "#9166aa", "#a4c28a", "#c15734", "#faa566", "#6c7b66", "#eda4ba", "#2d6df9", "#f62ef3", "#957206", "#a45dff",];
    return colors[index % 14];
}

/**
 * @description Add multiple datasets to multiple charts at once.
 * @param {Array<string>} chartsId - The charts ID
 * @param {Array<{id: string, label: string , datasets: Array<Array<number>>, index: number }>} chartDataList - List of the new chart data.
 */
export function BatchAddData(chartsId, chartDataList)
{
    chartsId.forEach((chartId, chartIndex) =>{
        const chart = Chart.getChart(chartId);
        chartDataList.forEach(shipData =>
        {
            const dataset =
                {
                    data: shipData.datasets[chartIndex],
                    label: shipData.label,
                    index: shipData.index,
                    guid: shipData.id,
                };
            chart.data.datasets.push(dataset)
        });
        chart.update();
    });

}

/**
 * @description Remove multiple datasets from multiple charts at once.
 * @param {Array<string>} chartsId - The charts ID
 * @param {Array<string>} guids - The labels of the dataset to remove
 */
export function BatchRemoveData(chartsId, guids)
{
    chartsId.forEach((chartId) =>{
        const chart = Chart.getChart(chartId);

        guids.forEach((guid) => {
            const shipIndex = chart.data.datasets.findIndex(dataset => {
                return dataset.guid === guid;
            })
            chart.data.datasets.splice(shipIndex, 1);
        });
        chart.update();
    });
}

/**
 * @description Update multiple dataset at once for a specific chart.
 * @param {string} chartId - The chart Id
 * @param {Array<{id: string, datasets: Array<number>}>} updatedChartDataList - The labels of the dataset to update
 */
export function BatchUpdateData(chartId, updatedChartDataList)
{
    const chart = Chart.getChart(chartId);
    updatedChartDataList.forEach(chartData =>
    {
        const shipIndex = chart.data.datasets.findIndex(dataset => {
            return dataset.guid === chartData.id;
        })
        chart.data.datasets[shipIndex].data = chartData.datasets;
    });
    chart.update();
}

/**
 * @description Update multiple dataset at once for a specific chart.
 * @param {string} chartId - The chart Id
 * @param {Array<{id: string, newLabel: string, datasets: Array<number>}>} updatedChartDataList - The labels of the dataset to update
 */
export function BatchUpdateDataNewLabels(chartId, updatedChartDataList)
{
    const chart = Chart.getChart(chartId);
    updatedChartDataList.forEach(chartData =>
    {
        const shipIndex = chart.data.datasets.findIndex(dataset => {
            return dataset.guid === chartData.id;
        })
        chart.data.datasets[shipIndex].data = chartData.datasets;
        chart.data.datasets[shipIndex].label = chartData.newLabel;
    });
    chart.update();
}

/**
 * @description Change the suggested max parameter of a chart.
 * @param {string} chartId - The chart Id
 * @param {number} newSuggestedMax - The new SuggestedMax parameter for the chart
 */
export function ChangeSuggestedMax(chartId, newSuggestedMax)
{
    const chart = Chart.getChart(chartId);
    if (chart == null)
    {
        return;
    }
    chart.options.scales['y'].suggestedMax = newSuggestedMax;
    chart.update();
}


/**
 * @description Create a chart from basic data.
 * @param {string} chartId - The canvas Id to create the chart on
 * @param {number} title - The title of the chart
 * @param {number} xLabel - The label for the x axis
 * @param {number} yLabel - The label for the y axis
 * @param {number} xUnit - The measurement unit of the x axis
 * @param {number} yUnit - The measurement unit of the y axis
 */
export function CreateChart(chartId, title, xLabel, yLabel, xUnit, yUnit)
{
    const ctx = document.getElementById(chartId);
    const chart = new Chart(ctx,
        {
            type: 'line',
            options:
                {
                    parsing: false,
                    scales:
                        {
                            x:
                                {
                                    type: "linear",
                                    beginAtZero : true,
                                    grace: "10%",
                                    maxTicksLimit: 10,
                                    title:
                                        {
                                            text: xLabel,
                                            font:
                                                {
                                                    size: 15,
                                                },
                                            display: true
                                        },
                                    grid:
                                        {
                                            tickLength: 5,
                                            borderDash : [5, 5],
                                            borderWidth: 3,
                                        },
                                    ticks:
                                        {
                                            callback: function (value, index, ticks) {
                                                return Chart.Ticks.formatters.numeric.apply(this, [value, index, ticks]) + " " + xUnit;
                                            }
                                        }
                                },
                            y:
                                {
                                    beginAtZero : true,
                                    grace: "10%",
                                    maxTicksLimit: 10,
                                    title:
                                        {
                                            text: yLabel,
                                            font:
                                                {
                                                    size: 15,
                                                },
                                            display: true
                                        },
                                    grid:
                                        {
                                            tickLength: 5,
                                            borderDash : [5, 5],
                                            borderWidth: 3,
                                        },
                                    ticks:
                                        {
                                            callback: function (value, index, ticks) {
                                                return Chart.Ticks.formatters.numeric.apply(this, [value, index, ticks]) + " " + yUnit;
                                            }
                                        }
                                }
                        },
                    plugins:
                        {
                            title:
                                {
                                    display: true,
                                    text: title,
                                },
                            tooltip:
                                {
                                    callbacks:
                                        {
                                            labelPointStyle: function(context) {
                                                return {
                                                    pointStyle: 'line',
                                                    rotation: 0
                                                };
                                            },
                                            label: function(context)
                                            {
                                                let label = context.dataset.label;

                                                if (label)
                                                {
                                                    label += ': ';
                                                }

                                                if (context.parsed.y !== null)
                                                {
                                                    label += context.parsed.y.toFixed(2);
                                                }

                                                if (yUnit)
                                                {
                                                    label += ' ' + yUnit;
                                                }

                                                return label;
                                            },
                                            title:  function (context)
                                            {
                                                let label =  context[0].parsed.x.toFixed(2);

                                                if (xUnit)
                                                {
                                                    label = xLabel + ': ' + label + ' ' + xUnit;
                                                }

                                                return label;
                                            }
                                        }
                                },
                            autocolors:
                                {
                                    mode: 'dataset',
                                    customize(context) {
                                        const index = context.datasetIndex;
                                        return {
                                            background: GetColor(index),
                                            border: GetColor(index)
                                        };
                                    }
                                }
                        }
                },
        });
}