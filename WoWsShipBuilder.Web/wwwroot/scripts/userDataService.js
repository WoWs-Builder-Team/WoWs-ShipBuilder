export function loadData(fileName) {
    return window.localStorage[fileName];
}

export function saveData(fileName, data) {
    window.localStorage[fileName] = data;
}