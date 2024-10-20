using System;
using System.Collections.Generic;

namespace WorkflowCore.Models
{
    public class App
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public string ManifestJson { get; set; }
        public Guid IconId { get; set; }
        public string Email { get; set; }
        public string WebsiteUrl { get; set; }
        public string LicenseAgreementUrl { get; set; }
        public string PrivacyPolicyUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime PublishedDate { get; set; }
        public Guid DeveloperId { get; set; }
        public bool NeedPrerequisite { get; set; }
        public string PrerequisiteDescription { get; set; }

        public string IconPath { get; set; }
        public List<GalleryVisual> Gallery { get; set; }
        public List<AppCategory> Categories { get; set; }
        public Developer Developer { get; set; }
    }
}
