const input = document.querySelector("#searchInput");
const searchResultsElement = document.querySelector("#searchResults");
const searchProductsElement = document.querySelector("#searchProducts");
const catalogStatusElement = document.querySelector("#catalogStatus");
const productsElement = document.querySelector("#products");
const statusElement = document.querySelector("#status");
let selectedSex = "All";
let catalogProducts = [];
let requestNumber = 0;
function filterProducts(products) {
    return products.filter(product => selectedSex === "All" || product.sex === selectedSex);
}
function renderCatalog() {
    const products = filterProducts(catalogProducts);
    renderProducts(products, productsElement);
    document.querySelector("#categoryTitle").textContent = selectedSex === "All" ? "All perfumes" : selectedSex + " perfumes";
    catalogStatusElement.textContent = products.length ? "Highest rated first" : "No perfumes available in this category.";
}
document.querySelector("#sexFilters").addEventListener("click", event => {
    const button = event.target.closest("button[data-sex]");
    if (!button) return;
    selectedSex = button.dataset.sex;
    document.querySelectorAll("#sexFilters button").forEach(item => item.setAttribute("aria-pressed", String(item === button)));
    renderCatalog();
    clearTimeout(debounceTimer);
    search();
});
let debounceTimer;
const availableProducts = new Map();
const cart = new Map();
const cartDialog = document.querySelector("#cartDialog");
const cartStatus = document.querySelector("#cartStatus");

function renderCart() {
    document.querySelector("#cartCount").textContent = [...cart.values()].reduce((sum, item) => sum + item.quantity, 0);
    document.querySelector("#cartItems").innerHTML = cart.size ? [...cart.values()].map(item => `
        <article class="cart-item">
            <div><h3>${escapeHtml(item.product.name)}</h3><p>${item.quantity} × $${Number(item.product.price).toFixed(2)}</p></div>
            <button type="button" class="cart-button" data-remove-id="${Number(item.product.id)}">Remove<span class="sr-only"> ${escapeHtml(item.product.name)}</span></button>
        </article>
    `).join("") : "<p>Your cart is empty.</p>";
    document.querySelector("#cartTotal").textContent = `$${([...cart.values()].reduce((sum, item) => sum + Math.round(Number(item.product.price) * 100) * item.quantity, 0) / 100).toFixed(2)}`;
}

function addToCart(event) {
    const button = event.target.closest("button[data-add-id]");
    if (!button) return;
    const id = Number(button.dataset.addId);
    const product = availableProducts.get(id);
    if (!product) return;
    const quantity = (cart.get(id)?.quantity || 0) + 1;
    if (quantity > product.stockQuantity) {
        cartStatus.textContent = `All available stock for ${product.name} is already in your cart.`;
        return;
    }
    cart.set(id, { product, quantity });
    renderCart();
    cartStatus.textContent = `${product.name} added to cart.`;
}

document.querySelector("#cartButton").addEventListener("click", () => {
    renderCart();
    cartDialog.showModal();
});
document.querySelector("#cartItems").addEventListener("click", event => {
    const button = event.target.closest("button[data-remove-id]");
    if (!button) return;
    cart.delete(Number(button.dataset.removeId));
    renderCart();
    (document.querySelector("#cartItems button") || cartDialog.querySelector("button")).focus();
});
productsElement.addEventListener("click", addToCart);
searchProductsElement.addEventListener("click", addToCart);

function escapeHtml(value) {
    const element = document.createElement("div");
    element.textContent = value;
    return element.innerHTML;
}

function renderProducts(products, target) {
    products.forEach(product => availableProducts.set(Number(product.id), product));
    target.innerHTML = products.map(product => `
        <article class="product-card">
            <div class="card-top">
                <span class="price">$${Number(product.price).toFixed(2)}</span>
                <span class="stock ${product.stockQuantity === 0 ? "out" : ""}">
                    ${product.stockQuantity === 0 ? "Out of stock" : `${product.stockQuantity} in stock`}
                </span>
            </div>
            <span class="sex-badge">${escapeHtml(product.sex)} &middot; Eau de parfum</span>
            <h2><a class="product-link" href="product.html?id=${Number(product.id)}">${escapeHtml(product.name)}</a></h2>
            <p>${escapeHtml(product.description)}</p>
            <div class="rating-control" data-product-id="${Number(product.id)}" role="group" aria-label="Product rating">
                <div class="stars">
                    ${[1, 2, 3, 4, 5].map(star => `<button type="button" class="star ${star <= product.rating ? "selected" : ""}" data-rating="${star}" aria-label="Rate ${star} out of 5 stars" aria-pressed="${star === product.rating}"><span aria-hidden="true">&#9733;</span></button>`).join("")}
                </div>
                <span class="rating-message" role="status">${product.rating > 0 ? `${Number(product.rating)} / 5` : "Not yet rated"}</span>
            </div>
            <button type="button" class="add-to-cart" data-add-id="${Number(product.id)}" ${product.stockQuantity > 0 ? "" : "disabled"}>${product.stockQuantity > 0 ? "Add to Cart" : "Out of stock"}</button>
            <button type="button" class="add-to-cart" data-purchase-id="${Number(product.id)}" ${product.stockQuantity > 0 ? "" : "disabled"}>Purchase</button>
            <p data-purchase-status="${Number(product.id)}" role="status"></p>
        </article>
    `).join("");
    products.forEach((product, index) => {
        const image = document.createElement("img");
        image.className = "product-photo";
        image.alt = product.name;
        target.children[index].prepend(image);
        displayProductPhoto(image, product);
    });
}

async function search() {
    const prefix = input.value.trim();

    if (!prefix) {
        ++requestNumber;
        searchResultsElement.hidden = true;
        searchProductsElement.innerHTML = "";
        statusElement.textContent = "";
        return;
    }

    const currentRequest = ++requestNumber;
    searchResultsElement.hidden = false;
    statusElement.textContent = "Searching...";

    try {
        const response = await fetch(`/api/products/search?prefix=${encodeURIComponent(prefix)}`);
        if (!response.ok) throw new Error("Search request failed");

        const products = filterProducts(await response.json());
        if (currentRequest !== requestNumber) return;

        renderProducts(products, searchProductsElement);
        statusElement.textContent = products.length === 0
            ? "No matching perfumes in this category."
            : `${products.length} perfume${products.length === 1 ? "" : "s"} found`;
    } catch {
        if (currentRequest !== requestNumber) return;
        searchProductsElement.innerHTML = "";
        statusElement.textContent = "Could not load perfumes. Please try again.";
    }
}

input.addEventListener("input", () => {
    ++requestNumber;
    clearTimeout(debounceTimer);
    if (!input.value.trim()) {
        search();
        return;
    }
    debounceTimer = setTimeout(search, 250);
});

async function loadCatalog() {
    try {
        const response = await fetch("/api/products");
        if (!response.ok) throw new Error("Catalog request failed");
        catalogProducts = await response.json();
        renderCatalog();
    } catch {
        catalogStatusElement.textContent = "Could not load the catalog. Please reload to try again.";
    }
}

loadCatalog();

document.addEventListener("product-purchased", async event => {
    const id = event.detail.productId;
    cart.delete(id);
    renderCart();
    await loadCatalog();
    await search();
    cartStatus.textContent = "Purchase saved successfully.";
});

const pendingRatings = new Set();

async function saveRating(event) {
    const button = event.target.closest("button[data-rating]");
    if (!button) return;
    const control = button.closest(".rating-control");
    const productId = Number(control.dataset.productId);
    const rating = Number(button.dataset.rating);
    if (pendingRatings.has(productId)) return;
    pendingRatings.add(productId);
    const controls = () => document.querySelectorAll(`.rating-control[data-product-id="${productId}"]`);
    controls().forEach(item => {
        item.querySelector(".rating-message").textContent = "Saving...";
        item.querySelectorAll("button").forEach(star => { star.disabled = true; });
    });
    try {
        const response = await fetch(`/api/products/${productId}/rating`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ rating })
        });
        if (response.status === 401) {
            controls().forEach(item => {
                item.querySelector(".rating-message").textContent = "Please log in with your email and password to rate this product.";
            });
            return;
        }
        if (!response.ok) throw new Error("Rating request failed");
        controls().forEach(item => {
            item.querySelectorAll("button").forEach(star => {
                star.classList.toggle("selected", Number(star.dataset.rating) <= rating);
                star.setAttribute("aria-pressed", String(Number(star.dataset.rating) === rating));
            });
            item.querySelector(".rating-message").textContent = `${rating} / 5 — Saved`;
        });
    } catch {
        controls().forEach(item => {
            item.querySelector(".rating-message").textContent = "Could not save. Please try again.";
        });
    } finally {
        pendingRatings.delete(productId);
        controls().forEach(item => item.querySelectorAll("button").forEach(star => { star.disabled = false; }));
    }
}

productsElement.addEventListener("click", saveRating);
searchProductsElement.addEventListener("click", saveRating);
