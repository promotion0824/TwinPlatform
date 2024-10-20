using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetCoreTwinCreator.Database;
using File = AssetCoreTwinCreator.Features.Asset.Attachments.Models.File;

namespace AssetCoreTwinCreator.Features.Asset.Attachments
{
    public interface IAttachmentsService
    {
        Task<List<File>> GetFiles(int categoryId = 0, List<int> assetRegisterIds = null, string fileName = null);
        Task<File> GetFile(int fileId);
    }
    public class AttachmentsService : IAttachmentsService
    {
        private readonly IDatabase _database;

        public AttachmentsService(IDatabase database)
        {
            _database = database;
        }

        public async Task<List<File>> GetFiles(int categoryId = 0, List<int> assetRegisterIds = null, string fileName = null)
        {
            var sql = string.Empty;
            object parameters;
            if(assetRegisterIds != null && assetRegisterIds.Any())
            {
                sql = @"SELECT
                        f.Id,
                        f.FileName,
                        f.BlobName,
                        f.Size,
                        af.AssetRegisterId
                        FROM Tes_Asset_File af
                        INNER JOIN Tes_File f
                        ON f.Id = af.FileId
                        WHERE af.assetregisterid IN @assetRegisterIds";

                parameters = new { assetRegisterIds };
            }
            else if(string.IsNullOrWhiteSpace(fileName) == false)
            {
                sql = @"SELECT
                        f.Id,
                        f.FileName,
                        f.BlobName,
                        af.AssetRegisterId
                        FROM Tes_Asset_Register ar
                        INNER JOIN Tes_Asset_File af
                        ON af.AssetRegisterId = ar.Id
                        INNER JOIN Tes_File f
                        ON f.id = af.FileId
                        WHERE ar.Archived = 0
                        AND ar.CategoryId = @categoryId
                        AND f.FileName = @fileName";
                parameters = new { categoryId, fileName };
            }
            else
            {
                if(categoryId == 0)
                {
                    throw new ArgumentException(nameof(categoryId));
                }

                sql = @"SELECT
                        f.Id,
                        f.FileName,
                        f.BlobName,
                        f.Size,
                        af.AssetRegisterId
                        FROM Tes_Asset_Register ar
                        INNER JOIN Tes_Asset_File af
                        ON af.AssetRegisterId = ar.Id
                        INNER JOIN Tes_File f
                        ON f.Id = af.FileId
                        WHERE ar.Archived = 0
                        AND ar.CategoryId = @categoryId";

                parameters = new { categoryId };
            }

            var lookup = new Dictionary<int, File>();
            await _database.Query<File, int, File>(DatabaseInstance.Build, sql,
            (f, af) =>
            {
                File file;
                if (lookup.TryGetValue(f.Id, out file) == false)
                {
                    lookup.Add(f.Id, file = f);
                }
                if (file.AssetRegisterIds == null)
                {
                    file.AssetRegisterIds = new List<int>();
                }
                file.AssetRegisterIds.Add(af);
                return file;
            }, "AssetRegisterId", parameters);

            return lookup.Values.ToList();
        }

        public async Task<File> GetFile(int fileId)
        {
            const string sql = "SELECT FileName, BlobName FROM Tes_File WHERE Id = @fileId";

            var file = await _database.Query<File>(DatabaseInstance.Build, sql, new { fileId });

            return file;
        }

    }
}