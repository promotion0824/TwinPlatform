using System;
using System.Text.RegularExpressions;
using Willow.Infrastructure.Exceptions;

namespace AssetCoreTwinCreator.MappingId.Extensions
{
    public static class GuidMappingExtensions
    {
        private static readonly Regex NumericRegex = new Regex("^[0-9]*$");

        private static bool IsValidNumericString(string guidString)
        {
            return NumericRegex.IsMatch(guidString);
        }

        public static int ToCategoryId(this Guid categoryGuid)
        {
            var guidStr = categoryGuid.ToString("N").Substring(3);
            if (!IsValidNumericString(guidStr) || !int.TryParse(guidStr, out var categoryId))
            {
                throw new BadRequestException($"Invalid guid value for CategoryId: {categoryGuid}");
            }

            return categoryId;
        }

        public static Guid ToCategoryGuid(this int categoryId)
        {
            var guidString = "003" + $"{categoryId:D29}";
            return Guid.Parse(guidString);
        }

        public static Guid ToCategoryColumnGuid(this int categoryColumnId)
        {
            var guidString = "004" + $"{categoryColumnId:D29}";
            return Guid.Parse(guidString);
        }

        public static int ToAssetId(this Guid assetIdGuid)
        {
            var guidStr = assetIdGuid.ToString("N").Substring(3);
            if (!IsValidNumericString(guidStr) || !int.TryParse(guidStr, out var categoryId) || categoryId.ToAssetGuid() != assetIdGuid)
            {
                throw new BadRequestException($"Invalid guid value for Asset: {assetIdGuid}");
            }

            return categoryId;
        }

        public static Guid ToAssetGuid(this int assetRegisterId)
        {
            var guidString = "006" + $"{assetRegisterId:D29}";
            return Guid.Parse(guidString);
        }

        public static Guid ToCompanyGuid(this int companyId)
        {
            var guidString = "007" + $"{companyId:D29}";
            return Guid.Parse(guidString);
        }

        public static Guid ToFileGuid(this int fileId)
        {
            var guidString = "008" + $"{fileId:D29}";
            return Guid.Parse(guidString);
        }

        public static int ToFileId(this Guid fileGuid)
        {
            var guidStr = fileGuid.ToString("N").Substring(3);
            if (!IsValidNumericString(guidStr) || !int.TryParse(guidStr, out var fileId))
            {
                throw new ArgumentException($"Invalid guid value for Company: {fileGuid}");
            }

            return fileId;
        }
    }
}
