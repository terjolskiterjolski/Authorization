const status = document.getElementById('purchase-status');
const table = document.getElementById('purchase-table');
const rows = document.getElementById('purchase-rows');
const button = document.getElementById('load-purchases');

async function loadProducts() {
    button.disabled = true;
    table.hidden = true;
    rows.replaceChildren();
    status.textContent = 'Loading bought products...';

    try {
        const response = await fetch('/api/purchased-products', { cache: 'no-store' });
        if (!response.ok) {
            if (response.status === 400) {
                throw new Error('Bought products could not be loaded: the server requires a customer ID for this endpoint.');
            }
            throw new Error('Could not load bought products. Please try again.');
        }
        const products = await response.json();
        if (!Array.isArray(products)) throw new Error('The server returned an invalid bought-products list.');

        for (const product of products) {
            const row = document.createElement('tr');
            for (const value of [product.productId, product.productName,
                Number(product.price).toFixed(2), product.stockQuantity,
                product.description, product.sex]) {
                const cell = document.createElement('td');
                cell.textContent = value ?? '';
                row.appendChild(cell);
            }
            rows.appendChild(row);
        }

        table.hidden = products.length === 0;
        status.textContent = products.length === 0
            ? 'No bought products found.'
            : `${products.length} bought product(s) found.`;
    } catch (error) {
        status.textContent = error.message;
    } finally {
        button.disabled = false;
    }
}

button.addEventListener('click', loadProducts);
loadProducts();
