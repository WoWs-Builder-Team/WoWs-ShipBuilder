
export function SetupGlobalChartConfig()
{
    let defaults = Chart.defaults;
    defaults.responsive = true;
    defaults.maintainAspectRatio = true;
    defaults.datasets.spanGaps = true;
    defaults.animation = false;
    
    //colors settings
    defaults.color = '#a9a9a9';
    defaults.borderColor = '#696969';

    //title settings
    defaults.plugins.title.display = true;
    defaults.plugins.title.font.size = 30; 
    //tooltip settings
    defaults.interaction.mode = 'nearest';
    defaults.interaction.intersect = true;
    defaults.interaction.displayColors = true;
    defaults.plugins.tooltip.caretPadding = 20;
    defaults.plugins.tooltip.caretSize = 10;
    defaults.plugins.tooltip.usePointStyle = true;
    defaults.plugins.tooltip.boxPadding = 5;
    //point settings
    defaults.elements.point.radius = 0;
    defaults.elements.point.hitRadius = 10;
    defaults.elements.point.hoverRadius = 5;
    
    const autocolors = window['chartjs-plugin-autocolors'];
    Chart.register(autocolors);
    
}

function GetColor(index)
{
    const colors =["#ef6fcc", "#62ce75", "#f53a4c", "#11ccdc", "#9166aa", "#a4c28a", "#c15734", "#faa566", "#6c7b66", "#eda4ba", "#2d6df9", "#f62ef3", "#957206", "#a45dff",];
    return colors[index % 14];
}

export function AddData(chartId, data, label, index) {
    const chart = Chart.getChart(chartId);
    const dataset =
        {
            data: data,
            label: label,
            index: index,
        };
    
    chart.data.datasets.push(dataset);
    
    chart.update();
}

export function BatchAddData(chartsId, datas, labels, indexes)
{
    chartsId.forEach((chartId, chartIndex) =>{
        const chart = Chart.getChart(chartId);
        datas.forEach((shipData, index) => {
            const dataset =
                {
                    data : shipData[chartIndex],
                    label: labels[index],
                    index: indexes[index],
                };
            chart.data.datasets.push(dataset)
        });
        chart.update();
    });

}

export function BatchRemoveData(chartsId, indexes)
{
    chartsId.forEach((chartId, chartIndex) =>{
        const chart = Chart.getChart(chartId);

        indexes.forEach((index) => {
            chart.data.datasets.splice(index, 1);  
        })
        chart.update();
    });
}

export function BatchUpdateTrajectory(chartId, indexes, newDatas)
{
    const chart = Chart.getChart(chartId);
    newDatas.forEach((shipData, index) => {
        chart.data.datasets[indexes[index]].data = shipData;
    });
    chart.update();
}

export function UpdateData(chartId, index, newData)
{
    const chart = Chart.getChart(chartId);
    chart.data.datasets[index].data = newData;
    chart.update();
}

export function RemoveData(chartId, index) {
    const chart = Chart.getChart(chartId);
    chart.data.datasets.splice(index, 1);
    chart.update();
}

export function RemoveAllData(chartId)
{
    const chart = Chart.getChart(chartId);
    chart.data.datasets.length = 0;
    chart.update();
}

export function ChangeSuggestedMax(chartId, newSuggestedMax)
{
    const chart = Chart.getChart(chartId);
    chart.options.scales['y'].suggestedMax = newSuggestedMax;
    chart.update();
}

export function CreateChart(chartId, title, xLabel, yLabel, xUnit, yUnit)
{
    const ctx = document.getElementById(chartId);
    const chart = new Chart(ctx,
        {
            type: 'line',
            options:
                {
                    aspectRatio: 3,
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
                                                    label += ' ' + xUnit;
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