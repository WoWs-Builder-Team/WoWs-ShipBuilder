export async function downloadBuildImage (id, imgName, buildString, encodeBuild) {
    let img = "";
    await html2canvas(document.querySelector("#" + id), {
        backgroundColor: "#282828",
        useCORS: true,
        allowTaint: true,
        ignoreElements: function( element ) {
            if( 'editBuildNameIcon' === element.id ) {
                return true;
            }},
    }).then( function( canvas ) {
        if (encodeBuild) {
            encodeBuildString(canvas, buildString);
        }
        img = canvas.toDataURL("image/png", 1.0);
        });
    let d = document.createElement("a");
    d.href = img;
    d.download = imgName + ".png";
    d.click();
}

function encodeBuildString (canvas, buildString) {
    const ctx = canvas.getContext("2d");
    const pixels = ctx.getImageData(0, 0, canvas.width, canvas.height);
    const data = pixels.data;
    let state = 'encoding';
    let charIndex = 0;
    let charValue = 0;
    let pixelElementIndex = 0;
    let zeros = 0;
    for (let i = 0; i < data.length; i += 4) {
        let red = data[i] - (data[i] % 2);
        let green = data[i + 1] - (data[i + 1] % 2);
        let blue = data[i + 2] - (data[i + 2] % 2);
        for (let n = 0; n < 3; n++) {
            if (pixelElementIndex % 8 === 0) {
                if (state === 'finishing' && zeros === 8) {
                    if ((pixelElementIndex - 1) % 3 < 2) {
                        data[i] = red;
                        data[i + 1] = green;
                        data[i + 2] = blue;
                    }
                    ctx.putImageData(pixels, 0, 0);
                    return;
                }
                if (charIndex >= buildString.length) {
                    state = 'finishing';
                }
                else {
                    charValue = buildString[charIndex++];
                }
            }
            switch (pixelElementIndex % 3) {
                case 0:
                    if (state === 'encoding') {
                        red += charValue % 2;
                        charValue /= 2;
                    }
                    break;
                case 1:
                    if (state === 'encoding') {
                        green += charValue % 2;
                        charValue /= 2;
                    }
                    break;
                case 2:
                    if (state === 'encoding') {
                        blue += charValue % 2;
                        charValue /= 2;
                    }
                    data[i] = red;
                    data[i + 1] = green;
                    data[i + 2] = blue;
                    break;
            }
            pixelElementIndex++;
            if (state === 'finishing') {
                zeros++;
            }
        }
    }
    ctx.putImageData(pixels, 0, 0);
}