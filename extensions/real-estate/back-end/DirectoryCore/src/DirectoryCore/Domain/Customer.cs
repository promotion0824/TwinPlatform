using System;
using System.Collections.Generic;
using DirectoryCore.Enums;

namespace DirectoryCore.Domain
{
    public class Customer
    {
        public IList<Site> Sites { get; set; } = new List<Site>();

        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string Suburb { get; set; }

        public string Postcode { get; set; }

        public string Country { get; set; }

        public string State { get; set; }

        public Guid? LogoId { get; set; }

        public CustomerStatus Status { get; set; }

        public string AccountExternalId { get; set; }

        public string SigmaConnectionId { get; set; }

        public CustomerFeatures Features { get; set; }

        public string CognitiveSearchUri { get; set; }

        //The name of an index within the CognitiveSearchUri
        public string CognitiveSearchIndex { get; set; }

        /// <summary>
        /// If the customer has been migrated to single tenant,
        /// this will be the URL to their single tenant instance. For example
        /// "https://sanofi.app.willowinc.com"
        /// </summary>
        public string SingleTenantUrl { get; set; }
    }
}
