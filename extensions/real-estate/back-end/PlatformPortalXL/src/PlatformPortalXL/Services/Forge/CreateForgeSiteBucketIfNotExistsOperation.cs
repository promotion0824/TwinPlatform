using Autodesk.Forge.Model;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlatformPortalXL.Services.Forge
{
    public class CreateForgeSiteBucketIfNotExistsOperation : AbstractOperation
    {
        public CreateForgeSiteBucketIfNotExistsOperation(ForgeOperationContext context)
            : base(context)
        {
        }

        protected override async Task DoExecuteAsync(CancellationToken cancellationToken)
        {
            if (!(await IsBucketExists()))
            {
                await CreateBucket();
            }

            var info = Context.ForgeInfo;
            info.BucketKey = CreateBucketName();
        }

        private async Task<bool> IsBucketExists()
        {
            var key = CreateBucketName();
            string lastBucketKey = null;
            do
            {
                var buckets = await this.Context.ForgeApi.GetBucketsAsync(startAt: lastBucketKey);
                foreach (KeyValuePair<string, dynamic> bucket in new DynamicDictionaryItems(buckets.items))
                {
                    if (bucket.Value.bucketKey == key)
                    {
                        return true;
                    }
                    lastBucketKey = bucket.Value.bucketKey;
                }

                if (!(buckets as DynamicJsonResponse).Dictionary.ContainsKey("next"))
                {
                    return false;
                }
            }
            while (true);
        }

        private async Task CreateBucket()
        {
            var bucketKey = CreateBucketName();
            var payload = new PostBucketsPayload(bucketKey, PolicyKey: PostBucketsPayload.PolicyKeyEnum.Persistent);
            await this.Context.ForgeApi.CreateBucketAsync(payload);
        }

        //Must be globally unique Possible values: -_.a-z0-9 (between 3-128 characters in length)
        //https://forge.autodesk.com/en/docs/data/v2/reference/http/buckets-POST/
        public string CreateBucketName()
        {
            var bucketName = $"willow-site-{Context.SiteId}";
            if (!string.IsNullOrEmpty(Context.BucketPostfix))
            {
                bucketName += $"-{Context.BucketPostfix}";
            }
            return bucketName;
        }
    }
}
