export function getAppSettings() {
    return window.localStorage['settings'];
}

export function setAppSettings(settings) {
    window.localStorage['settings'] = settings;
}