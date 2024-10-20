using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using DirectoryCore.Domain;
using DirectoryCore.Enums;
using Willow.Infrastructure;
using Willow.Infrastructure.Exceptions;

namespace DirectoryCore.Entities
{
    public class CustomerEntity
    {
        public Guid Id { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(200)]
        public string Address1 { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(200)]
        public string Address2 { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Suburb { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(20)]
        public string Postcode { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string Country { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(50)]
        public string State { get; set; }

        public Guid? LogoId { get; set; }

        public CustomerStatus Status { get; set; }

        [Required(AllowEmptyStrings = true)]
        [MaxLength(1000)]
        public string FeaturesJson { get; set; }

        [MaxLength(50)]
        public string AccountExternalId { get; set; }

        public string SigmaConnectionId { get; set; }

        public string ModelsOfInterestJson { get; set; }

        [ConcurrencyCheck]
        public Guid? ModelsOfInterestETag { get; set; }

        public string CognitiveSearchUri { get; set; }

        public string CognitiveSearchIndex { get; set; }

        public string SingleTenantUrl { get; set; }

        public static Customer MapTo(CustomerEntity entity)
        {
            if (entity == null)
            {
                return null;
            }

            return new Customer
            {
                Id = entity.Id,
                Name = entity.Name,
                Address1 = entity.Address1,
                Address2 = entity.Address2,
                Suburb = entity.Suburb,
                Postcode = entity.Postcode,
                Country = entity.Country,
                State = entity.State,
                Status = entity.Status,
                LogoId = entity.LogoId,
                AccountExternalId = entity.AccountExternalId,
                SigmaConnectionId = entity.SigmaConnectionId,
                Features = MapCustomerFeatures(entity.FeaturesJson),
                CognitiveSearchUri = entity.CognitiveSearchUri,
                CognitiveSearchIndex = entity.CognitiveSearchIndex,
                SingleTenantUrl = entity.SingleTenantUrl
            };
        }

        public static List<Customer> MapTo(IList<CustomerEntity> customerEntities)
        {
            return customerEntities?.Select(t => MapTo(t)).ToList();
        }

        private static CustomerFeatures MapCustomerFeatures(string featuresJson)
        {
            if (string.IsNullOrWhiteSpace(featuresJson))
            {
                featuresJson = "{}";
            }

            try
            {
                return JsonSerializerExtensions.Deserialize<CustomerFeatures>(featuresJson);
            }
            catch (Exception)
            {
                //Not a valid json format - return default customer features
            }

            return new CustomerFeatures();
        }

        public static List<CustomerModelOfInterest> MapCustomerModelsOfInterest(
            string modelsOfInterestJson
        )
        {
            if (string.IsNullOrWhiteSpace(modelsOfInterestJson))
            {
                return new List<CustomerModelOfInterest>();
            }

            try
            {
                return JsonSerializerExtensions.Deserialize<List<CustomerModelOfInterest>>(
                    modelsOfInterestJson
                );
            }
            catch (Exception ex)
            {
                //Not a valid json format - raise server exception
                throw new ServerException(ex);
            }
        }
    }
}
