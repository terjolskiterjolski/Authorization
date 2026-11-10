const form = document.querySelector('#product-form');
const status = document.querySelector('#status');
const button = document.querySelector('#save-button');
let saving = false;
form.addEventListener('reset', event => {
  if (saving) { event.preventDefault(); return; }
  status.hidden = true;
});
form.addEventListener('submit', async event => {
  event.preventDefault();
  if (saving || !form.reportValidity()) return;
  const data = new FormData(form);
  const name = data.get('productName').trim();
  if (!name) { showStatus('Enter a product name.', 'error'); document.querySelector('#productName').focus(); return; }
  const product = {
    productId: Number(data.get('productId')), productName: name,
    description: data.get('description').trim(), sex: data.get('sex'),
    price: Number(data.get('price')), stockQuantity: Number(data.get('stockQuantity'))
  };
  saving = true;
  button.disabled = true;
  form.querySelector('[type="reset"]').disabled = true;
  button.textContent = 'Saving…';
  status.hidden = true;
  try {
    const response = await fetch('/api/products', {
      method: 'POST', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(product)
    });
    const result = await response.json().catch(() => null);
    if (!response.ok) throw new Error(result?.message || 'Could not save the product. Check your inputs and try again.');
    saving = false;
    form.reset();
    showStatus(`${product.productName} was added successfully. You can add another product.`, 'success');
  } catch (error) {
    showStatus(error instanceof TypeError ? 'Unable to reach the server. Check that the app is running and try again.' : error.message, 'error');
  } finally {
    saving = false;
    button.disabled = false;
    form.querySelector('[type="reset"]').disabled = false;
    button.textContent = 'Add product ↗';
  }
});
function showStatus(message, kind) { status.textContent = message; status.className = kind; status.hidden = false; }
