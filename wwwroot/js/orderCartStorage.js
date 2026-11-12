
const DB_NAME = "TranslationOfficeBrowserStorage";
const STORE_NAME = "GuestOrderCart";
const CART_KEY = "ActiveGuestCart";
const DB_VERSION = 1;

function resolveCartKey(storageKey = "guest") {
    return storageKey === "guest"
        ? CART_KEY
        : `${CART_KEY}:${storageKey}`;
}

function openDatabase() {
    return new Promise((resolve, reject) => {
        const request = indexedDB.open(DB_NAME, DB_VERSION);

        request.onupgradeneeded = function (event) {
            const db = event.target.result;

            if (!db.objectStoreNames.contains(STORE_NAME)) {
                db.createObjectStore(STORE_NAME);
            }
        };

        request.onsuccess = function () {
            resolve(request.result);
        };

        request.onerror = function () {
            reject(request.error || new Error("IndexedDB open failed."));
        };

        request.onblocked = function () {
            reject(new Error("IndexedDB is blocked by another open connection."));
        };
    });
}

export async function loadCart(storageKey = "guest") {
    const db = await openDatabase();

    try {
        return await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readonly");
            const store = transaction.objectStore(STORE_NAME);
            const request = store.get(resolveCartKey(storageKey));

            request.onsuccess = function () {
                resolve(request.result ?? null);
            };

            request.onerror = function () {
                reject(request.error);
            };
        });
    }
    finally {
        db.close();
    }
}

export async function loadCartCount(storageKey = "guest") {
    const json = await loadCart(storageKey);
    if (!json) return 0;

    try {
        const state = JSON.parse(json);
        return Array.isArray(state?.Items) ? state.Items.length : 0;
    } catch {
        return 0;
    }
}

export async function saveCart(json, storageKey = "guest") {
    const db = await openDatabase();

    try {
        await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readwrite");

            transaction.objectStore(STORE_NAME).put(json, resolveCartKey(storageKey));

            transaction.oncomplete = function () {
                let itemCount = 0;
                try {
                    const state = JSON.parse(json);
                    itemCount = Array.isArray(state?.Items) ? state.Items.length : 0;
                } catch {
                    itemCount = 0;
                }
                window.dispatchEvent(new CustomEvent("orderCartChanged", { detail: itemCount }));
                resolve();
            };

            transaction.onerror = function () {
                reject(transaction.error);
            };

            transaction.onabort = function () {
                reject(transaction.error);
            };
        });
    }
    finally {
        db.close();
    }
}

export async function clearCart(storageKey = "guest") {
    const db = await openDatabase();

    try {
        await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readwrite");

            transaction.objectStore(STORE_NAME).delete(resolveCartKey(storageKey));

            transaction.oncomplete = function () {
                window.dispatchEvent(new CustomEvent("orderCartChanged", { detail: 0 }));
                resolve();
            };

            transaction.onerror = function () {
                reject(transaction.error);
            };

            transaction.onabort = function () {
                reject(transaction.error);
            };
        });
    }
    finally {
        db.close();
    }
}


let cartChangeHandler = null;

export function subscribeCartChanges(dotNetReference) {
    unsubscribeCartChanges();

    cartChangeHandler = function (event) {
        dotNetReference.invokeMethodAsync("OnCartChanged", Number(event.detail) || 0)
            .catch(() => { });
    };

    window.addEventListener("orderCartChanged", cartChangeHandler);
}

export function unsubscribeCartChanges() {
    if (cartChangeHandler) {
        window.removeEventListener("orderCartChanged", cartChangeHandler);
        cartChangeHandler = null;
    }
}
