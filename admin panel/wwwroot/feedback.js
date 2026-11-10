const search = document.querySelector('#feedback-search');
const refresh = document.querySelector('#refresh-feedback');
const sections = {
  ratings: { data: [], loaded: false },
  comments: { data: [], loaded: false }
};

function render(kind) {
  const section = sections[kind];
  if (!section.loaded) return;
  const list = document.querySelector(`#${kind}`);
  const status = document.querySelector(`#${kind}-status`);
  const query = search.value.trim().toLowerCase();
  const rows = section.data.filter(row =>
    `${row.customerId} ${row.productId} ${row.productName} ${row.feedback ?? ''}`.toLowerCase().includes(query));
  list.replaceChildren();
  for (const row of rows) {
    const item = document.createElement('li');
    const id = document.createElement('span');
    id.className = 'eyebrow';
    id.textContent = `PRODUCT #${row.productId} / CUSTOMER #${row.customerId}`;
    const name = document.createElement('h3');
    name.textContent = row.productName;
    const detail = document.createElement(kind === 'ratings' ? 'span' : 'blockquote');
    if (kind === 'ratings') {
      detail.className = 'rating-value';
      detail.textContent = Number(row.averageRating).toLocaleString(undefined, { maximumFractionDigits: 2 });
      detail.setAttribute('aria-label', `Average rating: ${detail.textContent}`);
    } else {
      detail.textContent = row.feedback;
    }
    item.append(id, name, detail);
    list.append(item);
  }
  status.className = '';
  status.textContent = rows.length
    ? `${rows.length} ${kind === 'ratings' ? 'customer rating' : 'comment'}${rows.length === 1 ? '' : 's'}${query ? ' matching your search' : ''}`
    : section.data.length ? 'No results match your search.' : `No ${kind} yet.`;
}

async function load(kind) {
  const section = sections[kind];
  const status = document.querySelector(`#${kind}-status`);
  section.loaded = false;
  document.querySelector(`#${kind}`).replaceChildren();
  status.className = '';
  status.textContent = `Loading ${kind}…`;
  try {
    const response = await fetch(`/api/feedback/${kind}`, { cache: 'no-store', signal: AbortSignal.timeout(15000) });
    if (!response.ok) throw new Error(`Could not load ${kind}. Select Refresh feedback to try again.`);
    const data = await response.json();
    if (!Array.isArray(data) || data.some(row =>
      !row || !Number.isInteger(row.customerId) || !Number.isInteger(row.productId) || typeof row.productName !== 'string' ||
      (kind === 'ratings' ? typeof row.averageRating !== 'number' : typeof row.feedback !== 'string'))) {
      throw new Error(`The server returned an unexpected ${kind} response. Select Refresh feedback to try again.`);
    }
    section.data = data;
    section.loaded = true;
    render(kind);
  } catch (error) {
    status.className = 'error';
    status.textContent = error.name === 'TimeoutError'
      ? `Loading ${kind} took too long. Select Refresh feedback to try again.`
      : error instanceof TypeError
        ? `Cannot connect to load ${kind}. Check your connection and select Refresh feedback.`
        : error.message;
  }
}

async function reload() {
  refresh.disabled = true;
  try {
    await Promise.all([load('ratings'), load('comments')]);
  } finally {
    refresh.disabled = false;
  }
}
search.addEventListener('input', () => { render('ratings'); render('comments'); });
refresh.addEventListener('click', reload);
reload();
