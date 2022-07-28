export function DrawDispersionPlotBatched(data, scaling, fusoReference, text){
    for(const key in data){
        const canvas = document.getElementById(key);
        const ctx = canvas.getContext("2d");
        
        const drawingData = data[key];
        
        const ellipseOffset = 100;
        const rulersOffset = 20;
        
        canvas.width = drawingData.xRadiusOuterEllipse * 2 + ellipseOffset;
        canvas.height = drawingData.yRadiusOuterEllipse * 2 + ellipseOffset;

        if (fusoReference.width !== 0 && fusoReference.length !== 0){
            if(fusoReference.width > drawingData.xRadiusOuterEllipse){
                canvas.width = fusoReference.width * 2 + ellipseOffset;
            }
            if(fusoReference.length > drawingData.yRadiusOuterEllipse){
                canvas.height = fusoReference.length * 2 + ellipseOffset;
            }
        }
        
        ctx.font = "14px Roboto";
        const topText = `${text[2]} ${Math.round(drawingData.xRadiusInnerEllipse * 2 / scaling)} ${text[4]}`;
        const bottomText = `${text[0]} ${Math.round(drawingData.xRadiusOuterEllipse * 2 / scaling)} ${text[4]}`;
        const leftText  = `${text[1]} ${Math.round(drawingData.yRadiusOuterEllipse * 2 / scaling)} ${text[4]}`;
        const rightText = `${text[3]} ${Math.round(drawingData.yRadiusInnerEllipse * 2 / scaling)} ${text[4]}`;
        const minSize = ctx.measureText(
            [topText, bottomText, leftText, rightText].reduce(
                function (a, b) {
                    return a.length > b.length ? a : b;
                }
        )).width;
        if(canvas.width < minSize){
            canvas.width = minSize;
        }
        if(canvas.height < minSize){
            canvas.height = minSize;
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

        if (fusoReference.width !== 0 && fusoReference.length !== 0){
            ctx.lineWidth = 4;
            ctx.strokeStyle = "rgba(0, 0, 0, 0.5)";
            ctx.fillStyle = "rgba(0, 0, 0, 0.25)";
            ctx.beginPath();
            ctx.ellipse(centerX, centerY, fusoReference.width, fusoReference.length, 0, 0, 2 * Math.PI);
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
        drawingData.hitPoints.forEach((point) => {
            ctx.beginPath();
            ctx.ellipse(centerX - point.x, centerY - point.y, pointRadius, pointRadius, 0, 0, 2 * Math.PI);
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
        
        ctx.fillStyle = "grey";
        ctx.textBaseline = "top";
        ctx.textAlign = "center";
        ctx.fillText(bottomText, centerX, canvasBottom + rulersOffset + 10);

        ctx.textBaseline = "bottom";
        ctx.translate(canvasLeft - rulersOffset - 10, centerY);
        ctx.rotate(-Math.PI / 2);
        ctx.fillText(leftText, 0, 0);
        ctx.resetTransform();
        
        ctx.fillStyle = "rgb(255 , 46 , 46)";
        ctx.fillText(topText, centerX, canvasTop - rulersOffset - 10);
        ctx.translate(canvasRight + rulersOffset + 10, centerY);
        ctx.rotate(Math.PI / 2);
        ctx.fillText(rightText, 0, 0);
        ctx.resetTransform();
    }
}