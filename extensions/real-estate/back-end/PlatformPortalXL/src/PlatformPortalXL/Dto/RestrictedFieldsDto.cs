using PlatformPortalXL.Helpers;
using System.Collections.Generic;

namespace PlatformPortalXL.Dto
{
    public class RestrictedFieldsDto
    {
        /// <summary>
        /// A list of JSON pointers to twin fields that must not be sent
        /// to the client
        /// </summary>
        public string[] HiddenFields { get; set; }
        
        /// <summary>
        /// A list of JSON pointers to twin fields that should be sent to the client,
        /// but which the client is not allowed to modify.
        /// </summary>
        public string[] ReadOnlyFields { get; set; }

        /// <summary>
        /// A list of JSON pointers to twin fields that the client 
        /// should expect to have values. 
        /// </summary>
        public List<string> ExpectedFields { get; set; }

        public static RestrictedFieldsDto MapFrom(TwinFieldsDto twinFields)
        {
            return new RestrictedFieldsDto
            {
                HiddenFields = TwinHelper.Hidden,
                ReadOnlyFields = TwinHelper.ReadOnly,
                ExpectedFields = twinFields?.ExpectedFields
            };
        }
    }
}
