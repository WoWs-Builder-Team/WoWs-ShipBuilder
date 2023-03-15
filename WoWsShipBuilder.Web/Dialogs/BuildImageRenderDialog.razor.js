export async function downloadBuildImage(id, buildName){
    let img = "";
    await html2canvas(document.querySelector("#" + id), {
        backgroundColor: "#282828",
        useCORS: true,
        allowTaint: true,
    }).then(canvas => img = canvas.toDataURL("image/png"));
    let d = document.createElement("a");
    d.href = img;
    d.download = buildName + ".png";
    d.click();
}