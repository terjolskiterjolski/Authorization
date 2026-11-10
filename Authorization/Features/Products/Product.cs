using Authorization.Features.Authentication.Models;
using Authorization.Features.ShoppingCart;
using Feedback;


namespace Authorization.Features.Products;

public sealed class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Sex { get; set; } = "Unisex";
    public string Description { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public int Rating { get; set; }
    public static int Sold {get; set;}
    public int InquiryFrequency {get; set;}
    public DateTime CreatedAtUtc { get; set; }
    Boolean isRated = false;
    public string? PhotoPath { get; set; }
    public void UpdateStockQuantity(int quantity)
    {
        StockQuantity = quantity;
    }
    public void addToCart (Customer customer, int quantity)
    {
        Sold += quantity;
        customer.Cart.Items.Add(new CartItem { Product = this, Quantity = quantity });
        
    }

    internal Product With(int inquiryFrequency)
    {
        return new Product
        {
            Id = Id,
            Name = Name,
            Sex = Sex,
            Description = Description,
            Price = Price,
            StockQuantity = StockQuantity,
            InquiryFrequency = inquiryFrequency,
            CreatedAtUtc = CreatedAtUtc
        };
    }

    public CustomerProductRating rate(int rating, int CustomerId)
    {
        if (rating < 1 || rating > 5)
        {
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
        }
        isRated = true;
        Rating = rating; 
        return new ( CustomerId: CustomerId, ProductId: Id, Rating: rating);   

    }

}
