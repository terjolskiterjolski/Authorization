const pendingPurchases = new Set();

document.addEventListener("click", async event => {
    const button = event.target.closest("button[data-purchase-id]");
    if (!button || button.disabled) return;
    const productId = Number(button.dataset.purchaseId);
    if (pendingPurchases.has(productId)) return;
    pendingPurchases.add(productId);
    const buttons = () => document.querySelectorAll(`[data-purchase-id="${productId}"]`);
    const showStatus = message => document.querySelectorAll(`[data-purchase-status="${productId}"]`)
        .forEach(element => { element.textContent = message; });
    buttons().forEach(item => {
        item.disabled = true;
        item.textContent = "Purchasing...";
    });
    showStatus("");
    let succeeded = false;
    try {
        const response = await fetch(`/api/products/${productId}/purchase`, {
            method: "POST",
            credentials: "same-origin",
            headers: { "Content-Type": "application/json" },
            body: "{}"
        });
        const result = await response.json().catch(() => ({}));
        if (!response.ok) {
            showStatus(result.message || "Could not purchase this product. Please try again.");
            return;
        }
        succeeded = true;
        showStatus("Purchase saved successfully.");
        document.dispatchEvent(new CustomEvent("product-purchased", { detail: { productId } }));
    } catch {
        showStatus("Could not confirm your purchase. Check before trying again.");
    } finally {
        pendingPurchases.delete(productId);
        buttons().forEach(item => {
            item.disabled = succeeded;
            item.textContent = succeeded ? "Purchased" : "Purchase";
        });
    }
});
