export function PreventMiddleClickDefault(id) {
    const element = document.getElementById(id);
    if (element != null) {
        element.removeEventListener("mousedown", handleMiddleClick);
        element.addEventListener("mousedown", handleMiddleClick);   
    }
}

function handleMiddleClick(event) {
    if (event.button === 1) {
        event.preventDefault();
        return false;
    }
}

export function PreventMiddleClickDefaultBatched(ids) {
    ids.forEach(id => PreventMiddleClickDefault(id));
}