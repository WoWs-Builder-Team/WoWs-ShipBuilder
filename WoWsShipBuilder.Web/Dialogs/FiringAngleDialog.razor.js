const verticalOffset = 30;
const artilleryIconWidth = 11.37708;
const artilleryIconHeight = 10.58333;
const torpedoIconWidth = 8.73125;
const torpedoIconHeight = 10.38521;

/**
 * @type {Array.<{hPos: number, vPos: number, baseAngle: number, sector: Array.<number>, deadZones: Array.<Array.<number>>}>}
 */
let gunContainerCache;
let isArtilleryCache;
/**
 * @type {HTMLCanvasElement} The canvas element to draw on.
 */
let canvas;

/**
 * Function to draw firing angles on the canvas.
 *
 * @param {Array.<{hPos: number, vPos: number, baseAngle: number, sector: Array.<number>, deadZones: Array.<Array.<number>>}>} gunContainers The list of gun data containers.
 * @param isArtillery Indicates whether the gun data containers represent artillery guns or torpedo launchers.
 */
export function drawVisualizer(gunContainers, isArtillery) {
    gunContainerCache = gunContainers;
    isArtilleryCache = isArtillery;
    canvas = document.getElementById("visualizer");
    window.removeEventListener("resize", redraw, false);
    window.addEventListener("resize", redraw, false);
    
    let myFont = new FontFace('RobotoBold', 'url(/assets/font/roboto/roboto-v29-latin_cyrillic-700.woff2)');

    myFont.load().then(function(font) {
        document.fonts.add(font);
        redraw();
    });
}

export function cleanSubscriptions() {
    window.removeEventListener("resize", redraw, false);
    canvas = null;
    gunContainerCache = null;
}

function redraw() {
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    configureCanvas(canvas);
    const shipRect = drawBasics(ctx);

    const radius = shipRect.width * 0.25;
    const scale = radius * 2 / (isArtilleryCache ? artilleryIconWidth : radius * 2 / torpedoIconWidth);
    let texts = [];
    gunContainerCache.forEach(gun => {
        const maxHeight = shipRect.height - (2 * radius);
        const verticalPosition = ((maxHeight / 8) * (gun.vPos + 1.5)) + verticalOffset + radius;
        let horizontalPosition = calculateHorizontalBorderCoordinate(verticalPosition, shipRect, gun.hPos);
        if (gun.hPos === 1) {
            horizontalPosition = shipRect.left + (shipRect.width / 2);
        }

        drawGun(ctx, scale, horizontalPosition, verticalPosition, degreeToRadian(gun.baseAngle), isArtilleryCache);
        drawSlice(ctx, gun.sector[0], gun.sector[1], horizontalPosition, verticalPosition, radius, gun.baseAngle, scale, false);
        gun.deadZones.forEach(deadZone => {
            drawSlice(ctx, deadZone[0], deadZone[1], horizontalPosition, verticalPosition, radius, gun.baseAngle, scale, true);
        });

        let startDegrees = gun.sector[0];
        let endDegrees = gun.sector[1];
        if (startDegrees > 179) {
            startDegrees -= 360;
        }
        if (endDegrees > 180) {
            endDegrees -= 360;
        }
        console.log(startDegrees, endDegrees);
        texts.push({text: `${parseFloat(startDegrees.toFixed(2))}° to ${parseFloat(endDegrees.toFixed(2))}°`, x: horizontalPosition, y: verticalPosition})
    });

    ctx.resetTransform();
    ctx.font = "18px RobotoBold";
    ctx.textAlign = "center";
    ctx.textBaseline = "top";
    ctx.fillStyle = "black";
    texts.forEach(txt => {
        ctx.fillText(txt.text, txt.x, txt.y);
    });
}

/**
 * Configures the html canvas element.
 *
 * @param {HTMLCanvasElement} canvas The canvas element to configure.
 */
function configureCanvas(canvas) {
    canvas.width = canvas.clientWidth;
    canvas.height = canvas.clientHeight;
}

/**
 *
 * @param {CanvasRenderingContext2D} ctx Context to render content on.
 * @return {DOMRect} A DOMRect representing a rectangle directly around the ship ellipse.
 */
function drawBasics(ctx) {
    ctx.strokeStyle = "black";
    ctx.fillStyle = "cornflowerblue";
    ctx.fillRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    const shipWidth = (ctx.canvas.height - (verticalOffset * 2)) / 3.5;
    const shipHeight = ctx.canvas.height - (verticalOffset * 2);

    const horizontalCenter = ctx.canvas.width / 2;
    const verticalCenter = ctx.canvas.height / 2;

    ctx.fillStyle = "lightskyblue";
    ctx.beginPath();
    ctx.ellipse(horizontalCenter, verticalCenter, shipWidth / 2, shipHeight / 2, 0, 0, 2 * Math.PI);
    ctx.fill();
    ctx.stroke();
    ctx.closePath();

    return new DOMRect(horizontalCenter - shipWidth / 2, verticalOffset, shipWidth, shipHeight);
}

/**
 *
 * @param {number} y The y coordinate of the point.
 * @param {DOMRect} shipRect
 * @param {number} hPos The horizontal turret position.
 * @return {number}
 */
function calculateHorizontalBorderCoordinate(y, shipRect, hPos) {
    const h = shipRect.left + (shipRect.width / 2);
    const k = shipRect.top + (shipRect.height / 2);
    const a = shipRect.height / 2;
    const b = shipRect.width / 2;
    const tmp = Math.sqrt(Math.pow(b, 2) - ((Math.pow(y - k, 2) * Math.pow(b, 2)) / Math.pow(a, 2)));
    let result;
    if (hPos > 1) {
        result = h - tmp;
    } else {
        result = h + tmp;
    }

    return result;
}

/**
 *
 * @return {Path2D}
 */
function getTurretPath() {
    return new Path2D("M 9.5250002,9.6572912 8.5989582,10.451041 H 2.5135415 l -0.79375,-0.7937498 h -1.5875 v -1.322916 h 0.79375 V 6.7468749 c 0,-0.5291667 0.3002661,-1.3683176 0.9634748," +
        "-1.6052961 0.449291,-0.1888714 0.8886086,-0.2467873 1.4177752,-0.2467873 v -4.7625 h 1.5875 v 4.7625 h 1.5875 v -4.7625 h 1.5875 v 4.7625 c 0.5291667,0 1.2930047,0.095838 1.8008907," +
        "0.4810755 0.4023848,0.3919374 0.5803588,1.1064245 0.5803588,1.5319045 v 1.4266037 h 0.79375 v 1.322916 z");
}

function getTurretGunsPath() {
    return new Path2D("m 6.4822914,4.8947915 v 1.0583334 h 1.5875 V 4.8947915 m -4.7624999,0 v 1.0583334 h 1.5875 V 4.8947915");
}

function getTorpedoLauncherPath() {
    return new Path2D("M 0,9.7895832 H 8.4666666 M 0,8.2020833 H 8.4666666 M 0,4.4979166 c 0.52916666,0 8.4666666,0 " +
        "8.4666666,0 M 0,1.5875 H 8.4666666 M 6.4771355,9.926714 V 1.6184604 c 0.2645833,-1.71052293 1.8520833,-1.71052293 " +
        "2.1166667,0 V 9.926714 c -0.7804407,0.807197 -1.4665162,0.597218 -2.1166667,0 z m -2.1166667,0 V 1.6184604 c " +
        "0.2645833,-1.71052293 1.8520833,-1.71052293 2.1166667,0 V 9.926714 c -0.6966683,0.689771 -1.4018189,0.721197 " +
        "-2.1166667,0 z m -2.1166667,0 V 1.6184604 c 0.2645833,-1.71052293 1.8520833,-1.71052293 2.1166667,0 V 9.926714 " +
        "c -0.6925275,0.682249 -1.3971989,0.728549 -2.1166667,0 z m -2.1166666,0 V 1.6184604 c 0.26458333,-1.71052296 " +
        "1.8520833,-1.71052296 2.1166667,0 V 9.926714 c -0.6978745,0.691941 -1.40312901,0.719063 -2.1166667,0 z");
}

function degreeToRadian(degrees) {
    return degrees * Math.PI / 180;
}

/**
 * @param {CanvasRenderingContext2D} context
 * @param {number} scale
 * @param x
 * @param y
 * @param radians
 * @param {boolean} isArtillery
 */
function drawGun(context, scale, x, y, radians, isArtillery) {
    const turret = isArtillery ? getTurretPath() : getTorpedoLauncherPath();
    const turretWidth = isArtillery ? artilleryIconWidth : torpedoIconWidth;
    const turretHeight = isArtillery ? artilleryIconHeight : torpedoIconHeight;

    context.fillStyle = "gray";
    context.strokeStyle = "black";
    context.lineWidth = 1 / scale;

    const xTranslation = x - (turretWidth * 0.5 * scale);
    const yTranslation = y - (turretHeight * 0.5 * scale);

    context.translate(xTranslation, yTranslation);
    context.scale(scale, scale);
    context.translate(turretWidth * 0.5, turretWidth * 0.5);
    context.rotate(radians);
    context.translate(-turretWidth * 0.5, -turretWidth * 0.5);

    context.fill(turret);

    context.stroke(turret);
    if (isArtillery) {
        context.stroke(getTurretGunsPath());
    }
    context.resetTransform();
}


function drawSlice(ctx, startDegrees, endDegrees, x, y, radius, baseAngle, scaling, isDeadZone) {
    ctx.rotate(degreeToRadian(baseAngle));
    ctx.translate(0, 1.2 * scaling);
    ctx.rotate(degreeToRadian(-baseAngle));

    ctx.beginPath();
    ctx.fillStyle = !isDeadZone ? "rgba(180,180,180,0.65)" : "rgba(220, 0, 0, 0.50)";
    ctx.moveTo(x, y);

    if (startDegrees === endDegrees) {
        startDegrees = 0;
        endDegrees = 360;
    }
    ctx.arc(x, y, radius * 2, degreeToRadian(startDegrees - 90), degreeToRadian(endDegrees - 90), false);
    ctx.closePath();
    ctx.fill();
    ctx.resetTransform();
}
