DELETE FROM [dbo].[RolePermission]
GO

DELETE FROM [dbo].[Roles]
GO

DELETE FROM [dbo].[Permissions]
GO

INSERT [dbo].[Roles] ([Id], [Name]) VALUES
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'Portfolio Admin'),
    (N'798edb9e-df4b-4398-a19b-2d2cff006cd4', N'Site Admin'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'Customer Admin'),
    (N'f652e84e-3ca9-4e74-8ec9-7fd337b17b47', N'Portfolio Viewer'),
    (N'95da3f2f-5e36-4619-9fd8-eb0094b9f16c', N'Site Viewer')
GO

INSERT [dbo].[Permissions] ([Id], [Name], [Description]) VALUES
    (N'manage-apps', N'Manage Apps', N'Install and uninstall Marketplace Apps, update the apps'' configuration'),
    (N'manage-connectors', N'Manage Connectors', N'Manage connectors'),
    (N'manage-floors', N'Manage Floors', N'Manage zones, link data points to zones'),
    (N'manage-portfolios', N'Manage Portfolios', N'Create, update and delete portfolios'),
    (N'manage-sites', N'Manage Buildings', N'Create, update and delete buildings'),
    (N'manage-users', N'Manage Users', N'Create, update and delete customer users'),
    (N'use-timemachine', N'Use Time Machine', N'Use Time Machine'),
    (N'view-apps', N'View apps', N'View marketplace and apps. View the installed apps'),
    (N'view-portfolios', N'View Portfolios', N'View portfolios'),
    (N'view-sites', N'View Buildings', N'View buildings'),
    (N'view-users', N'View Users', N'View customer users')
GO

INSERT [dbo].[RolePermission] ([RoleId], [PermissionId]) VALUES
	(N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-apps'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-connectors'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-floors'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-portfolios'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-sites'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'manage-users'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'use-timemachine'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'view-apps'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'view-portfolios'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'view-sites'),
    (N'48174c3b-57ed-4d0d-badc-6c9d8d0afab1', N'view-users'),

	(N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'manage-apps'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'manage-connectors'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'manage-floors'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'manage-sites'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'use-timemachine'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'view-apps'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'view-portfolios'),
    (N'622723ae-ded8-45aa-a7fb-0d2d307ee65d', N'view-sites'),

    (N'f652e84e-3ca9-4e74-8ec9-7fd337b17b47', N'use-timemachine'),
    (N'f652e84e-3ca9-4e74-8ec9-7fd337b17b47', N'view-apps'),
    (N'f652e84e-3ca9-4e74-8ec9-7fd337b17b47', N'view-portfolios'),
    (N'f652e84e-3ca9-4e74-8ec9-7fd337b17b47', N'view-sites'),

    (N'798edb9e-df4b-4398-a19b-2d2cff006cd4', N'use-timemachine'),
    (N'798edb9e-df4b-4398-a19b-2d2cff006cd4', N'view-apps'),
    (N'798edb9e-df4b-4398-a19b-2d2cff006cd4', N'manage-apps'),
    (N'798edb9e-df4b-4398-a19b-2d2cff006cd4', N'view-sites'),

    (N'95da3f2f-5e36-4619-9fd8-eb0094b9f16c', N'use-timemachine'),
    (N'95da3f2f-5e36-4619-9fd8-eb0094b9f16c', N'view-apps'),
    (N'95da3f2f-5e36-4619-9fd8-eb0094b9f16c', N'manage-apps'),
    (N'95da3f2f-5e36-4619-9fd8-eb0094b9f16c', N'view-sites')
GO
