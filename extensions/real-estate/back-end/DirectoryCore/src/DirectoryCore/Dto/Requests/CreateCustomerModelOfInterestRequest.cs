using System.ComponentModel.DataAnnotations;

namespace DirectoryCore.Dto.Requests
{
    public class CreateCustomerModelOfInterestRequest
    {
        [Required]
        public string ModelId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }

        [Required]
        public string Text { get; set; }
        public string Icon { get; set; }
    }
}
