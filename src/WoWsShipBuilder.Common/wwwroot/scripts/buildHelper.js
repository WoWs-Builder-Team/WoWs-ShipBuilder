export function loadContainers() {
    return window.localStorage['builds'];
}

export function saveContainers(buildString) {
    window.localStorage['builds'] = buildString;
}

export function deleteContainersFile() {
    window.localStorage.removeItem('builds');
}