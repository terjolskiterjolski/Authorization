const input = document.querySelector("#searchInput");
const productsElement = document.querySelector("#products");
const statusElement = document.querySelector("#status");
let requestNumber = 0;
let debounceTimer;

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value;
    return element.innerHTML;
}

function renderProducts(products) {
    productsElement.innerHTML = products.map(product => `
        <article class="product-card">
            <div class="card-top">
                <span class="price">$${Number(product.price).toFixed(2)}</span>
                <span class="stock ${product.stockQuantity === 0 ? "out" : ""}">
                    ${product.stockQuantity === 0 ? "Out of stock" : `${product.stockQuantity} in stock`}
                </span>
            </div>
            <h2>${escapeHtml(product.name)}</h2>
            <p>${escapeHtml(product.description)}</p>
        </article>
    `).join("");
}

async function search() {
    const currentRequest = ++requestNumber;
    statusElement.textContent = "Searching...";

    try {
        const response = await fetch(`/api/products/search?prefix=${encodeURIComponent(input.value.trim())}`);
        if (!response.ok) throw new Error("Search request failed");

        const products = await response.json();
        if (currentRequest !== requestNumber) return;

        renderProducts(products);
        statusElement.textContent = products.length === 0
            ? "No matching products found."
            : `${products.length} product${products.length === 1 ? "" : "s"} found`;
    } catch {
        if (currentRequest !== requestNumber) return;
        productsElement.innerHTML = "";
        statusElement.textContent = "Could not load products. Please try again.";
    }
}

input.addEventListener("input", () => {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(search, 250);
});

search();
