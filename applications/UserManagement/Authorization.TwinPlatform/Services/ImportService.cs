using Authorization.Common.Models;
using Authorization.TwinPlatform.Abstracts;
using Authorization.TwinPlatform.Extensions;
using Authorization.TwinPlatform.Factories;
using Authorization.TwinPlatform.Persistence.Contexts;
using Authorization.TwinPlatform.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace Authorization.TwinPlatform.Services;

/// <summary>
///  Service for handling Authorization data imports
/// </summary>
public class ImportService(TwinPlatformAuthContext authContext, ILogger<ImportService> logger) : IImportService
{

    /// <summary>
    /// Update Application Entity with Application Option.
    /// </summary>
    /// <param name="applicationModel">Application Model.</param>
    /// <param name="applicationOption">Application Options.</param>
    /// <returns>Awaitable task.</returns>
    public async Task UpdateApplication(ApplicationModel applicationModel, ApplicationOption applicationOption)
    {
        logger.LogTrace("Updating Application: {ApplicationName} entity.", applicationModel.Name);
        bool hasChanges = false;
        applicationOption.Description ??= string.Empty;
        if (applicationModel.Description != applicationOption.Description)
        {
            applicationModel.Description = applicationOption.Description;
            hasChanges = true;
        }
        if (applicationModel.SupportClientAuthentication != applicationOption.SupportClientAuthentication)
        {
            applicationModel.SupportClientAuthentication = applicationOption.SupportClientAuthentication;
            hasChanges = true;
        }
        if (hasChanges)
        {
            await authContext.UpdateAsync(EntityFactory.ConstructApplication(applicationModel), saveChanges: true);
        }
        logger.LogTrace("Application: {ApplicationName} Updated Successfully.", applicationModel.Name);
    }

    /// <summary>
    /// Method to import permissions
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createPermissions">List of create permissions</param>
    /// <returns>Completed task</returns>
    public async Task ImportPermissionsAsync(ApplicationModel application, List<CreatePermissionModel> createPermissions)
    {
        if (createPermissions?.Any() != true)
        {
            logger.LogTrace("Found 0 permissions to import");
            return;
        }

        ArgumentNullException.ThrowIfNull(application, nameof(application));

        if (string.IsNullOrWhiteSpace(application?.Name))
        {
            logger.LogTrace("Import Permission failed, Application Name cannot be empty,");

            throw new ArgumentNullException(nameof(application));
        }

        logger.LogTrace("Importing Permission for Application: {Application}", application?.Name);

        // Get all existing permission for the application from DB
        var extensionPermissions = await authContext.Permissions
            .AsNoTracking()
            .Where(x => x.ApplicationId == application!.Id)
            .ToListAsync();

        // Add/update permissions that are not exist in the database
        foreach (var model in createPermissions)
        {
            var existingPermission = extensionPermissions.Find(f => string.Compare(f.Name, model.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            if (existingPermission is null)
            {
                var newPermissionToInsert = EntityFactory.ConstructPermission(model, application!.Id);
                // Add Permission Entity without saving changes
                await authContext.AddEntityAsync(newPermissionToInsert, false);
            }
            else if (existingPermission.Description != model.Description)
            {
                existingPermission.Description = model.Description ?? string.Empty;
                // Update Permission Entity
                await authContext.UpdateAsync(existingPermission);
            }
        }

        // Remove permission that are not exist in the create permission array
        var permissionsToRemove = extensionPermissions.Where(z => !createPermissions.Exists(a => a.Name == z.Name));
        foreach (var modelToDelete in permissionsToRemove)
        {
            //Remove Permission Entity without saving changes
            await authContext.RemoveEntityAsync<Permission>(modelToDelete, false);
        }

        //Save all the changes here
        await authContext.SaveChangesAsync();

        logger.LogTrace("Import Permission completed successfully");

    }

    /// <summary>
    /// Method to import roles
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createRoleModels">List of create roles instances</param>
    /// <returns>Completed task</returns>
    public async Task ImportRolesAsync(ApplicationModel application, List<CreateRoleModel> createRoleModels)
    {
        ArgumentNullException.ThrowIfNull(application, nameof(application));

        logger.LogTrace("Importing Roles for Application: {Application}", application.Name);

        if (createRoleModels?.Any() != true)
        {
            logger.LogTrace("Found 0 roles to import");
            return;
        }

        var existingRoles = await authContext.Roles.AsNoTracking().ToListAsync();

        //Create Roles that does not exist in the database
        foreach (var model in createRoleModels)
        {
            var existingRole = existingRoles.Find(f => string.Compare(f.Name, model.Name, StringComparison.InvariantCultureIgnoreCase) == 0);
            var targetRole = EntityFactory.ConstructRole(model);
            if (existingRole is null)
            {
                await authContext.AddEntityAsync(targetRole, false);
            }
            else
            {
                // See if the description property need an update
                if (existingRole.Description != targetRole.Description)
                {
                    existingRole.Description = targetRole.Description;
                    await authContext.UpdateAsync(existingRole);
                }
            }
        }

        //we do not remove role since Role entity can be used across multiple application

        await authContext.SaveChangesAsync();
        logger.LogTrace("Import Roles completed successfully");
    }

    /// <summary>
    /// Method to update permissions within roles
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createRoleModels">List of create roles instances</param>
    /// <returns>Completed task</returns>
    public async Task UpdateRolePermissions(ApplicationModel application, List<CreateRoleModel> createRoleModels)
    {
        ArgumentNullException.ThrowIfNull(application, nameof(application));

        logger.LogTrace("Updating Role Permissions for Application: {Application}", application.Name);

        var roleNamesToUpdate = createRoleModels.Select(x => x.Name);

        var roleEntitiesToUpdate = await authContext.Roles
            .Include(x => x.RolePermission)
            .ThenInclude(x => x.Permission)
            .Where(w => roleNamesToUpdate.Contains(w.Name))
            .ToListAsync();

        var allExtPerms = await authContext.Permissions.AsNoTracking().Where(w => w.ApplicationId == application.Id).ToListAsync();

        logger.LogInformation("Incoming Create Role names:{RoleNames}", string.Join(",", roleNamesToUpdate));
        logger.LogInformation("Existing Role Entities selected for update:{RoleEntityNames}", string.Join(",", roleEntitiesToUpdate.Select(s => s.Name)));

        foreach (var roleEntity in roleEntitiesToUpdate)
        {
            var targetRole = createRoleModels.Find(f => f.Name == roleEntity.Name);

            if (targetRole is null)
            {
                logger.LogWarning("Role Entity name: {RoleEntityName} does not exist while updating permission for application:{Application}", roleEntity.Name, application.Name);
                continue;
            }

            var targetPermissions = allExtPerms.Where(w => targetRole.Permissions is not null && targetRole.Permissions.Contains(w.Name));

            var (permissionIdToAdd, permissionIdToRemove) = roleEntity.RolePermission.Where(w => w.Permission.ApplicationId == application.Id).Select(s => s.PermissionId)
                                                        .GetDelta(targetPermissions.Select(s => s.Id));

            foreach (var id in permissionIdToAdd)
            {
                roleEntity.RolePermission.Add(new RolePermission
                {
                    PermissionId = id,
                    RoleId = roleEntity.Id
                });
            }

            foreach (var permissionId in permissionIdToRemove)
            {
                var rolePermission = roleEntity.RolePermission.Single(x => x.PermissionId == permissionId);
                roleEntity.RolePermission.Remove(rolePermission);
            }
        }

        await authContext.SaveChangesAsync();
        logger.LogTrace("Updating Role Permissions completed successfully.");

    }


    /// <summary>
    /// Method to import groups
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createGroupModels">List of create group model instances</param>
    /// <returns>Awaitable task</returns>
    public async Task ImportGroups(ApplicationModel application, List<CreateGroupModel> createGroupModels)
    {
        ArgumentNullException.ThrowIfNull(application, nameof(application));

        logger.LogTrace("Importing groups for Application: {Application}", application.Name);

        if (createGroupModels?.Any() != true)
        {
            logger.LogTrace("Found 0 groups to import. Returning.");
            return;
        }

        var existingGroups = await authContext.Groups.AsNoTracking().ToListAsync();
        var existingGroupTypes = await authContext.GroupTypes.AsNoTracking().ToListAsync();

        //Create groups that are not exist in the database
        foreach (var model in createGroupModels)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                logger.LogWarning("Group Import Model cannot have empty group name. Skipping import.");
                continue;
            }

            // Get the group type to import
            var groupType = existingGroupTypes.SingleOrDefault(w => w.Name.ToLowerInvariant() == model.Type?.ToLowerInvariant());
            if (string.IsNullOrWhiteSpace(model.Type) || groupType is null)
            {
                logger.LogWarning("Group Import Model must have a valid group type. Skipping import for group:{GroupName}.", model.Name);
                continue;
            }

            // check if a group with the same name exist
            var existingGroupFound = existingGroups.Find(w => w.Name.ToLowerInvariant() == model.Name.ToLowerInvariant());

            if (existingGroupFound is not null)
            {
                logger.LogWarning("Duplicate group name found. Skipping import for group:{GroupName}.", model.Name);
                continue;
            }

            var groupToInsert = new Group()
            {
                Name = model.Name,
                GroupTypeId = groupType.Id
            };
            await authContext.AddEntityAsync<Group>(groupToInsert);
        }
        await authContext.SaveChangesAsync();
        logger.LogTrace("Import groups completed for Application: {Application}.", application.Name);
    }

    /// <summary>
    /// Import group assignment from configuration.
    /// </summary>
    /// <param name="application">Application Model.</param>
    /// <param name="createGroupAssignmentModels">List of create group assignment models.</param>
    /// <returns>Awaitable task.</returns>
    public async Task ImportGroupAssignments(ApplicationModel application, List<CreateGroupAssignmentModel> createGroupAssignmentModels)
    {
        ArgumentNullException.ThrowIfNull(application, nameof(application));

        logger.LogTrace("Importing group assignment for Application: {Application}", application.Name);

        if (createGroupAssignmentModels?.Any() != true)
        {
            logger.LogWarning("Found 0 group assignments to import. Returning.");
            return;
        }

        var existingGroups = await authContext.Groups.AsNoTracking().ToListAsync();
        var existingRoles = await authContext.Roles.AsNoTracking().ToListAsync();
        var existingGroupAssignments = await authContext.GroupRoleAssignments.AsNoTracking().ToListAsync();

        //Create groups that are not exist in the database
        foreach (var model in createGroupAssignmentModels)
        {
            var group = existingGroups.Find(f => f.Name.ToLowerInvariant() == model.GroupName?.ToLowerInvariant());

            if (group is null)
            {
                logger.LogWarning("Group not found for assignment. Skipping assignment import for group:{group}.", model.GroupName);
                continue;
            }

            var role = existingRoles.Find(f => f.Name.ToLowerInvariant() == model.RoleName?.ToLowerInvariant());

            if (role is null)
            {
                logger.LogWarning("Role not found for assignment. Skipping assignment import for role:{RoleName}.", model.RoleName);
                continue;
            }

            var existingAssignment = existingGroupAssignments.Find(f => f.GroupId == group.Id && f.RoleId == role.Id && f.Expression == model.Expression);
            if (existingAssignment is not null)
            {
                logger.LogWarning("Duplicate assignment found. Skipping assignment import for group:{GroupName} - role:{RoleName}.", model.GroupName, model.RoleName);
                continue;
            }

            var groupAssignmentToImport = new GroupRoleAssignment()
            {
                GroupId = group.Id,
                RoleId = role.Id,
                Expression = model.Expression,
                Condition = model.Condition
            };

            await authContext.AddEntityAsync(groupAssignmentToImport);
        }

        await authContext.SaveChangesAsync();
        logger.LogTrace("Import groups assignments completed for Application: {Application}.", application.Name);
    }

    const string FailedActionFormat = "Action {0}: Failed with Message: {1}";
    const string SuccessActionFormat = "Action {0}: Succeeded";
    const string SkippedActionFormat = "Skipped Action {0} did not match.";
    internal static async Task ExecuteFileImport<TEntity, TFileModel>(
        IEnumerable<TFileModel> importRows,
        Func<TFileModel, TEntity?> getEntityRecord,
        TwinPlatformAuthContext authContext,
        Func<TEntity, IQueryable<TEntity>> getUniqueRecordQuery,
        string[] uniqueFieldNames)
        where TEntity : class, IEntityBase
        where TFileModel : BaseFileImportModel
    {
        foreach (var fileRecord in importRows)
        {
            try
            {
                // Ignore NoAction records
                if (fileRecord.Action == FileImportRecordAction.NoAction)
                {
                    continue;
                }

                if (fileRecord.Action == FileImportRecordAction.Update || fileRecord.Action == FileImportRecordAction.Delete)
                {
                    if (string.IsNullOrEmpty(fileRecord.Id))
                    {
                        fileRecord.Message = string.Format(FailedActionFormat, fileRecord.Action.ToString(), "Id is a required value.");
                        continue;
                    }
                }

                var currentRecord = getEntityRecord(fileRecord);
                if (currentRecord == null)
                {
                    continue;
                }
                var uniqueRecordQuery = getUniqueRecordQuery(currentRecord);

                switch (fileRecord.Action)
                {
                    case FileImportRecordAction.Create:
                        {
                            // Found duplicate record for Create
                            if (await uniqueRecordQuery.AnyAsync())
                            {
                                fileRecord.Message = string.Format(FailedActionFormat, fileRecord.Action.ToString(), $"Duplicate record error. Fields [{string.Join(',',uniqueFieldNames)}] should be unique.");
                                continue;
                            }

                            var insertedId = await authContext.AddEntityAsync(currentRecord, saveChanges: true);
                            fileRecord.Id = insertedId.ToString();
                            break;
                        }
                    case FileImportRecordAction.Update:
                        {
                            // Found duplicate record for Update
                            if (await uniqueRecordQuery.AnyAsync(a => a.Id != currentRecord.Id))
                            {
                                fileRecord.Message = string.Format(FailedActionFormat, fileRecord.Action.ToString(), $"Duplicate record error. Fields [{string.Join(',', uniqueFieldNames)}] should be unique.");
                                continue;
                            }

                            await authContext.UpdateAsync(currentRecord, saveChanges: true);
                            break;
                        }
                    case FileImportRecordAction.Delete:
                        {
                            // Record exist
                            if (!await authContext.Set<TEntity>().AnyAsync(a => a.Id == currentRecord.Id))
                            {
                                fileRecord.Message = string.Format(FailedActionFormat, fileRecord.Action.ToString(), $"Record with Id:{currentRecord.Id} not found.");
                                continue;
                            }

                            await authContext.RemoveEntityAsync(currentRecord, saveChanges: true);
                            break;
                        }
                    default:
                        {
                            fileRecord.Message = string.Format(SkippedActionFormat, fileRecord.Action?.ToString() ?? string.Empty);
                            break;
                        }
                }
                
                fileRecord.Message = string.Format(SuccessActionFormat, fileRecord.Action.ToString());
            }
            catch (Exception ex)
            {
                fileRecord.Message = string.Format(FailedActionFormat, fileRecord.Action.ToString(), ex.Message);
            }
        }
    }
}
