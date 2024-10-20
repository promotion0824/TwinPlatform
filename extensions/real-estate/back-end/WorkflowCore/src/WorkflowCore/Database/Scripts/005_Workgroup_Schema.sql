/****** Object:  Table [dbo].[WF_Workgroups]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_Workgroups](
    [Id] [uniqueidentifier] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
CONSTRAINT [PK_WF_Workgroups] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[WF_WorkgroupMembers]    ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WF_WorkgroupMembers](
	[WorkgroupId] [uniqueidentifier] NOT NULL,
	[MemberId] [uniqueidentifier] NOT NULL,
 CONSTRAINT [PK_WF_WorkgroupMembers] PRIMARY KEY CLUSTERED 
(
	[WorkgroupId] ASC,
	[MemberId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[WF_WorkgroupMembers]  WITH CHECK ADD  CONSTRAINT [FK_WF_WorkgroupMembers_WF_Workgroups_WorkgroupId] FOREIGN KEY([WorkgroupId])
REFERENCES [dbo].[WF_Workgroups] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[WF_WorkgroupMembers] CHECK CONSTRAINT [FK_WF_WorkgroupMembers_WF_Workgroups_WorkgroupId]
GO
