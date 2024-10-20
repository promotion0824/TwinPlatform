-- Allow to insert categories without siteId
ALTER TABLE [WF_TicketCategory] ALTER COLUMN [SiteId] UNIQUEIDENTIFIER NULL;

-- Unassign existing ticket category and remove manually inserted records
UPDATE [dbo].[WF_Ticket]
SET CategoryId = NULL
WHERE CategoryId IS NOT NULL;

DELETE FROM [dbo].[WF_TicketCategory];

-- Insert default Ticket Categories for all buildings
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('78532F12-D567-40DA-BC29-5447EBD40FC7', NULL, 'Building Access');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('70353B4A-6502-458F-89FE-0ED96C3A0431', NULL, 'Building Management');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('18D29B13-3EA2-42C7-9DDC-BD782ADEB0D2', NULL, 'Cleaning');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('11638ED9-7C6A-4C0C-AC9F-2479FCB90C17', NULL, 'Conferences');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('47C145B5-1F09-4F0F-8A70-8C9181360CCF', NULL, 'Construction Management');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('EB6866CB-BFF3-4CA6-ADFB-101951A7F591', NULL, 'Contractor');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('271AE18E-F09D-40DE-9A48-0CD432FC24E8', NULL, 'Electrical');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('AD3F15F3-CD55-4A8E-9846-ECB3C2A4369F', NULL, 'Elevator');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('2D808044-7EB7-4706-9856-C82E6D58AAA9', NULL, 'Events');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('17C5A036-F160-4B03-A355-2630CE04ABF0', NULL, 'Facilities');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('82E75EBF-A5F3-47FC-94FD-94BABAE2F6EE', NULL, 'General');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('CDA67D9F-6AE5-40FD-878B-013830C4C208', NULL, 'HVAC');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('2B8CE6F5-A534-4F9B-A0FB-B73DB2BD71E8', NULL, 'Installation');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('BEC6E078-C0DA-426E-A11C-095483A8E98B', NULL, 'Key Management');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('84C2E699-C879-4EEF-B42C-33D9C1C9E74A', NULL, 'Lighting');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('53AFD4F9-63B9-4518-851F-2EF0B413EB8F', NULL, 'Maintenance');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('99223087-2ADF-4C78-BC04-B7ED5B60C953', NULL, 'Parking');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('2FCCF042-4996-466A-9F86-B5CC6A6DB3AE', NULL, 'Pest Control');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('207E067D-0097-4CB0-82A3-0899F05139D2', NULL, 'Plumbing');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('0F162127-2941-44BF-87F6-13F0DC5B39EF', NULL, 'Radio');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('0E1E5650-8BA6-4B1D-94AB-FCAF3AC2E1B0', NULL, 'Removals');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('09FF7FD9-DC4D-4C9A-9DFC-AAC2C6FF7E2E', NULL, 'Security');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('B7FFD98D-25C5-4C0E-8218-A9DF2E628E09', NULL, 'Service Request');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('998CA5D2-A85F-4516-842A-6CC1458C0E05', NULL, 'Signage');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('10CE9400-7412-47CE-9F57-9C28CC8CAB65', NULL, 'Training');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('1DD3DE9B-31DD-4702-B6C6-99F16DF4C99A', NULL, 'Transportation');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('16E9805A-C334-4080-A6DC-A451BB66E5DF', NULL, 'Unspecified');
INSERT INTO [dbo].[WF_TicketCategory] ([Id], [SiteId], [Name]) VALUES ('810F2BA6-1044-4FF8-B157-64A4BD2307E9', NULL, 'Utilities');