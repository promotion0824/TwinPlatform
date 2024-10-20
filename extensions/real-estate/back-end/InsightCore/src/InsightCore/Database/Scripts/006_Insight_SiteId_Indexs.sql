﻿IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_Insights_SiteId_StatusEquipNameState')
    CREATE INDEX [IX_Insights_SiteId_StatusEquipNameState] ON [InsightDB].[dbo].[Insights] ([SiteId], [Status], [EquipmentId], [Name], [State]) INCLUDE ([Priority])