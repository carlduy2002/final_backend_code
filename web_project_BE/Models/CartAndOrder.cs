namespace web_project_BE.Models
{
    public class CartAndOrder
    {
        public List<Cart_Detail> Cart { get; set; }
        public Order Order { get; set; }
    }
}
