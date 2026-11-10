-- Refresh perfume metadata while preserving stock, ratings and product references.
INSERT INTO products (product_id, name, description, price, stock_quantity, created_at_utc, sex)
VALUES
    (1, 'Rose Bloom', 'An eau de parfum with rose, peony and soft musk.', 89.99::numeric, 12, NOW(), 'Women'),
    (2, 'Jasmine Velvet', 'An eau de parfum with jasmine, orange blossom and vanilla.', 99.99::numeric, 20, NOW(), 'Women'),
    (3, 'Amber Petal', 'An eau de parfum with magnolia, amber and sandalwood.', 109.99::numeric, 15, NOW(), 'Women'),
    (4, 'Pear Blossom', 'An eau de parfum with pear, freesia and white musk.', 79.99::numeric, 8, NOW(), 'Women'),
    (5, 'Cedar Noir', 'An eau de parfum with cedar, bergamot and vetiver.', 99.99::numeric, 10, NOW(), 'Men'),
    (6, 'Ocean Drift', 'An eau de parfum with marine notes, citrus and sage.', 84.99::numeric, 6, NOW(), 'Men'),
    (7, 'Leather Ember', 'An eau de parfum with leather, black pepper and amber.', 119.99::numeric, 18, NOW(), 'Men'),
    (8, 'Vetiver Woods', 'An eau de parfum with vetiver, oakmoss and grapefruit.', 94.99::numeric, 25, NOW(), 'Men'),
    (9, 'Citrus Mist', 'An eau de parfum with lemon, neroli and clean musk.', 74.99::numeric, 30, NOW(), 'Unisex'),
    (10, 'Santal Dusk', 'An eau de parfum with sandalwood, cardamom and iris.', 124.99::numeric, 22, NOW(), 'Unisex'),
    (11, 'Green Tea', 'An eau de parfum with green tea, mint and bergamot.', 69.99::numeric, 14, NOW(), 'Unisex'),
    (12, 'Amber Horizon', 'An eau de parfum with amber, vanilla and smoky woods.', 114.99::numeric, 16, NOW(), 'Unisex')
ON CONFLICT (product_id) DO UPDATE
SET name = EXCLUDED.name, description = EXCLUDED.description,
    price = EXCLUDED.price, sex = EXCLUDED.sex;
