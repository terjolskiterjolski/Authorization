
using Authorization.Features.ShoppingCart;
using Authorization.Features.Products;
using Feedback;
namespace Authorization.Features.Authentication.Models;

public sealed class Customer
{
    private static int nextId = 0;
    public int Id { get; set; } = System.Threading.Interlocked.Increment(ref nextId);
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public Cart Cart { get; }public List<CustomerProductRating> RatedProducts { get; set; } = new();
    public Customer()
    {
        Cart = new Cart(this, new List<CartItem>());
    }

    public Customer With(string name, string email, string phone, string address)
    {
        return new Customer
        {
            Id = Id,
            Name = name,
            Email = email,
            Phone = phone,
            Address = address
        };
    }
    public void rateProduct(Product product, int rating)
    {
        CustomerProductRating ratedProduct = product.rate(rating, Id);
        RatedProducts.Add(ratedProduct);
    }

}
