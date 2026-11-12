
const DB_NAME = "TranslationOfficeBrowserStorage";
const STORE_NAME = "GuestOrderCart";
const CART_KEY = "ActiveGuestCart";
const DB_VERSION = 1;

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
            reject(request.error);
        };
    });
}

export async function loadCart() {
    const db = await openDatabase();

    try {
        return await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readonly");
            const store = transaction.objectStore(STORE_NAME);
            const request = store.get(CART_KEY);

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

export async function saveCart(json) {
    const db = await openDatabase();

    try {
        await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readwrite");

            transaction.objectStore(STORE_NAME).put(json, CART_KEY);

            transaction.oncomplete = function () {
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

export async function clearCart() {
    const db = await openDatabase();

    try {
        await new Promise((resolve, reject) => {
            const transaction = db.transaction(STORE_NAME, "readwrite");

            transaction.objectStore(STORE_NAME).delete(CART_KEY);

            transaction.oncomplete = function () {
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
