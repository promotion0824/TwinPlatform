
IF NOT EXISTS(Select id from NotificationTriggers where [Type]=1
                                                        And [Source]=1
                                                        And [Focus]=4
                                                        And [ChannelJson]='[1]'
                                                        And [PriorityJson]='[1]'
                                                        And [IsEnabled]=0
                                                        And [CanUserDisableNotification]=0
                                                        And IsDefault=1)
BEGIN

DECLARE @NotificationTriggerId UNIQUEIDENTIFIER;
SET @NotificationTriggerId = NEWID();

INSERT INTO NotificationTriggers  ([Id],[Type],[Source],[Focus],[ChannelJson],[PriorityJson],[IsEnabled],[CanUserDisableNotification],
[CreatedBy],[CreatedDate], [IsDefault])  Values (@NotificationTriggerId,1,1,4,'[1]','[1]',0,0,NewId(),GETUTCDATE(),1)

Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,1)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,2)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,3)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,4)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,10)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,11)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,12)
Insert into NotificationTriggerSkillCategories (NotificationTriggerId, CategoryId) Values (@NotificationTriggerId,16)
END




