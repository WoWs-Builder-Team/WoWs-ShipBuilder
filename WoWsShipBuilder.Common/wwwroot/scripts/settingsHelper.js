export function getAppSettings() {
    return JSON.parse(window.localStorage['settings']);
}

export function setAppSettings(settings) {
    window.localStorage['settings'] = JSON.stringify(settings);
}
