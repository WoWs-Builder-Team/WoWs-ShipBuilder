export async function findUploadAndDecode() {
    let fileInput = document.getElementsByClassName("mud-file-upload")[0].querySelector("input");
    let file = fileInput.files[0];
    let canvas = document.createElement("canvas");
    let context = canvas.getContext("2d");
    let bitmap = await createImageBitmap(file);
    canvas.width = bitmap.width;
    canvas.height = bitmap.height;
    context.drawImage(bitmap, 0, 0);
    return await extractBuildFromImage(context.getImageData(0, 0, bitmap.width, bitmap.height));
}

/**
 * @param {ImageData} image The ImageData containing the build image.
 */
export async function extractBuildFromImage(image) {
    let colorUnitIndex = 0;
    let charValue = 0;
    let extractedText = '';
    let data = image.data;
    for (let i = 0; i < data.length; i += 4) {
        let red = data[i];
        let green = data[i + 1];
        let blue = data[i + 2];

        for (let n = 0; n < 3; n++) {
            const tmp = colorUnitIndex % 3;
            if (tmp === 0) {
                charValue = charValue * 2 + red % 2;
            } else if (tmp === 1) {
                charValue = charValue * 2 + green % 2;
            } else if (tmp === 2) {
                charValue = charValue * 2 + blue % 2;
            }

            charValue = Math.floor(charValue);
            colorUnitIndex++;

            if (colorUnitIndex % 8 === 0) {
                charValue = Math.floor(reverseBits(charValue));
                if (charValue === 0) {
                    return extractedText;
                }
                const str = String.fromCharCode(charValue);
                extractedText += str;
            }
        }
    }

    return extractedText;
}

function reverseBits(charValue) {
    let result = 0;
    for (let i = 0; i < 8; i++) {
        result = (result * 2) + Math.floor(charValue % 2);
        charValue = Math.floor(charValue / 2);
    }
    return result;
}