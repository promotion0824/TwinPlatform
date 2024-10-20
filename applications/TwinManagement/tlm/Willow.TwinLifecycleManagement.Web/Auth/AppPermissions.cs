namespace Willow.TwinLifecycleManagement.Web.Auth;

/// <summary>
/// Class to hold TLM Application permission constants.
/// </summary>
public static class AppPermissions
{
    #region Delete

    /// <summary>
    /// Can Delete Models Permission.
    /// </summary>
    public const string CanDeleteModels = nameof(CanDeleteModels);

    /// <summary>
    /// Can Delete Twins By Site Id Permission.
    /// </summary>
    public const string CanDeleteTwinsBySiteId = nameof(CanDeleteTwinsBySiteId);

    /// <summary>
    /// Can Delete All Twins Permission.
    /// </summary>
    public const string CanDeleteAllTwins = nameof(CanDeleteAllTwins);

    /// <summary>
    /// Can Delete specific Twins Permission.
    /// </summary>
    public const string CanDeleteTwins = nameof(CanDeleteTwins);

    /// <summary>
    /// Can Delete Twins or Relationship by File Permission.
    /// </summary>
    public const string CanDeleteTwinsorRelationshipByFile = nameof(CanDeleteTwinsorRelationshipByFile);
    #endregion

    #region DQRule

    /// <summary>
    /// Can Upload Data Quality Rules Permission.
    /// </summary>
    public const string CanUploadDQRules = nameof(CanUploadDQRules);

    /// <summary>
    /// Can Read Data Quality Rules Permission.
    /// </summary>
    public const string CanReadDQRules = nameof(CanReadDQRules);

    #endregion

    #region DQValidation

    /// <summary>
    /// Can Read Data Quality Validation Jobs Permission.
    /// </summary>
    public const string CanReadDQValidationJobs = nameof(CanReadDQValidationJobs);

    /// <summary>
    /// Can Validate Twins Permission.
    /// </summary>
    public const string CanValidateTwins = nameof(CanValidateTwins);

    /// <summary>
    /// Can Read Data Quality Validation Results Permission.
    /// </summary>
    public const string CanReadDQValidationResults = nameof(CanReadDQValidationResults);
    #endregion

    #region Export

    /// <summary>
    /// Can Export Twins Permission.
    /// </summary>
    public const string CanExportTwins = nameof(CanExportTwins);

    /// <summary>
    /// Can Export ADX jobs Permission.
    /// </summary>
    public const string CanStartAdxExportJob = nameof(CanStartAdxExportJob);

    #endregion

    #region FileImport

    /// <summary>
    /// Can Import Twins Permission.
    /// </summary>
    public const string CanImportTwins = nameof(CanImportTwins);

    /// <summary>
    /// Can Import Documents Permission.
    /// </summary>
    public const string CanImportDocuments = nameof(CanImportDocuments);

    /// <summary>
    /// Can Read Documents Permission.
    /// </summary>
    public const string CanReadDocuments = nameof(CanReadDocuments);

    #endregion

    #region GitImport

    /// <summary>
    /// Can Import Models From Git Permission.
    /// </summary>
    public const string CanImportModelsFromGit = nameof(CanImportModelsFromGit);

    #endregion

    #region JobStatus

    /// <summary>
    /// Can Read Jobs Permission.
    /// </summary>
    public const string CanReadJobs = nameof(CanReadJobs);

    /// <summary>
    /// Can Cancel Jobs Permission.
    /// </summary>
    public const string CanCancelJobs = nameof(CanCancelJobs);

    /// <summary>
    /// Can Delete Jobs Permission.
    /// </summary>
    public const string CanDeleteJobs = nameof(CanDeleteJobs);

    #endregion

    #region UnifiedJobs
    /// <summary>
    /// Can create or update Jobs permission
    /// </summary>
    public const string CanCreateOrUpdateJobs = nameof(CanCreateOrUpdateJobs);

    #endregion

    #region Models

    /// <summary>
    /// Can Read Models Permission.
    /// </summary>
    public const string CanReadModels = nameof(CanReadModels);

    #endregion

    #region Twins

    /// <summary>
    /// Can Read Twins Permission.
    /// </summary>
    public const string CanReadTwins = nameof(CanReadTwins);
    #endregion

    #region Mappings

    /// <summary>
    /// Can Read Mappings Permission.
    /// </summary>
    public const string CanReadMappings = nameof(CanReadMappings);

    /// <summary>
	/// Can Sync to Mapped Permission.
	/// </summary>
	public const string CanSyncToMapped = nameof(CanSyncToMapped);

    #endregion

    #region SyncJobs
    public const string CanTriggerSyncJobs = nameof(CanTriggerSyncJobs);
    #endregion

    /// <summary>
    /// Can Search Documents Permission.
    /// </summary>
    public const string CanSearchDocuments = nameof(CanSearchDocuments);

    /// <summary>
    /// Can Chat with Copilot.
    /// </summary>
    public const string CanChatWithCopilot = nameof(CanChatWithCopilot);
}
