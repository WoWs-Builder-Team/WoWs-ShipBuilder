export function drawDepthChargeDamageDistributionChart(id, dataRecord){
    const canvas = document.getElementById(id);
    if(canvas === null){
        return;
    }
    let ratio = window.devicePixelRatio;
    canvas.width = canvas.clientWidth * ratio;
    canvas.height = canvas.clientWidth * ratio;
    const width = canvas.width;
    const ctx = canvas.getContext("2d")
    ctx.strokeStyle = "white"
    ctx.lineWidth  = 2
    const centre = width / 2
    const dcDmg = dataRecord.dcDmg
    const splashRadius = dataRecord.splashRadius
    const pointsOfDmg = dataRecord.pointsOfDmg
    const mapLength = Object.keys(pointsOfDmg).length
    Object.keys(pointsOfDmg).sort().forEach((key, index) =>
        drawDistribution(ctx, pointsOfDmg[key][0], 1 / (mapLength - index), centre, dcDmg, key, splashRadius, ratio)
    )
    drawExtraElements(ctx, centre)
    ctx.resetTransform()
}
function drawDistribution(ctx, radiusCoeff, opacity, centre, dcDmg, dmgCoeff, splashRadius, ratio){
    const offset = 25;
    const radius = radiusCoeff * Math.abs(centre - offset)
    ctx.fillStyle = `rgba(70,130,180,${opacity})`
    ctx.beginPath()
    ctx.ellipse(centre, centre, radius, radius, 0, 0, 2 * Math.PI)
    ctx.fill()
    ctx.stroke()

    const rad = radius * Math.sqrt(2) / 2
    ctx.scale(ratio, ratio);
    ctx.font = "13px Roboto"
    ctx.fillStyle = "white"
    ctx.textAlign="center"
    ctx.textBaseline="top"
    ctx.translate((centre + rad - 4) / ratio, (centre - rad + 4) / ratio)
    ctx.rotate(Math.PI/4)
    ctx.fillText(`${Math.round(dcDmg * dmgCoeff)}`, 0, 0)
    ctx.resetTransform();
    ctx.scale(ratio, ratio);
    ctx.textBaseline="bottom"
    ctx.translate((centre - rad - 1) / ratio, (centre - rad - 1) / ratio)
    ctx.rotate(-Math.PI/4)
    ctx.fillText(`${Math.round(splashRadius * radiusCoeff)} m`, 0, 0)
    ctx.resetTransform();
}
function drawExtraElements(ctx, centre){
    const offset = 5
    const end = (centre * 2) - offset
    ctx.fillStyle = "white"
    ctx.beginPath()
    ctx.ellipse(centre, centre, 2.5, 2.5, 0, 0, 2 * Math.PI)
    ctx.fill()
    ctx.lineWidth  = 1
    ctx.setLineDash([3, 5])
    ctx.fillStyle = "rgba(128,128,128,0.5)"
    ctx.beginPath()
    ctx.moveTo(centre, offset)
    ctx.lineTo(centre, end)
    ctx.moveTo(offset, centre)
    ctx.lineTo(end, centre)
    ctx.stroke()
}