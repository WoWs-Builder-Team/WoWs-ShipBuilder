export function DrawVisualizer()
{
    const canvas = document.getElementById("visualizer");    
    const ctx = canvas.getContext("2d");
    const width = canvas.clientWidth;
    const height = canvas.clientHeight;
    //set the canvas drawing surface to the actual size of the canvas html element
    canvas.width= width;
    canvas.height = height;
    
    ctx.strokeStyle = "black";
    ctx.fillStyle = "cornflowerblue";
    ctx.fillRect(0,0, canvas.clientWidth, canvas.clientHeight);

    ctx.fillStyle = "lightskyblue";
    const shipWidth = width * 0.5;
    const shipHeight = height * 0.9;
    ctx.beginPath();
    ctx.ellipse( width / 2, height /2, shipWidth/2, shipHeight /2, 0, 0, 2 * Math.PI);
    ctx.fill();
    ctx.stroke();
    
    drawGun(ctx, width / 2, 200, Math.PI/4);

    drawSlice(ctx, -Math.PI/3, Math.PI * 24/18, width / 2, 200);
 
}

function getTurretPath()
{
    return new Path2D("M 9.5250002,9.6572912 8.5989582,10.451041 H 2.5135415 l -0.79375,-0.7937498 h -1.5875 v -1.322916 h 0.79375 V 6.7468749 c 0,-0.5291667 0.3002661,-1.3683176 0.9634748," +
        "-1.6052961 0.449291,-0.1888714 0.8886086,-0.2467873 1.4177752,-0.2467873 v -4.7625 h 1.5875 v 4.7625 h 1.5875 v -4.7625 h 1.5875 v 4.7625 c 0.5291667,0 1.2930047,0.095838 1.8008907," +
        "0.4810755 0.4023848,0.3919374 0.5803588,1.1064245 0.5803588,1.5319045 v 1.4266037 h 0.79375 v 1.322916 z");
}

function getTurretGunsPath()
{
    return new Path2D("m 6.4822914,4.8947915 v 1.0583334 h 1.5875 V 4.8947915 m -4.7624999,0 v 1.0583334 h 1.5875 V 4.8947915");
}

function getScaling()
{
    return 10;
}

function drawGun(context, x, y, angle)
{
    const turret = getTurretPath();
    const turretWidth = 11.37708;
    const turretHeight = 10.58333;
    const guns = getTurretGunsPath();
    const scaling = getScaling();
    
    context.fillStyle = "gray";
    context.strokeStyle = "black";
    context.lineWidth  = 1 / scaling;
    
    const xTranslation = x - (turretWidth * 0.5 * scaling);
    const yTranslation = y - (turretHeight * 0.5 * scaling);

    context.translate(xTranslation, yTranslation);
    context.scale(scaling, scaling);
    context.translate(turretWidth * 0.5 , turretWidth * 0.5 );
    context.rotate(angle);
    context.translate(-turretWidth* 0.5 , -turretWidth* 0.5 );
    
    context.fill(turret);
    
    context.stroke(turret);
    context.stroke(guns);
    context.resetTransform();
}

function drawSlice(ctx, startingAngle, endingAngle, x, y)
{
    const scaling = getScaling();
    const offset = 3 * scaling;
    const radius = 80;
    
    ctx.translate(0, offset);
    
    ctx.beginPath();
    ctx.fillStyle = "rgba(0,0,0,0.60)";
    ctx.moveTo(x,y);

    ctx.arc(x,y,radius,startingAngle,endingAngle,false);
    ctx.closePath();
    ctx.fill();
    ctx.resetTransform();
}