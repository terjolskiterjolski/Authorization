using System;
using ProductsEditing;

namespace ProductsEditing
{
    public class Product
    {
        public int Id { get; set; }
        public string? PhotoPath { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Sex { get; set; } = string.Empty;
    }

    public class Change
    {
        private readonly Product product;

        public Change(Product product)
        {
            ArgumentNullException.ThrowIfNull(product);
            this.product = product;
        }
        public void ChangeId(int newId)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (newId <= 0)
                throw new ArgumentException("New ID must be a positive integer.", nameof(newId));

            product.Id = newId;
        }

        public void ChangeName(Product product, string newName)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrWhiteSpace(newName) || newName.Trim().Length > 100)
                throw new ArgumentException("New name cannot be null or whitespace.", nameof(newName));

            product.Name = newName.Trim();
        }

        public void ChangePrice(Product product, decimal newPrice)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (newPrice < 0 || newPrice > 99999999.99m || decimal.Round(newPrice, 2) != newPrice)
                throw new ArgumentException("New price cannot be negative.", nameof(newPrice));

            product.Price = newPrice;
        }

        public void ChangeStockQuantity(Product product, int newStockQuantity)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (newStockQuantity < 0)
                throw new ArgumentException("New stock quantity cannot be negative.", nameof(newStockQuantity));

            product.StockQuantity = newStockQuantity;
        }

        public void ChangeDescription(Product product, string newDescription)
        {
            ArgumentNullException.ThrowIfNull(product);

            if (string.IsNullOrWhiteSpace(newDescription))
                throw new ArgumentException("New description cannot be null or whitespace.", nameof(newDescription));

            product.Description = newDescription.Trim();
        }
    }
}
