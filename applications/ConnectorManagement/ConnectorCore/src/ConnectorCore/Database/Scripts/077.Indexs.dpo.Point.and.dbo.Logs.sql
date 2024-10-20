
IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_Point_SiteId_EntityId')
BEGIN
CREATE INDEX [IX_Point_SiteId_EntityId] ON [ConnectorCoreDB].[dbo].[Point] ([SiteId],[EntityId])  INCLUDE ([DeviceId])
END

IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_Logs_EndTime')
BEGIN
CREATE INDEX [IX_Logs_EndTime] ON [ConnectorCoreDB].[dbo].[Logs] ([EndTime]) INCLUDE ([ConnectorId], [PointCount], [ErrorCount])
END

IF NOT EXISTS (SELECT name FROM sysindexes WHERE name = 'IX_Logs_StartTime')
BEGIN
CREATE INDEX [IX_Logs_StartTime] ON [ConnectorCoreDB].[dbo].[Logs] ([StartTime]) INCLUDE ([ConnectorId], [Source])
END