using Authorization.Common;
using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Web.Abstracts;
using Authorization.TwinPlatform.Web.Helper;
using System.IO.Compression;

namespace Authorization.TwinPlatform.Web.Services;

/// <summary>
/// Service to manage Import / Export of Authorization Instance Data
/// </summary>
public class ImportExportManager(IPermissionService permissionService,
    IRoleAssignmentService roleAssignmentService,
    IGroupRoleAssignmentService groupRoleAssignmentService,
    IUserService userService,
    IGroupService groupService,
    IUserGroupService userGroupService,
    ILogger<ImportExportManager> logger,
    IUserAuthorizationManager authorizationManager,
    IAuditLogger<ImportExportManager> auditLogger) : BaseManager, IImportExportManager
{

    private readonly string[] supportedFileTypes = [PermissionFileModel.Type, UserFileModel.Type, GroupFileModel.Type, GroupUserFileModel.Type, UserRoleAssignmentFileModel.Type, GroupRoleAssignmentFileModel.Type];

    /// <summary>
    /// Gets the entity types supported for import/export operation;
    /// </summary>
    /// <returns>Array of supported entity types.</returns>
    public string[] GetSupportedRecordTypes() => supportedFileTypes;

    /// <summary>
    /// Method to import Authorization Data
    /// </summary>
    /// <param name="zipFileStream">ZipArchive File Stream</param>
    /// <returns>Report File byte array.</returns>
    public async Task<byte[]> ImportRecordsAsync(Stream zipFileStream)
    {
        logger.LogInformation("Request received to import records.");
        using (var compressedStream = new MemoryStream())
        {
            using (var outputZipArchive = new ZipArchive(compressedStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                using var zipArchive = new ZipArchive(zipFileStream, ZipArchiveMode.Read, leaveOpen: false);
                foreach (var zipArchiveEntry in zipArchive.Entries)
                {
                    using var fileStream = zipArchiveEntry.Open();

                    var fileNameWithoutExt = Path.GetFileNameWithoutExtension(zipArchiveEntry.Name);
                    // Process individual files
                    if (string.Equals(fileNameWithoutExt, PermissionFileModel.Type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var records = CsvConverter.ConvertFromCSVStream<PermissionFileModel>(fileStream);
                        logger.LogInformation("Found {count} permission record(s) to import.", records.Count());
                        var importedRecords = await permissionService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, PermissionFileModel.Type);
                        // Audit log
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(PermissionFileModel.Type, RecordAction.Import, null));
                    }
                    else if (string.Equals(fileNameWithoutExt, UserFileModel.Type, StringComparison.InvariantCultureIgnoreCase))    // User import
                    {
                        var records = CsvConverter.ConvertFromCSVStream<UserFileModel>(fileStream);
                        logger.LogInformation("Found {count} user record(s) to import.", records.Count());
                        var importedRecords = await userService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, UserFileModel.Type);
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(UserFileModel.Type, RecordAction.Import, null));
                    }
                    else if (string.Equals(fileNameWithoutExt, GroupFileModel.Type, StringComparison.InvariantCultureIgnoreCase))    // Group import
                    {
                        var records = CsvConverter.ConvertFromCSVStream<GroupFileModel>(fileStream);
                        logger.LogInformation("Found {count} group record(s) to import.", records.Count());
                        var importedRecords = await groupService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, GroupFileModel.Type);
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(GroupFileModel.Type, RecordAction.Import, null));
                    }
                    else if (string.Equals(fileNameWithoutExt, GroupUserFileModel.Type, StringComparison.InvariantCultureIgnoreCase))    // Group User import
                    {
                        var records = CsvConverter.ConvertFromCSVStream<GroupUserFileModel>(fileStream);
                        logger.LogInformation("Found {count} group-user record(s) to import.", records.Count());
                        var importedRecords = await userGroupService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, GroupUserFileModel.Type);
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(GroupUserFileModel.Type, RecordAction.Import, null));
                    }
                    else if (string.Equals(fileNameWithoutExt, UserRoleAssignmentFileModel.Type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var records = CsvConverter.ConvertFromCSVStream<UserRoleAssignmentFileModel>(fileStream);
                        logger.LogInformation("Found {count} user role assignment record(s) to import.", records.Count());
                        var importedRecords = await roleAssignmentService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, UserRoleAssignmentFileModel.Type);
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(UserRoleAssignmentFileModel.Type, RecordAction.Import, null));
                    }
                    else if (string.Equals(fileNameWithoutExt, GroupRoleAssignmentFileModel.Type, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var records = CsvConverter.ConvertFromCSVStream<GroupRoleAssignmentFileModel>(fileStream);
                        logger.LogInformation("Found {count} group role assignment record(s) to import.", records.Count());
                        var importedRecords = await groupRoleAssignmentService.ImportAsync(records);
                        await CsvConverter.CopyRecordsToZipAsync(importedRecords.ToList(), outputZipArchive, GroupRoleAssignmentFileModel.Type);
                        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(GroupRoleAssignmentFileModel.Type, RecordAction.Import, null));
                    }
                }
            }
            compressedStream.Position = 0;
            return compressedStream.ToArray();
        }
    }

    /// <summary>
    /// Export all record types matching the input entity type list.
    /// </summary>
    /// <param name="recordTypes">Array of Entity Types.</param>
    /// <returns>ByteArray of Zip data.</returns>
    public async Task<byte[]> ExportRecordsByTypesAsync(string[] recordTypes)
    {
        logger.LogInformation("Request received to export records of {types}", string.Join(',', recordTypes));
        using var compressedStream = new MemoryStream();
        using (var zipArchive = new ZipArchive(compressedStream, ZipArchiveMode.Create, leaveOpen: false))
        {
            if (recordTypes.Contains(PermissionFileModel.Type))
            {
                // Add Permissions
                var allPermissions = await permissionService.GetListAsync<PermissionFileModel>(new FilterPropertyModel());
                logger.LogInformation("Retrieved {count} permission records for export.", allPermissions.Count);
                await CsvConverter.CopyRecordsToZipAsync(allPermissions, zipArchive, PermissionFileModel.Type);
            }

            if (recordTypes.Contains(UserFileModel.Type))
            {
                // Add Users
                var allUsers = await userService.GetListAsync<UserFileModel>(new FilterPropertyModel());
                logger.LogInformation("Retrieved {count} user records for export.", allUsers.Count);
                await CsvConverter.CopyRecordsToZipAsync(allUsers, zipArchive, UserFileModel.Type);
            }

            if (recordTypes.Contains(GroupFileModel.Type))
            {
                // Add Groups   
                var allGroups = await groupService.GetListAsync<GroupFileModel>(new FilterPropertyModel());

                logger.LogInformation("Retrieved {count} group records for export.", allGroups.Count);
                await CsvConverter.CopyRecordsToZipAsync(allGroups, zipArchive, GroupFileModel.Type);
            }

            if (recordTypes.Contains(GroupUserFileModel.Type))
            {
                // Add Group - User relationships
                var allRelationships = await userGroupService.GetAll<GroupUserFileModel>();
                logger.LogInformation("Retrieved {count} user-group relationship records for export.", allRelationships.Count);
                await CsvConverter.CopyRecordsToZipAsync(allRelationships, zipArchive, GroupUserFileModel.Type);
            }

            if (recordTypes.Contains(UserRoleAssignmentFileModel.Type))
            {
                // Add User Assignments
                var allRoleAssignments = await roleAssignmentService.GetAssignmentsAsync<UserRoleAssignmentFileModel>();
                logger.LogInformation("Retrieved {count} role assignments records for export.", allRoleAssignments.Count);
                await CsvConverter.CopyRecordsToZipAsync(allRoleAssignments, zipArchive, UserRoleAssignmentFileModel.Type);
            }

            if (recordTypes.Contains(GroupRoleAssignmentFileModel.Type))
            {
                // Add Group Assignments
                var allGroupRoleAssignments = await groupRoleAssignmentService.GetAssignmentsAsync<GroupRoleAssignmentFileModel>();
                logger.LogInformation("Retrieved {count} group role assignments records for export.", allGroupRoleAssignments.Count);
                await CsvConverter.CopyRecordsToZipAsync(allGroupRoleAssignments, zipArchive, GroupRoleAssignmentFileModel.Type);
            }

        }

        // Audit log
        auditLogger.LogInformation(authorizationManager.CurrentEmail, AuditLog.Format(string.Join(',', recordTypes), RecordAction.Export, null));
        return compressedStream.ToArray();
    }
}
