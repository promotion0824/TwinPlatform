using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Models;

namespace PlatformPortalXL.Dto
{
    public class AppCategoryDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public static AppCategoryDto MapFrom(AppCategory category)
        {
            if (category == null)
            {
                return null;
            }

            return new AppCategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };
        }

        public static List<AppCategoryDto> MapFrom(IEnumerable<AppCategory> categories)
        {
            return categories?.Select(MapFrom).ToList();
        }
    }
}
