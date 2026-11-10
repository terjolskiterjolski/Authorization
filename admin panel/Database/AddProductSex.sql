-- Run once on an existing database before using the updated forms.
-- Existing products start as Unisex; update their selection as needed.
ALTER TABLE public.products
    ADD COLUMN IF NOT EXISTS sex TEXT NOT NULL DEFAULT 'Unisex'
    CHECK (sex IN ('Men', 'Women', 'Unisex'));
