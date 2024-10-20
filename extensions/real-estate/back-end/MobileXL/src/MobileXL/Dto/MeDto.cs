using MobileXL.Models;
using System;
using System.Collections.Generic;

namespace MobileXL.Dto
{
    public class MeDto
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Initials { get; set; }
        public string Email { get; set; }
        public string Company { get; set; }
        public string AccountExternalId { get; set; }
        public Guid CustomerId { get; set; }
        public CustomerUserPreferences Preferences { get; set; }
        public List<SiteSimpleDto> Sites { get; set; }
        public Customer Customer { get; set; }
    }
}
