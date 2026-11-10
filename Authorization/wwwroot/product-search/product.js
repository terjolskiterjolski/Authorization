async function loadProduct() {
    const status = document.querySelector("#productStatus");
    const id = Number(new URLSearchParams(window.location.search).get("id"));
    if (!Number.isSafeInteger(id) || id <= 0) {
        status.textContent = "Product not found. Return to the collection to select a perfume.";
        return;
    }

    try {
        const response = await fetch("/api/products");
        if (!response.ok) throw new Error("Could not load products");
        const products = await response.json();
        const product = products.find(item => Number(item.id) === id);
        if (!product) {
            status.textContent = "Product not found. Return to the collection to select a perfume.";
            return;
        }

        document.title = `${product.name} | Perfume Collection`;
        document.querySelector("#productName").textContent = product.name;
        const image = document.querySelector("#productPhoto");
        displayProductPhoto(image, product);
        document.querySelector("#productDescription").textContent = product.description;
        document.querySelector("#productPrice").textContent = `$${Number(product.price).toFixed(2)}`;
        document.querySelector("#productCategory").textContent = product.sex;
        document.querySelector("#productStock").textContent = product.stockQuantity > 0
            ? `${product.stockQuantity} in stock` : "Out of stock";
        document.querySelector("#productRating").textContent = product.rating > 0
            ? `${product.rating} / 5` : "Not yet rated";
        let purchaseButton = document.querySelector("#purchaseProduct");
        if (!purchaseButton) {
            purchaseButton = document.createElement("button");
            purchaseButton.id = "purchaseProduct";
            purchaseButton.type = "button";
            purchaseButton.className = "add-to-cart";
            const purchaseStatus = document.createElement("p");
            purchaseStatus.dataset.purchaseStatus = String(id);
            purchaseStatus.setAttribute("role", "status");
            document.querySelector("#productDetail").append(purchaseButton, purchaseStatus);
        }
        purchaseButton.dataset.purchaseId = String(id);
        purchaseButton.textContent = "Purchase";
        purchaseButton.disabled = product.stockQuantity <= 0;
        status.hidden = true;
        document.querySelector("#productDetail").hidden = false;
        document.querySelector("#feedbackSection").hidden = false;
    } catch {
        status.textContent = "Could not load this perfume. Please reload to try again.";
    }
}

document.querySelector("#feedbackForm").addEventListener("submit", async event => {
    event.preventDefault();
    const button = document.querySelector("#saveFeedback");
    if (button.disabled) return;
    const status = document.querySelector("#feedbackStatus");
    const message = document.querySelector("#feedbackMessage").value.trim();
    if (!message) {
        status.textContent = "Please enter your feedback.";
        return;
    }

    button.disabled = true;
    button.textContent = "Saving...";
    status.textContent = "";
    try {
        const id = Number(new URLSearchParams(window.location.search).get("id"));
        const response = await fetch(`/api/products/${id}/feedback`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ message, rating: Number(document.querySelector("#feedbackRating").value) })
        });
        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            status.textContent = error.message || "Could not save your feedback. Please try again.";
            return;
        }
        status.textContent = "Your feedback has been saved.";
        document.querySelector("#feedbackForm").reset();
    } catch {
        status.textContent = "Could not connect to save your feedback. Please try again.";
    } finally {
        button.disabled = false;
        button.textContent = "Save feedback";
    }
});

loadProduct();

document.addEventListener("product-purchased", async () => {
    await loadProduct();
    const status = document.querySelector("[data-purchase-status]");
    if (status) status.textContent = "Purchase saved successfully.";
});
