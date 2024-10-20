INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '9D9B0D0E-29A0-4CC3-8FC6-68CA3E4B553B','Air Quality System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Air Quality System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '8D997A12-21EF-4270-A1EA-A7CB154A6939','Chilled Beams',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Chilled Beams');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '8C8AE1FB-6AE2-4BC1-A668-03250BF059E6','Computer Room AC Unit',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Computer Room AC Unit');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '4DB428AE-C196-4B48-A4A6-AD4EA2C6E17D','Condenser Water System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Condenser Water System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'D7ED8A38-2F82-4193-BC7F-97E613730695','Fire System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Fire System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'D36BA424-01A5-4070-8539-3ED6CDD9E4F1','Metering Gas',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Gas');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '9BC2B39E-26B7-4957-AD0E-3D3EB81D5B9A','Gas Turbine',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Gas Turbine');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '2F2E5D38-DB37-42B1-9D72-1EFD627A6946','Metering Electrical',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Electrical');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'D46F60FC-28E2-4329-8539-404D20B47EF6','Metering Thermal',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Thermal');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'E90685E3-2C23-4798-93EA-761AD02DA863','Metering Water',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Water');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'B8A18EC7-F461-4AD7-BBF4-1DC905696C86','Metering Steam',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Steam');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '1E994467-A8BA-4788-B79E-A9CE4128FDE6','Packaged Air Conditioning Unit',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Packaged Air Conditioning Unit');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'C8EAAB9F-B5DA-4BFE-A7E7-95816861D057','Radiant Equipment',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Radiant Equipment');
GO

INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'airQualitySystem',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'airQualitySystem' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'chilledBeam',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'chilledBeam' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'condenserWaterSystem',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'condenserWaterSystem' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'fire',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'fire' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'fireSystem',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'fireSystem' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'meter',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'meter' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'gas',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'gas' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'gasTurbine',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'gasTurbine' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'elec',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'elec' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'thermal',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'thermal' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'water',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'water' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'steam',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'steam' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'pac',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'pac' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'dxCool',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'dxCool' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'radiantEquip',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'radiantEquip' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select NEWID(),'crac',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'crac' and [ClientId] is null);
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Air Quality System'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'airQualitySystem') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Chilled Beams'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'chilledBeam') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Condenser Water System'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'condenserWaterSystem') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Fire System'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'fire') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'fireSystem') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Computer Room AC Unit'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'crac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Metering Gas'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'gas') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Gas Turbine'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'gasTurbine') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Metering Electrical'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'elec') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Metering Thermal'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'thermal') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Metering Water'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'water') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Metering Steam'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'steam') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Packaged Air Conditioning Unit'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'pac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'dxCool') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO

DECLARE @CategoryName VARCHAR(100)
SET @CategoryName = 'Radiant Equipment'
delete from [dbo].[TagCategory] where CategoryId in (select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName)
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'radiantEquip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = @CategoryName) as [CategoryId] ;
GO