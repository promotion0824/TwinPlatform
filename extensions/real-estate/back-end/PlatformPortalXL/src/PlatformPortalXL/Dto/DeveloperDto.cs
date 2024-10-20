using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class DeveloperDto
    {
        public string Name { get; set; }

        public static DeveloperDto MapFrom(Developer developer)
        {
            if (developer == null)
            {
                return null;
            }

            return new DeveloperDto
            {
                Name = developer.Name.ToLowerInvariant(),
            };
        }
    }
}
