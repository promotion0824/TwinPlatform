using System;
using MobileXL.Models.Enums;

namespace MobileXL.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? LogoId { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
        public CustomerStatus Status { get; set; }
        public string SingleTenantUrl { get; set; }
    }
}
