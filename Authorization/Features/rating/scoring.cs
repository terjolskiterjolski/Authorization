using Authorization.Features.Products;

namespace products
{
    class Score
    {
        public int rating;
        public void rate(Product product)
        {
            product.Rating = rating;
        }
    }

    class Ordering
    {
        public Product[] sequenceOfProducts(Product[] products)
        {
            Array.Sort(products, static (x, y) => y.Rating.CompareTo(x.Rating));
            return products;
        }
    }


}