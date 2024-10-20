using System;

namespace WorkflowCore.Models
{
    public class Customer
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? LogoId { get; set; }
        public string LogoPath { get; set; }
        public string AccountExternalId { get; set; }
    }
}
