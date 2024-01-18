const verticalOffset = 30;
const artilleryIconWidth = 11.37708;
const artilleryIconHeight = 10.58333;
const torpedoIconWidth = 8.73125;
const torpedoIconHeight = 10.38521;

const turretIndicatorScalingFactor = 0.065;
const sliceIndicatorScalingFactor = 8;

const mouseOffsetScalingFactor = 2;

/**
 * @type {Array.<{hPos: number, vPos: number, baseAngle: number, sector: Array.<number>, deadZones: Array.<Array.<number>>}>}
 */
let gunContainerCache;
/**
 * @type {Array<TurretData>}
 */
let turretDataCache = [];
/**
 * @type {HTMLCanvasElement} The canvas element to draw on.
 */
let canvas;

let ratio;
let isMouseInside;
let mouseX;
let mouseY;
/**
 * @type {DOMRect}
 */
let shipRect;

/**
 * Function to draw firing angles on the canvas.
 *
 * @param {Array.<{hPos: number, vPos: number, baseAngle: number, sector: Array.<number>, deadZones: Array.<Array.<number>>}>} gunContainers The list of gun data containers.
 * @param isArtillery Indicates whether the gun data containers represent artillery guns or torpedo launchers.
 */
export function drawVisualizer(gunContainers, isArtillery) {
    gunContainerCache = gunContainers;
    ratio = window.devicePixelRatio;
    canvas = document.getElementById("visualizer");
    window.removeEventListener("resize", resize, false);
    window.addEventListener("resize", resize, false);

    let myFont = new FontFace('RobotoBold', 'url(/_content/WoWsShipBuilder.Common/assets/font/roboto/roboto-v29-latin_cyrillic-700.woff2)');

    myFont.load().then(function(font) {
        document.fonts.add(font);
        canvas.addEventListener("mouseenter", mouseEnter, false)
        canvas.addEventListener("mousemove", mouseMove, false);
        canvas.addEventListener("mouseleave", mouseLeave, false)
        redraw();
    });
}


export function cleanSubscriptions() {
    window.removeEventListener("resize", resize, false);
    canvas.removeEventListener("mouseenter", mouseEnter, false)
    canvas.removeEventListener("mousemove", mouseMove, false);
    canvas.removeEventListener("mouseleave", mouseLeave, false)
    canvas = null;
    gunContainerCache = null;
    turretDataCache = [];
}

function resize() {
    turretDataCache = [];
    redraw();
}

/**
 * @param {MouseEvent} event
 */
function mouseMove(event) {
    mouseX = event.offsetX;
    mouseY = event.offsetY;
    redraw();
}

function mouseEnter() {
    isMouseInside = true
}

function mouseLeave() {
    isMouseInside = false
    redraw();
}

function processTurretData(radius)
{
    const data = [];
    gunContainerCache.forEach(gun => {

        // we add 1.6 because that's the minimum Y value from game data. How the fuck the minimum is not 0, i don't know. Wg is weird
        // we add radius because the position needs to be the center of the circle, and we add the vertical offset so the origin is the canvas, not the ship rectangle.
        const verticalPosition =  (gun.vPos + 1.6) * (shipRect.height / 8) + radius + verticalOffset;

        let horizontalPosition = calculateHorizontalBorderCoordinate(verticalPosition, shipRect, gun.hPos);
        if (gun.hPos === 1) {
            horizontalPosition = shipRect.left + (shipRect.width / 2);
        }

        let startDegrees = gun.sector[0];
        let endDegrees = gun.sector[1];
        if (startDegrees > 179) {
            startDegrees -= 360;
        }
        if (endDegrees > 180) {
            endDegrees -= 360;
        }
        const text = `${parseFloat(startDegrees.toFixed(2))}° to ${parseFloat(endDegrees.toFixed(2))}°`;
        const turretData = new TurretData(horizontalPosition, verticalPosition, gun.baseAngle, gun.sector, gun.deadZones, text);
        data.push(turretData)
    });
    return data;
}
function redraw()
{
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, ctx.canvas.width, ctx.canvas.height);
    configureCanvas(canvas, ratio);
    shipRect = drawBasics(ctx);
    const radius = shipRect.width * turretIndicatorScalingFactor;

    if (turretDataCache.length === 0) {
        turretDataCache = processTurretData(radius);
    }

    let selectedTurrets = turretDataCache;

    if (isMouseInside)
    {
        let closeTurrets = turretDataCache.filter(turret => IsMouseInRadius(turret.hPos, turret.vPos, radius));
        if (closeTurrets.length > 0)
        {
            selectedTurrets = closeTurrets;
        }
    }

    selectedTurrets.forEach(turretData => {
        //draw the angle slices
        drawSlice(ctx, turretData.sector[0], turretData.sector[1], turretData.hPos, turretData.vPos, radius, turretData.baseAngle, false);
        turretData.deadZones.forEach(deadZone => {
            drawSlice(ctx, deadZone[0], deadZone[1], turretData.hPos, turretData.vPos, radius, turretData.baseAngle, true);
        });
    })

    turretDataCache.forEach(turretData => {
        drawGun(ctx, turretData, radius, ratio);
    });

    selectedTurrets.forEach(TurretData => {
        drawAngleText(ctx, TurretData)
    })
}

/**
 * @param {CanvasRenderingContext2D} context
 * @param {TurretData} turretData
 * @param {number} radius
 */
function drawGun(context, turretData, radius, ratio) {

    // draw the circle representing the
    context.fillStyle = "gray";
    context.strokeStyle = "black";
    context.lineWidth = 1;

    context.beginPath()
    context.arc(turretData.hPos, turretData.vPos, radius , 0, 2 * Math.PI);
    context.closePath()
    context.stroke();
    context.fill()

    context.resetTransform();
}

/**
 * @param {CanvasRenderingContext2D} ctx
 * @param {number} startDegrees
 * @param {number} endDegrees
 * @param {number} x
 * @param {number} y
 * @param {number} radius
 * @param {number} baseAngle
 * @param {boolean} isDeadZone
 */
function drawSlice(ctx, startDegrees, endDegrees, x, y, radius, baseAngle, isDeadZone) {
    ctx.rotate(degreeToRadian(baseAngle));
    ctx.translate(0, 1.2);
    ctx.rotate(degreeToRadian(-baseAngle));

    ctx.beginPath();
    ctx.fillStyle = !isDeadZone ? "rgba(180,180,180,0.65)" : "rgba(220, 0, 0, 0.50)";

    ctx.moveTo(x, y);

    if (startDegrees === endDegrees) {
        startDegrees = 0;
        endDegrees = 360;
    }
    ctx.arc(x, y, radius * sliceIndicatorScalingFactor, degreeToRadian(startDegrees - 90), degreeToRadian(endDegrees - 90), false);
    ctx.closePath();
    ctx.fill();
    ctx.resetTransform();
}

/**
 *
 * @param {CanvasRenderingContext2D} ctx
 * @param {TurretData} turretData
 */
function drawAngleText(ctx, turretData){
    ctx.scale(ratio,ratio)
    ctx.font = "18px RobotoBold";
    ctx.textAlign = "center";
    ctx.textBaseline = "top";
    ctx.fillStyle = "black";
    ctx.fillText(turretData.angleText, turretData.hPos / ratio, turretData.vPos / ratio);
    ctx.resetTransform();
}

/**
 * Configures the html canvas element.
 *
 * @param {HTMLCanvasElement} canvas The canvas element to configure.
 * @param {number} ratio The ration from the devicePixelRation to use for better text rendering.
 */
function configureCanvas(canvas, ratio) {
    canvas.width = canvas.clientWidth * ratio;
    canvas.height = canvas.clientHeight * ratio;
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

    // ctx.fillStyle = "lightskyblue";
    ctx.fillStyle = "#0c1791";
    ctx.strokeStyle = "#b3b3b3";
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

function degreeToRadian(degrees) {
    return degrees * Math.PI / 180;
}


//@param {Array.<{hPos: number, vPos: number, baseAngle: number, sector: Array.<number>, deadZones: Array.<Array.<number>>}>} gunContainers The list of gun data containers.
/**
 * @param {number} hPos
 * @param {number} vPos
 * @param {number} baseAngle
 * @param {Array<number>} sector
 * @param {Array<Array.<number>>} deadZones
 * @param {string} angleText
 * @constructor
 */
function TurretData(hPos, vPos, baseAngle, sector, deadZones, angleText)
{
    this.hPos = hPos;
    this.vPos = vPos;
    this.baseAngle = baseAngle;
    this.sector = sector;
    this.deadZones = deadZones;
    this.angleText = angleText;
}

function IsMouseInRadius(x, y, radius)
{
    const mouseOffset = radius * mouseOffsetScalingFactor;
    return Math.abs(x - (mouseX *  window.devicePixelRatio)) < mouseOffset && Math.abs(y - (mouseY *  window.devicePixelRatio)) < mouseOffset
}
