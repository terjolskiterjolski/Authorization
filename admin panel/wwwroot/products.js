const container = document.querySelector('#products');
const listStatus = document.querySelector('#list-status');
const search = document.querySelector('#search');
const reload = document.querySelector('#reload');
let products = [];
let pendingSaves = 0;

async function loadProducts() {
  reload.disabled = true;
  listStatus.textContent = 'Loading products…';
  try {
    const response = await fetch('/api/products', { cache: 'no-store' });
    const result = await response.json();
    if (!response.ok) throw new Error(result.message || 'Could not load products.');
    products = result;
    container.replaceChildren(...products.map(createCard));
    filterProducts();
  } catch (error) {
    listStatus.textContent = error instanceof TypeError ? 'Unable to reach the server. Start the app and select Reload products.' : error.message;
  } finally { reload.disabled = pendingSaves > 0; }
}

function createCard(product) {
  const card = document.querySelector('#product-template').content.firstElementChild.cloneNode(true);
  const form = card.querySelector('form');
  const status = card.querySelector('.form-status');
  const submit = form.querySelector('[type="submit"]');
  const reset = form.querySelector('[type="reset"]');
  let saving = false;
  const photoInput = card.querySelector('.photo-input');
  const photoStatus = card.querySelector('.photo-status');
  const photoButton = card.querySelector('.upload-photo');
  photoButton.addEventListener('click', () => photoInput.click());
  photoInput.addEventListener('change', async () => {
    const file = photoInput.files[0];
    if (!file) return;
    if (!['image/jpeg', 'image/png', 'image/webp'].includes(file.type) || file.size > 5 * 1024 * 1024) {
      photoStatus.textContent = 'Choose a JPG, PNG, or WebP photo smaller than 5 MB.';
      photoInput.value = '';
      return;
    }
    saving = true;
    pendingSaves++;
    reload.disabled = true;
    photoButton.disabled = photoInput.disabled = submit.disabled = reset.disabled = true;
    photoStatus.textContent = 'Uploading photo...';
    const data = new FormData();
    data.append('photo', file);
    try {
      const response = await fetch(`/api/products/${product.id}/photos`, { method: 'POST', body: data });
      const result = await response.json().catch(() => null);
      if (!response.ok) throw new Error(result?.message || 'Could not upload photo.');
      product.photoPath = result.photoPath;
      updatePhoto();
      photoStatus.textContent = 'Photo saved.';
    } catch (error) {
      photoStatus.textContent = error instanceof TypeError ? 'Unable to reach the server. Try again.' : error.message;
    } finally {
      saving = false;
      pendingSaves--;
      reload.disabled = pendingSaves > 0;
      photoButton.disabled = photoInput.disabled = submit.disabled = reset.disabled = false;
      photoInput.value = '';
    }
  });
  function updatePhoto() {
    const image = card.querySelector('.product-photo');
    if (!product.photoPath) { image.hidden = true; image.removeAttribute('src'); return; }
    image.onload = () => { image.hidden = false; };
    image.onerror = () => { image.hidden = true; photoStatus.textContent = 'Saved photo unavailable. Upload a new photo.'; };
    image.alt = product.name;
    image.hidden = false;
    image.src = new URL(product.photoPath.replace(/\\/g, '/'), window.location.origin).href;
  }
  updateCard();
  form.addEventListener('reset', event => {
    if (saving) { event.preventDefault(); return; }
    status.hidden = true;
  });
  form.addEventListener('submit', async event => {
    event.preventDefault();
    if (saving || !form.reportValidity()) return;
    const data = new FormData(form);
    const update = {
      id: Number(data.get('id')), name: data.get('name').trim(),
      price: Number(data.get('price')), stockQuantity: Number(data.get('stockQuantity')),
      description: data.get('description').trim(), sex: data.get('sex')
    };
    if (!update.name || !update.description) { showStatus('Enter a name and description.', 'error'); return; }
    saving = true;
    photoButton.disabled = photoInput.disabled = true;
    pendingSaves++;
    reload.disabled = true;
    submit.disabled = reset.disabled = true;
    submit.textContent = 'Saving…';
    status.hidden = true;
    try {
      const response = await fetch(`/api/products/${product.id}`, {
        method: 'PUT', headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(update)
      });
      const result = await response.json().catch(() => null);
      if (!response.ok) throw new Error(result?.message || 'Could not save changes.');
      Object.assign(product, update);
      updateCard();
      showStatus('Changes saved successfully.', 'success');
    } catch (error) {
      showStatus(error instanceof TypeError ? 'Unable to reach the server. Your changes have not been saved.' : error.message, 'error');
    } finally {
      saving = false;
      photoButton.disabled = photoInput.disabled = false;
      pendingSaves--;
      reload.disabled = pendingSaves > 0;
      submit.disabled = reset.disabled = false;
      submit.textContent = 'Save changes';
    }
  });
  function updateCard() {
    updatePhoto();
    card.dataset.search = `${product.id} ${product.name}`.toLowerCase();
    card.querySelector('.product-id').textContent = `PRODUCT #${product.id}`;
    card.querySelector('.product-name').textContent = product.name;
    card.querySelector('.product-description').textContent = product.description;
    card.querySelector('.product-price').textContent = `Price: ${Number(product.price).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
    card.querySelector('.product-sex').textContent =  `Sex: ${product.sex || 'Not set'}`;
    card.querySelector('.product-stock').textContent = `${product.stockQuantity} in stock`;
    for (const field of ['id', 'name', 'price', 'stockQuantity', 'description', 'sex']) {
      const input = form.elements.namedItem(field);
      input.defaultValue = input.value = product[field];
      if (input.tagName === 'SELECT') {
        for (const option of input.options) option.defaultSelected = option.value === product[field];
      }
    }
  }
  function showStatus(message, kind) { status.textContent = message; status.className = `form-status ${kind}`; status.hidden = false; }
  return card;
}

function filterProducts() {
  const query = search.value.trim().toLowerCase();
  let visible = 0;
  for (const card of container.children) {
    card.hidden = !card.dataset.search.includes(query);
    if (!card.hidden) visible++;
  }
  listStatus.textContent = products.length === 0 ? 'No products yet. Add your first product from the Add product page.' : visible === 0 ? 'No products match your search.' : `${visible} product${visible === 1 ? '' : 's'}`;
}
search.addEventListener('input', filterProducts);
reload.addEventListener('click', loadProducts);
loadProducts();
