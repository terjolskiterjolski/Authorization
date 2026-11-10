using Authorization.Features.Products;
using Authorization.Features.Authentication.Models;
namespace Authorization.Features.ShoppingCart;
public class Cart
{
    private readonly Customer customer;

    public int Id => customer.Id;
    public List<CartItem> Items { get; set; } = new List<CartItem>();
    public decimal TotalPrice => Items.Sum(item => item.Product.Price * item.Quantity);

    public Cart(Customer customer, List<CartItem> items)
    {
        this.customer = customer;
        Items = items;
    }

    public IEnumerable<CartItem> Buy()
    {
        IEnumerable<CartItem> itemsToBuy = Items.Where(item => item.Product.StockQuantity >= item.Quantity);
        Items = null!;
        return itemsToBuy;
    }
}

public class CartItem
{
    public int Id { get; set; }
    public Product Product { get; set; } = new Product();
    public int Quantity { get; set; }
}
