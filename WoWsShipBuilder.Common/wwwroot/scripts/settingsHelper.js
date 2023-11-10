export function getAppSettings() {
    try {
        return JSON.parse(window.localStorage['settings']);
    } catch (e) {
        console.log(e);
        return null;
    }
}

export function setAppSettings(settings) {
    window.localStorage['settings'] = JSON.stringify(settings);
}
