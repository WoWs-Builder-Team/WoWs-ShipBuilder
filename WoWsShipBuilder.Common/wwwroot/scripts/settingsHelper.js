export function getAppSettings() {
    const settings = window.localStorage['settings'];
    if (settings) {
        return JSON.parse(window.localStorage['settings']);
    }

    return null;
}

export function setAppSettings(settings) {
    window.localStorage['settings'] = JSON.stringify(settings);
}
