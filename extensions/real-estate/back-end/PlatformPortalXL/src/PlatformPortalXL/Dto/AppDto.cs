using System;
using System.Collections.Generic;
using System.Linq;
using PlatformPortalXL.Helpers;
using PlatformPortalXL.Models;
using PlatformPortalXL.Services;

namespace PlatformPortalXL.Dto
{
    public class AppDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AppManifest Manifest { get; set; }
        public string IconUrl { get; set; }
        public List<string> ImageUrls { get; set; }
        public List<string> CategoryNames { get; set; }
        public bool IsInstalled { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public string LicenseAgreementUrl { get; set; }
        public string PrivacyPolicyUrl { get; set; }
        public bool NeedPrerequisite { get; set; }
        public string PrerequisiteDescription { get; set; }
        public DeveloperDto Developer { get; set; }
        public string[] SupportedApplicationKinds { get; set; }

        public static AppDto MapFrom(App app, IImageUrlHelper helper)
        {
            return new AppDto
            {
                Id = app.Id,
                Name = app.Name,
                Version = app.Version,
                Description = app.Description,
                Manifest = string.IsNullOrEmpty(app.ManifestJson) ? new AppManifest() : JsonSerializerHelper.Deserialize<AppManifest>(app.ManifestJson),
                IconUrl = helper.GetAppIconUrl(app.IconPath, app.IconId),
                Email = app.Email,
                WebsiteUrl = app.WebsiteUrl,
                LicenseAgreementUrl = app.LicenseAgreementUrl,
                PrivacyPolicyUrl = app.PrivacyPolicyUrl,
                NeedPrerequisite = app.NeedPrerequisite,
                PrerequisiteDescription = app.PrerequisiteDescription,
                ImageUrls = app.Gallery?.Select(x => helper.GetAppGalleryImagePath(x.ImagePath, x.ImageId)).ToList(),
                CategoryNames = app.Categories?.Select(x => x.Name).ToList(),
                Developer = DeveloperDto.MapFrom(app.Developer),
                SupportedApplicationKinds = app.SupportedApplicationKinds?.Select(s => s.ToLowerInvariant()).ToArray()
            };
        }

        public static List<AppDto> MapFrom(IEnumerable<App> apps, IImageUrlHelper helper)
        {
            return apps?.Select(x => MapFrom(x, helper)).ToList();
        }
    }
}
