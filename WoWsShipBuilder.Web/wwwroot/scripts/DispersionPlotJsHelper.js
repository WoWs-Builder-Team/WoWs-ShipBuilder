export function DrawDispersionPlotBatched(data, scaling, fusoReference){
    for(const key in data){
        const canvas = document.getElementById(key);
        const ctx = canvas.getContext("2d");
        
        const drawingData = data[key];
        
        const ellipseOffset = 100;
        const rulersOffset = 20;
        canvas.width = drawingData.xRadiusOuterEllipse * 2 + ellipseOffset;
        canvas.height = drawingData.yRadiusOuterEllipse * 2 + ellipseOffset;
        if (Object.values(fusoReference)[0] !== 0 && Object.values(fusoReference)[1] !== 0){
            if(Object.values(fusoReference)[0] > drawingData.xRadiusOuterEllipse){
                canvas.width = Object.values(fusoReference)[0] * 2 + ellipseOffset;
            }
            if(Object.values(fusoReference)[1] > drawingData.yRadiusOuterEllipse){
                canvas.height = Object.values(fusoReference)[1] * 2 + ellipseOffset;
            }
        }
        let width = canvas.width;
        let height = canvas.height;
        let centerX = width / 2;
        let centerY = height / 2;
        const top = ellipseOffset / 2;
        const bottom = height - ellipseOffset / 2;
        const right = width - ellipseOffset / 2;
        const left = ellipseOffset / 2;

        ctx.clearRect(0, 0, width, height);
        ctx.lineWidth  = 1;
        ctx.strokeStyle="gray";
        ctx.beginPath();
        ctx.moveTo(centerX, top);
        ctx.lineTo(centerX, bottom);
        ctx.moveTo(left, centerY);
        ctx.lineTo(right, centerY);
        ctx.stroke();

        if (Object.values(fusoReference)[0] !== 0 && Object.values(fusoReference)[1] !== 0){
            ctx.lineWidth  = 4;
            ctx.strokeStyle="rgba(0, 0, 0, 0.5)";
            ctx.fillStyle = "rgba(0, 0, 0, 0.25)";
            ctx.beginPath();
            ctx.ellipse(centerX, centerY, Object.values(fusoReference)[0], Object.values(fusoReference)[1], 0, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke()
        }
        
        ctx.lineWidth  = 4;
        ctx.strokeStyle="gray";
        ctx.fillStyle = "rgba(128, 128, 128, 0.1)";
        ctx.beginPath();
        ctx.ellipse(centerX, centerY, drawingData.xRadiusOuterEllipse, drawingData.yRadiusOuterEllipse, 0, 0, 2 * Math.PI);
        ctx.fill();
        ctx.stroke()

        ctx.strokeStyle = "rgba(255 , 46 , 46, 0.5)";
        ctx.lineWidth  = 4;
        ctx.fillStyle = "rgba(255 , 46 , 46, 0.1)";
        ctx.beginPath();
        ctx.ellipse(centerX, centerY, drawingData.xRadiusInnerEllipse, drawingData.yRadiusInnerEllipse, 0, 0, 2 * Math.PI);
        ctx.fill();
        ctx.stroke()

        ctx.strokeStyle = "transparent";
        ctx.lineWidth  = 1;
        ctx.fillStyle = "rgba(218,165,32, 0.33)";
        let pointRadius = 6;
        if(scaling < 1){
            pointRadius *= scaling;
        }
        drawingData.hitPoints.forEach(function (point){
            ctx.beginPath();
            ctx.ellipse(centerX - Object.values(point)[0], centerY - Object.values(point)[1], pointRadius, pointRadius, 0, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke()
        });
        
        ctx.lineWidth  = 1;
        ctx.strokeStyle="gray";
        ctx.beginPath();
        ctx.moveTo(centerX - drawingData.xRadiusOuterEllipse, bottom + rulersOffset);
        ctx.lineTo(centerX + drawingData.xRadiusOuterEllipse, bottom + rulersOffset);
        ctx.moveTo(centerX - drawingData.xRadiusOuterEllipse, bottom + rulersOffset  + 10);
        ctx.lineTo(centerX - drawingData.xRadiusOuterEllipse, bottom + rulersOffset - 10);
        ctx.moveTo(centerX + drawingData.xRadiusOuterEllipse, bottom + rulersOffset + 10);
        ctx.lineTo(centerX + drawingData.xRadiusOuterEllipse, bottom + rulersOffset - 10);

        ctx.moveTo(left - rulersOffset, centerY - drawingData.yRadiusOuterEllipse);
        ctx.lineTo(left - rulersOffset, centerY + drawingData.yRadiusOuterEllipse);
        ctx.moveTo(left - rulersOffset + 10, centerY - drawingData.yRadiusOuterEllipse);
        ctx.lineTo(left - rulersOffset - 10, centerY - drawingData.yRadiusOuterEllipse);
        ctx.moveTo(left - rulersOffset + 10, centerY + drawingData.yRadiusOuterEllipse);
        ctx.lineTo(left - rulersOffset - 10, centerY + drawingData.yRadiusOuterEllipse);
        ctx.stroke();

        ctx.strokeStyle="rgb(255 , 46 , 46)";
        ctx.beginPath();
        ctx.moveTo(centerX - drawingData.xRadiusInnerEllipse, top - rulersOffset);
        ctx.lineTo(centerX + drawingData.xRadiusInnerEllipse, top - rulersOffset);
        ctx.moveTo(centerX - drawingData.xRadiusInnerEllipse, top - rulersOffset + 10);
        ctx.lineTo(centerX - drawingData.xRadiusInnerEllipse, top - rulersOffset - 10);
        ctx.moveTo(centerX + drawingData.xRadiusInnerEllipse, top - rulersOffset + 10);
        ctx.lineTo(centerX + drawingData.xRadiusInnerEllipse, top - rulersOffset - 10);

        ctx.moveTo(right + rulersOffset, centerY - drawingData.yRadiusInnerEllipse);
        ctx.lineTo(right + rulersOffset, centerY + drawingData.yRadiusInnerEllipse);
        ctx.moveTo(right + rulersOffset + 10, centerY - drawingData.yRadiusInnerEllipse);
        ctx.lineTo(right + rulersOffset - 10, centerY - drawingData.yRadiusInnerEllipse);
        ctx.moveTo(right + rulersOffset + 10, centerY + drawingData.yRadiusInnerEllipse);
        ctx.lineTo(right + rulersOffset - 10, centerY + drawingData.yRadiusInnerEllipse);
        ctx.stroke();
        
        ctx.font = "13px Roboto";
        ctx.fillStyle = "grey";
        ctx.textBaseline = "top";
        ctx.textAlign = "center";
        ctx.fillText(`Max Vertical: ${Math.round(drawingData.xRadiusOuterEllipse * 2 / scaling)} m`, centerX, bottom + rulersOffset + 10);

        ctx.save();
        ctx.translate(left - rulersOffset - 10, centerY);
        ctx.rotate(-Math.PI / 2);
        ctx.textBaseline = "bottom";
        ctx.fillText(`Max Horizontal: ${Math.round(drawingData.yRadiusOuterEllipse * 2 / scaling)} m`, 0, 0);
        ctx.restore();
        
        ctx.fillStyle = "rgb(255 , 46 , 46)";
        ctx.textBaseline = "bottom";
        ctx.fillText(`50% Hits Vertical: ${Math.round(drawingData.xRadiusInnerEllipse * 2 / scaling)} m`, centerX, top - rulersOffset - 10);

        ctx.translate(right + rulersOffset + 10, centerY);
        ctx.rotate(Math.PI / 2);
        ctx.textBaseline = "bottom";
        ctx.fillText(`50% Hits Horizontal: ${Math.round(drawingData.yRadiusInnerEllipse * 2 / scaling)} m`, 0, 0);
        ctx.restore();
    }
}