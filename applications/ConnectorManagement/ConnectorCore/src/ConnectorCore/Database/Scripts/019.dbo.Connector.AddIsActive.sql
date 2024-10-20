alter table [Connector] add [IsActive] bit;
GO
update [Connector] set [IsActive] = 1;
GO
alter table [Connector] alter column [IsActive] bit not null;
GO