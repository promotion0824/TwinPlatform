using System;

namespace MobileXL.Models
{
    public class CustomerTicketStatus
    {
        public Guid CustomerId { get; set; }
        public int StatusCode { get; set; }
        public string Status { get; set; }
        public string Tab { get; set; }
        public string Color { get; set; }
    }
}
