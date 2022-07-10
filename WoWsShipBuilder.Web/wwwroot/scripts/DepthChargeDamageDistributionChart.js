export function DrawDepthChargeDamageDistributionChart(id, dataRecord){
    console.log(dataRecord);
    const canvas = document.getElementById(id);
    const ctx = canvas.getContext("2d");
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    canvas.width = width;
    canvas.height = height;
    console.log(width);
    console.log(height);
}