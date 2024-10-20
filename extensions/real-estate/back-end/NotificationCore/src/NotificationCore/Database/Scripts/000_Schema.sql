/****** Object:  Table [dbo].[NotificationTrigger] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggers] (
    Id UNIQUEIDENTIFIER  NOT NULL,
    [Type] INT NOT NULL,
    Source INT NOT NULL,
    Focus INT NOT NULL,
    LocationJson NVARCHAR(MAX) NOT NULL,
    IsEnabled BIT NOT NULL,
    CanpUserDisableNotification BIT NOT NULL DEFAULT 0,
    CreatedBy UNIQUEIDENTIFIER NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy UNIQUEIDENTIFIER NULL,
    UpdatedDate DATETIME NULL,
   CONSTRAINT [PK_NotificationTrigger] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

/****** Object:  Table [dbo].[WorkgroupSubscription] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WorkgroupSubscriptions] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    WorkgroupId UNIQUEIDENTIFIER NOT NULL,   
    CONSTRAINT [PK_WorkgroupSubscriptions] PRIMARY KEY  (NotificationTriggerId,WorkgroupId),
    CONSTRAINT [FK_WorkgroupSubscriptions_NotificationTriggerId] FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationSubscriptionOverrides] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationSubscriptionOverrides] (
    UserId UNIQUEIDENTIFIER NOT NULL,
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    IsEnabled BIT NOT NULL,
    CONSTRAINT PK_NotificationSubscriptionOverrides PRIMARY KEY (UserId, NotificationTriggerId),
    CONSTRAINT FK_NotificationSubscriptionOverrides_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationTriggerTwins] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggerTwins] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    TwinId NVARCHAR(250) NOT NULL,
    CONSTRAINT PK_NotificationTriggerTwins PRIMARY KEY (TwinId, NotificationTriggerId),
    CONSTRAINT FK_NotificationTriggerTwins_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationTriggerSkills] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggerSkills] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    SkillId NVARCHAR(450) NOT NULL,
    CONSTRAINT PK_NotificationTriggerSkills PRIMARY KEY (SkillId, NotificationTriggerId),
    CONSTRAINT FK_NotificationTriggerSkills_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationTriggerPriorities] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggerPriorities] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    PriorityId INT NOT NULL,
    CONSTRAINT PK_NotificationTriggerPriorities PRIMARY KEY (PriorityId, NotificationTriggerId),
    CONSTRAINT FK_NotificationTriggerPriorities_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationTriggerChannels] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggerChannels] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    Channel INT NOT NULL,
    CONSTRAINT PK_NotificationTriggerChannels PRIMARY KEY (NotificationTriggerId, Channel),
    CONSTRAINT FK_NotificationTriggerChannels_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
/****** Object:  Table [dbo].[NotificationTriggerCategories] ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NotificationTriggerCategories] (
    NotificationTriggerId UNIQUEIDENTIFIER NOT NULL,
    CategoryId INT NOT NULL,
    CONSTRAINT PK_NotificationTriggerCategories PRIMARY KEY (NotificationTriggerId, CategoryId),
    CONSTRAINT FK_NotificationTriggerCategories_NotificationTriggerId FOREIGN KEY (NotificationTriggerId) REFERENCES NotificationTriggers(Id)
)
GO
