public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
    public List<OrderItem> OrderItems { get; set; } = new ();
}