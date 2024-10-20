ALTER TABLE dbo.Sites ADD
	ConstructionYear int NULL,
	SiteCode nvarchar(20) NULL,
	SiteContactName nvarchar(100) NULL,
	SiteContactEmail nvarchar(100) NULL,
	SiteContactTitle nvarchar(50) NULL,
	SiteContactPhone nvarchar(50) NULL
GO

ALTER TABLE dbo.Metrics ADD
	FormatString nvarchar(64) NOT NULL CONSTRAINT DF_Metrics_FormatString DEFAULT '',
	WarningLimit decimal(15, 6) NOT NULL CONSTRAINT DF_Metrics_WarningLimit DEFAULT 0,
	ErrorLimit decimal(15, 6) NOT NULL CONSTRAINT DF_Metrics_ErrorLimit DEFAULT 0
GO

ALTER TABLE dbo.Metrics
	DROP COLUMN Unit
GO

ALTER TABLE dbo.Metrics
	DROP DF_Metrics_FormatString, DF_Metrics_WarningLimit, DF_Metrics_ErrorLimit
GO

INSERT [dbo].[Metrics] ([Id], [ParentId], [Key], [Name], [FormatString], [WarningLimit], [ErrorLimit])
VALUES
(N'69b5f142-534e-4310-a270-0620bc39d925', NULL, N'Energy', N'Energy Score', N'p0', CAST(0.800000 AS Decimal(15, 6)), CAST(0.600000 AS Decimal(15, 6))),
(N'902a8ce2-07c1-4408-ad14-08bac8f4dc6d', N'69b5f142-534e-4310-a270-0620bc39d925', N'KWhPerM2PerDay', N'kWh/m2/Day', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'de6af23c-4704-47db-a1b9-0f8fe08f382a', N'69b5f142-534e-4310-a270-0620bc39d925', N'WattsPerM2', N'W/m²', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'9782de87-04f0-44cc-9aff-14360507e3f6', NULL, N'Wellness', N'Wellness Score', N'p0', CAST(0.800000 AS Decimal(15, 6)), CAST(0.600000 AS Decimal(15, 6))),
(N'5774a48a-1ff7-4e4e-bdec-1db3c60ba8cd', N'69b5f142-534e-4310-a270-0620bc39d925', N'KWhPerDay', N'kWh/day', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'b298a457-e2ef-4939-a139-1db5890ac120', N'9782de87-04f0-44cc-9aff-14360507e3f6', N'Thermal', N'Thermal Index', N'p0', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'490f33e9-96b9-405c-b791-4ed6c098490b', N'69b5f142-534e-4310-a270-0620bc39d925', N'DollarsPerM2PerDay', N'$/m2/Day', N'c', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'46a43d75-51bc-45d5-bb1c-603e2e376c35', NULL, N'Building', N'Site Score', N'p0', CAST(0.800000 AS Decimal(15, 6)), CAST(0.600000 AS Decimal(15, 6))),
(N'acf9bc2c-bd03-4bd7-ab0f-677d2929848e', N'1556b409-6cdc-4a0d-ac6b-ec8169e63e44', N'DisconnectedPoints', N'DisconnectedPoints', N'n0', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'3f99ab06-35ac-40bb-90b7-7ad94d55685a', N'1556b409-6cdc-4a0d-ac6b-ec8169e63e44', N'ConfiguredPoints', N'ConfiguredPoints', N'n0', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'b4a48d40-d3fe-4d76-99c4-bab86238875d', N'9782de87-04f0-44cc-9aff-14360507e3f6', N'ColdIndex', N'ColdIndex', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'd26920e7-2674-4b95-bdc3-c79da124cfce', N'9782de87-04f0-44cc-9aff-14360507e3f6', N'HotIndex', N'HotIndex', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'10510b93-27b7-4f06-b835-cc861734e735', N'69b5f142-534e-4310-a270-0620bc39d925', N'WattsPerFT2', N'W/ft2', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'fd2666c6-1822-4e77-892b-dd974f1f1a81', N'69b5f142-534e-4310-a270-0620bc39d925', N'TonnesCO2PerDay', N'CO2 MetricTons/Day', N'n2', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6))),
(N'1556b409-6cdc-4a0d-ac6b-ec8169e63e44', NULL, N'Systems', N'Systems Performance Score', N'p0', CAST(0.800000 AS Decimal(15, 6)), CAST(0.600000 AS Decimal(15, 6))),
(N'4527d0e9-d481-4277-9248-f6bfb539a068', N'9782de87-04f0-44cc-9aff-14360507e3f6', N'AssetCount', N'AssetCount', N'n0', CAST(0.000000 AS Decimal(15, 6)), CAST(0.000000 AS Decimal(15, 6)))
GO
