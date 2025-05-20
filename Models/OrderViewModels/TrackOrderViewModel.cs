namespace EcommerceMVC.Models.OrderViewModels
{
    public class TrackOrderViewModel
    {
        public int OrderId { get; set; }

        public string Status { get; set; }

        public DateTime OrderDate { get; set; }

        public string FullName { get; set; }

        public string Address { get; set; }

        public string? TrackingNumber { get; set; }

        public List<OrderItemViewModel> Items { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
