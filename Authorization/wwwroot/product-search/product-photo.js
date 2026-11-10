function displayProductPhoto(image, product) {
    // Hidden images must load eagerly so onload can make them visible.
    image.loading = 'eager';
    const value = [product.photoPath, product.photoUrl, product.photo]
        .find(item => typeof item === "string" && item.trim());
    image.hidden = true;
    if (!value) return;

    let source = value.trim();
    if (/^(iVBORw0KGgo|\/9j\/|R0lGOD|UklGR)/.test(source)) {
        const mime = source.startsWith('/9j/') ? 'image/jpeg'
            : source.startsWith('R0lGOD') ? 'image/gif'
            : source.startsWith('UklGR') ? 'image/webp' : 'image/png';
        source = `data:${mime};base64,${source.replace(/\s/g, '')}`;
    } else if (!/^(https?:\/\/|data:image\/)/i.test(source)) {
        source = source.replace(/\\/g, '/');
        const webRoot = source.toLowerCase().lastIndexOf('wwwroot/');
        if (webRoot >= 0) source = source.slice(webRoot + 'wwwroot/'.length);
        source = '/' + source.replace(/^\/+/, '');
    }

    image.alt = product.name || 'Product photo';
    image.onload = () => {
        image.hidden = false;
    };
    image.onerror = () => {
        image.hidden = true;
    };
    image.src = source;
}
