using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Willow.Common;

namespace PlatformPortalXL.Services.Forge
{
    public class UploadFileToForgeServerOperation : AbstractOperation
    {
        private readonly IDateTimeService _dateTimeService;

        public UploadFileToForgeServerOperation(ForgeOperationContext context, IDateTimeService dateTimeService)
            : base(context)
        {
            _dateTimeService = dateTimeService;
        }

        protected new ForgeOperationContext Context => base.Context;

        protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
        {
            var result = await UploadChunks();
            Context.ForgeInfo.FileObjectId = result.objectId;
        }

        private async Task<dynamic> UploadChunks(long chunkSize = 2 * 1024 * 1024)
        {
            var fileName = Context.File.FileName;
            var uniqueFileName = Path.GetFileNameWithoutExtension(fileName) + "_" + _dateTimeService.UtcNow.ToString("yyyyMMddHHmmss", CultureInfo.InvariantCulture) + Path.GetExtension(fileName);
            var fileSize = Context.File.Length;
            long numberOfChunks = (fileSize + chunkSize - 1) / chunkSize;

            var id = Context.Id.ToString();

            var start = 0L;
            chunkSize = (numberOfChunks > 1 ? chunkSize : fileSize);
            var end = chunkSize - 1;
            dynamic result = null;

            var fileBytes = Context.GetFileBytes();

            using (MemoryStream readStream = new MemoryStream(fileBytes))
            {
                for (int chunk = 0; chunk < numberOfChunks; chunk++)
                {
                    string range = string.Format(CultureInfo.InvariantCulture, "bytes {0}-{1}/{2}", start, end, fileSize);

                    long numberOfBytes = chunkSize;
                    byte[] bytes = new byte[numberOfBytes];
                    var readByteCount = await readStream.ReadAsync(bytes, 0, (int)numberOfBytes);

                    using (var writeStream = new MemoryStream())
                    {
                        await writeStream.WriteAsync(bytes, 0, readByteCount);
                        writeStream.Position = 0;
                        result = await this.Context.ForgeApi.UploadChunkAsync(Context.ForgeInfo.BucketKey, uniqueFileName, readByteCount, range, id, writeStream);
                    }

                    start = end + 1;
                    chunkSize = ((start + chunkSize > fileSize) ? fileSize - start : chunkSize);
                    end = start + chunkSize - 1;
                }
            }

            return result;
        }
    }
}
