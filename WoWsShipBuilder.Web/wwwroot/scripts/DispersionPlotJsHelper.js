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
        if(canvas.width < 156){
            canvas.width = 156;
        }
        if(canvas.height < 156){
            canvas.height = 156;
        }
        const width = canvas.width;
        const height = canvas.height;
        const centerX = width / 2;
        const centerY = height / 2;
        const canvasTop = ellipseOffset / 2;
        const canvasBottom = height - ellipseOffset / 2;
        const canvasRight = width - ellipseOffset / 2;
        const canvasLeft = ellipseOffset / 2;
        const outerEllipseTop = centerY - drawingData.yRadiusOuterEllipse;
        const outerEllipseBottom = centerY + drawingData.yRadiusOuterEllipse;
        const outerEllipseRight = centerX + drawingData.xRadiusOuterEllipse;
        const outerEllipseLeft = centerX - drawingData.xRadiusOuterEllipse;
        const innerEllipseTop = centerY - drawingData.yRadiusInnerEllipse;
        const innerEllipseBottom = centerY + drawingData.yRadiusInnerEllipse;
        const innerEllipseRight = centerX + drawingData.xRadiusInnerEllipse;
        const innerEllipseLeft = centerX - drawingData.xRadiusInnerEllipse;

        ctx.clearRect(0, 0, width, height);
        
        ctx.lineWidth = 1;
        ctx.strokeStyle = "gray";
        ctx.beginPath();
        ctx.moveTo(centerX, outerEllipseTop);
        ctx.lineTo(centerX, outerEllipseBottom);
        ctx.moveTo(outerEllipseLeft, centerY);
        ctx.lineTo(outerEllipseRight, centerY);
        ctx.stroke();

        if (Object.values(fusoReference)[0] !== 0 && Object.values(fusoReference)[1] !== 0){
            ctx.lineWidth = 4;
            ctx.strokeStyle = "rgba(0, 0, 0, 0.5)";
            ctx.fillStyle = "rgba(0, 0, 0, 0.25)";
            ctx.beginPath();
            ctx.ellipse(centerX, centerY, Object.values(fusoReference)[0], Object.values(fusoReference)[1], 0, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke();
        }
        
        ctx.lineWidth = 4;
        ctx.strokeStyle = "gray";
        ctx.fillStyle = "rgba(128, 128, 128, 0.1)";
        ctx.beginPath();
        ctx.ellipse(centerX, centerY, drawingData.xRadiusOuterEllipse, drawingData.yRadiusOuterEllipse, 0, 0, 2 * Math.PI);
        ctx.fill();
        ctx.stroke();

        ctx.lineWidth = 4;
        ctx.strokeStyle = "rgba(255 , 46 , 46, 0.5)";
        ctx.fillStyle = "rgba(255 , 46 , 46, 0.1)";
        ctx.beginPath();
        ctx.ellipse(centerX, centerY, drawingData.xRadiusInnerEllipse, drawingData.yRadiusInnerEllipse, 0, 0, 2 * Math.PI);
        ctx.fill();
        ctx.stroke();

        ctx.lineWidth = 1;
        ctx.strokeStyle = "transparent";
        ctx.fillStyle = "rgba(218,165,32, 0.33)";
        let pointRadius = 6;
        if(scaling < 1){
            pointRadius *= scaling;
        }
        drawingData.hitPoints.forEach(function (point){
            ctx.beginPath();
            ctx.ellipse(centerX - Object.values(point)[0], centerY - Object.values(point)[1], pointRadius, pointRadius, 0, 0, 2 * Math.PI);
            ctx.fill();
            ctx.stroke();
        });
        
        ctx.lineWidth = 1;
        ctx.strokeStyle = "gray";
        ctx.beginPath();
        ctx.moveTo(outerEllipseLeft, canvasBottom + rulersOffset);
        ctx.lineTo(outerEllipseRight, canvasBottom + rulersOffset);
        ctx.moveTo(outerEllipseLeft, canvasBottom + rulersOffset + 10);
        ctx.lineTo(outerEllipseLeft, canvasBottom + rulersOffset - 10);
        ctx.moveTo(outerEllipseRight, canvasBottom + rulersOffset + 10);
        ctx.lineTo(outerEllipseRight, canvasBottom + rulersOffset - 10);

        ctx.moveTo(canvasLeft - rulersOffset, outerEllipseTop);
        ctx.lineTo(canvasLeft - rulersOffset, outerEllipseBottom);
        ctx.moveTo(canvasLeft - rulersOffset + 10, outerEllipseTop);
        ctx.lineTo(canvasLeft - rulersOffset - 10, outerEllipseTop);
        ctx.moveTo(canvasLeft - rulersOffset + 10, outerEllipseBottom);
        ctx.lineTo(canvasLeft - rulersOffset - 10, outerEllipseBottom);
        ctx.stroke();

        ctx.strokeStyle = "rgb(255 , 46 , 46)";
        ctx.beginPath();
        ctx.moveTo(innerEllipseLeft, canvasTop - rulersOffset);
        ctx.lineTo(innerEllipseRight, canvasTop - rulersOffset);
        ctx.moveTo(innerEllipseLeft, canvasTop - rulersOffset + 10);
        ctx.lineTo(innerEllipseLeft, canvasTop - rulersOffset - 10);
        ctx.moveTo(innerEllipseRight, canvasTop - rulersOffset + 10);
        ctx.lineTo(innerEllipseRight, canvasTop - rulersOffset - 10);

        ctx.moveTo(canvasRight + rulersOffset, innerEllipseTop);
        ctx.lineTo(canvasRight + rulersOffset, innerEllipseBottom);
        ctx.moveTo(canvasRight + rulersOffset + 10, innerEllipseTop);
        ctx.lineTo(canvasRight + rulersOffset - 10, innerEllipseTop);
        ctx.moveTo(canvasRight + rulersOffset + 10, innerEllipseBottom);
        ctx.lineTo(canvasRight + rulersOffset - 10, innerEllipseBottom);
        ctx.stroke();
        
        ctx.font = "13px Roboto";
        ctx.fillStyle = "grey";
        ctx.textBaseline = "top";
        ctx.textAlign = "center";
        ctx.fillText(`Max Vertical: ${Math.round(drawingData.xRadiusOuterEllipse * 2 / scaling)} m`, centerX, canvasBottom + rulersOffset + 10);

        ctx.save();
        ctx.translate(canvasLeft - rulersOffset - 10, centerY);
        ctx.rotate(-Math.PI / 2);
        ctx.textBaseline = "bottom";
        ctx.fillText(`Max Horizontal: ${Math.round(drawingData.yRadiusOuterEllipse * 2 / scaling)} m`, 0, 0);
        ctx.restore();
        
        ctx.fillStyle = "rgb(255 , 46 , 46)";
        ctx.textBaseline = "bottom";
        ctx.fillText(`50% Hits Vertical: ${Math.round(drawingData.xRadiusInnerEllipse * 2 / scaling)} m`, centerX, canvasTop - rulersOffset - 10);

        ctx.translate(canvasRight + rulersOffset + 10, centerY);
        ctx.rotate(Math.PI / 2);
        ctx.textBaseline = "bottom";
        ctx.fillText(`50% Hits Horizontal: ${Math.round(drawingData.yRadiusInnerEllipse * 2 / scaling)} m`, 0, 0);
        ctx.restore();
    }
}