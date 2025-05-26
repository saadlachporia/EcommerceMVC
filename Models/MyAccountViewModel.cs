public class MyAccountViewModel
{
    public string UserName { get; set; }
    public string Email { get; set; }

    public List<OrderSummaryViewModel> Orders { get; set; } = new();
}

public class OrderSummaryViewModel
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal Total { get; set; }
    public string Status { get; set; }
}
