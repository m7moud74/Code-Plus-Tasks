public class Order
{
    public int OrderID { get; set; }

    public List<Product> Products { get; set; } = new();
    
}