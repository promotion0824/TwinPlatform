INSERT INTO [dbo].[Category] ([Id],[Name],[ClientId],[SiteId],[ParentId])
select '504A89D3-A8A4-4119-923C-876CDED9AF32','AC Units',NULL,NULL,NULL
where not exists (select 1 from [dbo].[Category] where [Name] = 'AC Units');
GO

INSERT INTO [dbo].[Tag] ([Id],[Name],[Description],[ClientId])
select 'F30B9471-A873-4BC5-AA0A-045955DE6192','ac',NULL,NULL
where not exists(select 1 from [dbo].[Tag] where [Name] = 'ac' and [ClientId] is null);
GO

insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'equip') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'AC Units') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'hvac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'AC Units') as [CategoryId] ;
GO
insert into [dbo].[TagCategory](TagId, CategoryId)
select (
select top 1 [Id] from [dbo].[Tag] where [Name] = 'ac') as [TagId],
(select top 1 [Id] from [dbo].[Category] where [Name] = 'AC Units') as [CategoryId] ;
GO