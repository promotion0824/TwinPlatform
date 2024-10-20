using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace PlatformPortalXL.Features.Workflow
{
    public static class Attachments
    {
        public static bool IsValid(IFormFileCollection attachmentFiles)
        {
            if(attachmentFiles == null)
                return true;
                      
            foreach(var attachmentFile in attachmentFiles)
            {
                try
                {
                    var stream = attachmentFile.OpenReadStream();
                    Image.Load(stream);
                }
                catch (UnknownImageFormatException)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
