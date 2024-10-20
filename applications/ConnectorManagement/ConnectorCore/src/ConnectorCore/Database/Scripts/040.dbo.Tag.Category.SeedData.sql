delete from [dbo].[TagCategory]
GO
delete from [dbo].[Category]
GO

INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '5BE94935-8CD2-4477-9CE5-0727C192A6FB','Air Handling Unit',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Air Handling Unit');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '9B61DF8E-0611-4ED4-8FE8-FB0FE9DA1129','Boiler',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Boiler');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '3379EB7A-E389-466A-8535-2DA674E88E51','Chilled Beams',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Chilled Beams');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '78E58279-4221-4C5C-9396-8268AA57E689','Chilled Water System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Chilled Water System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '69AE6EA3-E3B6-4C3D-95CE-DA0C64651916','Chiller',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Chiller');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'C2036850-4F1C-45F7-AE73-1204C4AEDB91','Cooling Tower',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Cooling Tower');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '1F679FCC-FCBF-445A-AB30-AA3BD2F256F0','Electrical Equipment',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Electrical Equipment');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '0428B7F1-7D07-43EF-9E36-658A117BCD85','Fan Coil Unit',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Fan Coil Unit');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '9CE8B387-2D45-4C3E-862A-7E33B3A223CD','Fans Ventilation',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Fans Ventilation');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'DC4D9EAC-8547-47C8-A301-DA01589E62E0','Fire System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Fire System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'C6590AAB-085B-4A44-B91A-3309D4E7D377','Gas Turbine',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Gas Turbine');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '7D53B8FF-848A-4CCB-84C3-5DC34E45E994','Heat Exchanger',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Heat Exchanger');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '48B6A914-5E4D-40E0-B958-7756F55EA3C5','Heating Hot Water System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Heating Hot Water System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '5BD5E36A-5EA3-4888-8B5D-9C39D08467A2','Hydraulic Equipment',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Hydraulic Equipment');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '3D3D48DD-6470-44C2-B1B1-FE103ECFA862','Lighting System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Lighting System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'A016E8D1-1B3F-48EB-959B-5911B2C420CB','Metering Electrical',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Metering Electrical');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'B61229B6-55B6-42D4-9B96-41D9DDDC5DEA','Packaged Air Conditioning Unit',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Packaged Air Conditioning Unit');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '82E05A67-D393-4753-BAD8-F9D1048A12EE','People Counting and Traffic',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'People Counting and Traffic');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'E6BAA43D-7B95-40D0-98BB-FB5B4F43E8F2','Pumps',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Pumps');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'ABFBEE2B-EB03-4125-ADFB-A8ED6D8CCE8F','Radiant Equipment',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Radiant Equipment');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '80937C39-6E08-4ED0-9E9D-FFC84CAD8C11','Return Air System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Return Air System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '3E8071CA-2969-4111-BFAC-622EE913A82B','Security System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Security System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '13D8B681-8AB8-4609-A42C-E1745A422237','Shading System',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Shading System');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'D95C4C7D-996A-4781-AD77-94228B6C7549','Variable Air Volume',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Variable Air Volume');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'D295693D-2378-4263-83BF-12C8DF273429','Vertical Transport',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Vertical Transport');
GO
INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select 'B36B4F47-6E87-41EA-B371-757C24EC745E','Weather Station',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'Weather Station');
GO



INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '49FFE79F-80B7-4DFE-AA0F-E3424D021570','equip',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'equip' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '3C4A372B-2008-4D0B-9FDD-4E0547F4E308','hvac',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'hvac' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D6B86C8E-F53A-44B0-B5AE-CB27CDBE4692','elec',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'elec' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '28F9E7D7-9822-4E21-9BD2-CF169AEA738D','fireSystem',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'fireSystem' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'E9B29EA0-68AC-430F-A4DA-B5C2C11A8201','gas',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'gas' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '4FC8D3B8-EC20-4105-8EEA-7DD9B2DDBB6D','hotWaterPlant',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'hotWaterPlant' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '508DCEFA-431C-4BAC-93AD-D3350087FB1C','hyd',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'hyd' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '1D7BB162-2A78-4457-87A7-5D1D868B0BFD','lightsGroup',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'lightsGroup' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '274454A5-9578-449F-96CF-22C4D1CF1C48','pplCount',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'pplCount' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '8FD6C829-72EE-4057-9CAE-C3CE21C55338','security',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'security' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '72B903D5-D781-4E59-A020-25C50D752360','blindsGroup',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'blindsGroup' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'AD6A15F1-437B-41F0-ADDD-06932ACEA7BF','verticalTransport',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'verticalTransport' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'A432147A-3F25-464D-BED7-63E6E112A624','weather',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'weather' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '7F6CF8C6-C3E5-43AC-ACA3-F9327DE6975E','ahu',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'ahu' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'B8AC71F1-C854-4E9C-9648-22AAC1A94604','boiler',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'boiler' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'BBB64754-FD1E-42D8-9410-4048FB47040A','chilledBeam',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'chilledBeam' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D817561A-3564-4821-81FF-088258C34BDD','chilledWaterPlant',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'chilledWaterPlant' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D97C7DFC-75D8-4779-9140-7065E91EC3BF','chiller',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'chiller' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D39F3DC1-57B4-4834-A3C4-282E43F9F4AB','coolingTower',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'coolingTower' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '812D035B-FADA-441E-972A-7814A88E9975','fcu',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'fcu' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'E3AD353B-32D3-44B0-806E-CB1AA22D38EE','fan',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'fan' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'EC3E1BDF-436E-403C-8B59-2D0241FA67F9','turbine',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'turbine' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '56543AA3-C8D4-44F1-B483-014020F65302','heatExchanger',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'heatExchanger' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '595B6505-F655-4933-99A9-0D6FF6B63D71','meter',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'meter' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D0683799-5FB4-4727-ACCF-7094307F30C2','dxCool',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'dxCool' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '13658B5C-5522-4FED-A135-12550DE67322','pump',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'pump' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '26F44B87-BACA-4A22-9105-C1974824636C','radiantEquip',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'radiantEquip' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'D9531D13-CDF7-4A0C-AA57-F457D679AB86','returnAirSystem',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'returnAirSystem' and [ClientId] is null);
GO
INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select '36B2B581-1422-4019-BA44-68210B00B6BD','vav',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'vav' and [ClientId] is null);
GO

insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Air Handling Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Air Handling Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'ahu') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Air Handling Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Boiler') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Boiler') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'boiler') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Boiler') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Beams') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Beams') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'chilledBeam') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Beams') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Water System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Water System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'chilledWaterPlant') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chilled Water System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chiller') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chiller') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'chiller') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Chiller') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Cooling Tower') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Cooling Tower') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'coolingTower') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Cooling Tower') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Electrical Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'elec') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Electrical Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fan Coil Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fan Coil Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'fcu') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fan Coil Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fans Ventilation') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fans Ventilation') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'fan') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fans Ventilation') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fire System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'fireSystem') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Fire System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Gas Turbine') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'gas') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Gas Turbine') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'turbine') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Gas Turbine') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Heat Exchanger') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Heat Exchanger') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'heatExchanger') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Heat Exchanger') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Heating Hot Water System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hotWaterPlant') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Heating Hot Water System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Hydraulic Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hyd') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Hydraulic Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Lighting System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'lightsGroup') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Lighting System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Metering Electrical') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'elec') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Metering Electrical') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'meter') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Metering Electrical') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Packaged Air Conditioning Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Packaged Air Conditioning Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'dxCool') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Packaged Air Conditioning Unit') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'People Counting and Traffic') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'pplCount') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'People Counting and Traffic') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Pumps') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Pumps') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'pump') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Pumps') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Radiant Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Radiant Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'radiantEquip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Radiant Equipment') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Return Air System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Return Air System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'returnAirSystem') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Return Air System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Security System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'security') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Security System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Shading System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'blindsGroup') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Shading System') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Variable Air Volume') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Variable Air Volume') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'vav') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Variable Air Volume') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Vertical Transport') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'verticalTransport') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Vertical Transport') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Weather Station') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'weather') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'Weather Station') as [CategoryId] ;
GO
