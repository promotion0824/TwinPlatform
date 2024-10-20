using Authorization.TwinPlatform.Common.Abstracts;
using Authorization.TwinPlatform.Common.Model;
using AutoFixture;
using FluentAssertions;
using Moq;
using NotificationCore.Entities;
using NotificationCore.Models;
using NotificationCore.Repositories;
using NotificationCore.Services;
using NotificationCore.Test.Infrastructure;
using NotificationCore.TriggerFilterRules;
using Willow.Batch;
using Xunit;
using Xunit.Abstractions;

namespace NotificationCore.Test.Services.NotificationServiceTest;

public class CreateNotificationTests : BaseInMemoryTest
{
    Guid workgroup1 = Guid.Parse("11487CFF-04A6-444B-B4DD-A195C655E87F");
    Guid workgroup2 = Guid.Parse("22487CFF-04A6-444B-B4DD-A195C655E87E");
    Guid triggerId1 = Guid.Parse("33487CFF-04A6-444B-B4DD-A195C655E87A");
    Guid triggerId2 = Guid.Parse("44487CFF-04A6-444B-B4DD-A195C655E87A");
    Guid triggerId3 = Guid.Parse("55487CFF-04A6-444B-B4DD-A195C655E87A");
    public CreateNotificationTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task NotificationMessageMatchOneFilter_CreateNotification_ShouldCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();

        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }



        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items =  workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var triggers = GetNotificationTriggerEntities(workgroupList);
            db.AddRange(triggers);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 1)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 1)
                                      .With(x => x.Locations, ["Location1"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(1);
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(new List<Guid> { triggerId1 });
            var expectedNotificationUsers = (from user in workgroupList.First(x => x.Id == workgroup1).Users
                                             select new NotificationUserEntity
                                             {
                                                 NotificationId = notificationsEntities[0].Id,
                                                 UserId = user.Id,
                                                 State = NotificationUserState.New
                                             })
                                                                                 .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }

    [Fact]
    public async Task NotificationUnMatchedPriority_CreateNotification_ShouldCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }


        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var triggers = GetNotificationTriggerEntities(workgroupList);
            db.AddRange(triggers);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 1)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 3)
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(0);


            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().HaveCount(0);
        }
    }

    [Fact]
    public async Task NotificationMatchMultiTriggers_CreateNotification_ShouldCreateOneNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = userId1 }, new UserModel { Id = userId2 }])
                .Create();
            workgroupList.Add(workgroup);
        }

        // change the second workgroup users with overlapped user to test userId uniqueness
        workgroupList[1].Users = new List<UserModel> { new UserModel { Id = userId2 }, new UserModel { Id = userId3 } };


        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var triggers = GetNotificationTriggerEntities(workgroupList);
            db.AddRange(triggers);
            db.SaveChanges();


            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 1)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();


            notificationsEntities.Should().HaveCount(1);
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(new List<Guid> { triggerId1, triggerId2 });
            // users from workgroup1 and workgroup2
            var expectedNotificationUsers = (from user in workgroupList.First(x => x.Id == workgroup1).Users
                                             select new NotificationUserEntity
                                             {
                                                 NotificationId = notificationsEntities[0].Id,
                                                 UserId = user.Id,
                                                 State = NotificationUserState.New
                                             })
                                                                                 .ToList();
            expectedNotificationUsers.AddRange(from user in workgroupList.First(x => x.Id == workgroup2).Users
                                               select new NotificationUserEntity
                                               {
                                                   NotificationId = notificationsEntities[0].Id,
                                                   UserId = user.Id,
                                                   State = NotificationUserState.New
                                               });
            expectedNotificationUsers = expectedNotificationUsers.DistinctBy(x => x.UserId).ToList();

            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().HaveCount(3);
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }

    [Fact]
    public async Task NotificationTriggerWithUserOverride_CreateNotification_ShouldNotCreateNotificationForThisUser()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = userId1 }, new UserModel { Id = userId2 }])
                .Create();
            workgroupList.Add(workgroup);
        }

        // change the second workgroup users with overlapped user to test userId uniqueness
        workgroupList[1].Users = new List<UserModel> { new UserModel { Id = userId2 }, new UserModel { Id = userId3 } };


        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });


        var userOverride = new NotificationSubscriptionOverrideEntity
        {
            NotificationTriggerId = triggerId2,
            UserId = userId3,
            IsEnabled = true,
            IsMuted = true
        };

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();

            var triggers = GetNotificationTriggerEntities(workgroupList);
            db.Add(userOverride);
            db.AddRange(triggers);
            db.SaveChanges();


            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 1)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();


            notificationsEntities.Should().HaveCount(1);
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(new List<Guid> { triggerId1, triggerId2 });


            var expectedNotificationUsers = (from user in workgroupList.First(x => x.Id == workgroup1).Users
                                             select new NotificationUserEntity
                                             {
                                                 NotificationId = notificationsEntities[0].Id,
                                                 UserId = user.Id,
                                                 State = NotificationUserState.New
                                             })
                                                                                 .ToList();
            expectedNotificationUsers.AddRange(from user in workgroupList.First(x => x.Id == workgroup2).Users
                                               select new NotificationUserEntity
                                               {
                                                   NotificationId = notificationsEntities[0].Id,
                                                   UserId = user.Id,
                                                   State = NotificationUserState.New
                                               });

            // users from workgroup1 and workgroup2 except userId3 (overridden user subscription)
            expectedNotificationUsers = expectedNotificationUsers.Where(x => x.UserId != userId3).DistinctBy(x => x.UserId).ToList();

            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().HaveCount(2);
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }

    [Fact]
    public async Task NotificationTriggerWithUserOverride_CreateNotification_ShouldNotCreateNotificationForThisMutedUser()
    {
        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var workGroupEntites = new List<WorkgroupSubscriptionEntity> {
            new (){ NotificationTriggerId = triggerId1, WorkgroupId = workgroup1 }};

        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var uId = Guid.NewGuid();

        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }
        var wgUsers = workgroupList[0].Users.ToList();

        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {
            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var wgtrigger1 = new NotificationTriggerEntity
            {
                Id = triggerId1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Workgroup,
                CreatedBy = uId,
                Twins = new List<NotificationTriggerTwinEntity> { new() { NotificationTriggerId = triggerId1, TwinId = "Twin1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = triggerId1, Id = "Location1" } },
                WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == triggerId1).ToList(),
                NotificationSubscriptionOverrides = new List<NotificationSubscriptionOverrideEntity> { new NotificationSubscriptionOverrideEntity { UserId = uId, NotificationTriggerId = triggerId1, IsEnabled = true, IsMuted = false } }

            };
            var trigger1 = new NotificationTriggerEntity
            {
                Id = t1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Workgroup,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new() { NotificationTriggerId = t1, TwinId = "Twin1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = t1, Id = "Location1" } ,
                    new () { NotificationTriggerId = t1, Id = "Location2" }
                },
                WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == t1).ToList(),
            };

            db.Add(wgtrigger1);
            db.Add(trigger1);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 5)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1", "Loc10", "LocA"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(1);
            var expectedTriggerIds = new List<Guid> { t1, triggerId1 };
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedTriggerIds);
            var notificationsUsers = db.NotificationsUsers.ToList();
            var usersInDb = notificationsUsers.Select(x => x.UserId).ToList();
            usersInDb.Should().BeEquivalentTo(wgUsers.Select(x => x.Id).ToList());
        }
    }


    [Theory]
    [MemberData(nameof(MemberData))]
    public async Task NotificationTriggerExists_CreateNotification_ShouldCreateNotifications(List<NotificationTriggerEntity> triggerEntities,
                                                                                            NotificationMessage notificationMessage,
                                                                                            List<GroupModel> workGroupResponse,
                                                                                            ExpectedResult expectedResult)
    {

        

        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workGroupResponse.ToArray() });




        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            db.AddRange(triggerEntities);
            db.SaveChanges();


            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

           
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();


            notificationsEntities.Should().HaveCount(1);
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedResult.triggerIds);
            notificationsUsers.Should().HaveCount(expectedResult.userIds.Count);
            notificationsUsers.Select(x => x.UserId).Should().BeEquivalentTo(expectedResult.userIds);
        }
    }


    [Fact]
    public async Task TriggerWithoutLocationExists_CreateNotification_ShouldCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var t3 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }



        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var trigger1 = new NotificationTriggerEntity
            {
                Id = t1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new () { NotificationTriggerId = t1, TwinId = "Twin1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = t1, Id = "Location1" } ,
                    new () { NotificationTriggerId = t1, Id = "Location2" }
                }
            };

            var trigger2 = new NotificationTriggerEntity
            {
                Id = t2,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Skill,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Skills = new List<NotificationTriggerSkillEntity> { new() { NotificationTriggerId = t2, SkillId = "Skill1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };

            var trigger3 = new NotificationTriggerEntity
            {
                Id = t3,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new () { NotificationTriggerId = t3, TwinId = "Twin2" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };
            db.Add(trigger1);
            db.Add(trigger2);
            db.Add(trigger3);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 5)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1", "Loc10", "LocA"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(1);
            var expectedTriggerIds = new List<Guid> { t1, t2 };
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedTriggerIds);
            var expectedNotificationUsers = (from userId in db.NotificationTriggers.Where(x=> expectedTriggerIds.Contains(x.Id)).Select(x => x.CreatedBy)
                                              select new NotificationUserEntity
                                              {
                                                  NotificationId = notificationsEntities[0].Id,
                                                  UserId = userId,
                                                  State = NotificationUserState.New
                                              })
                                                                                 .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }

    [Fact]
    public async Task SkillFoucsTriggerExits_CreateNotification_ShouldCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var t3 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }



        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var trigger1 = new NotificationTriggerEntity
            {
                Id = t1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Skill,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Skills = new List<NotificationTriggerSkillEntity> { new() { NotificationTriggerId = t1, SkillId = "Skill1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = t1, Id = "Location1" } ,
                    new () { NotificationTriggerId = t1, Id = "Location2" }
                }
            };

            var trigger2 = new NotificationTriggerEntity
            {
                Id = t2,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Skill,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Skills = new List<NotificationTriggerSkillEntity> { new() { NotificationTriggerId = t2, SkillId = "Skill2" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };

            var trigger3 = new NotificationTriggerEntity
            {
                Id = t3,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new() { NotificationTriggerId = t3, TwinId = "Twin2" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };
            db.Add(trigger1);
            db.Add(trigger2);
            db.Add(trigger3);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 5)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1", "Loc10", "LocA"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();


            notificationsEntities.Should().HaveCount(1);
            notificationsEntities.First().PropertyBagJson.Should().Contain("\"skillId\":\"Skill1\"");
            notificationsEntities.First().PropertyBagJson.Should().Contain($"\"sourceId\":\"{notificationMessage.SourceId}\"");
            notificationsEntities.First().Source.Should().Be(NotificationSource.Insight);

            var expectedTriggerIds = new List<Guid> { t1 };
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedTriggerIds);
            var expectedNotificationUsers = (from userId in db.NotificationTriggers.Where(x => expectedTriggerIds.Contains(x.Id)).Select(x => x.CreatedBy)
                                             select new NotificationUserEntity
                                             {
                                                 NotificationId = notificationsEntities[0].Id,
                                                 UserId = userId,
                                                 State = NotificationUserState.New
                                             })
                                                                                 .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }
    public static IEnumerable<object[]> MemberData()
    {
        // set triggers ids
        var triggerId1 = Guid.NewGuid();
        var triggerId2 = Guid.NewGuid();
        var triggerId3 = Guid.NewGuid();
        var triggerId4 = Guid.NewGuid();
        var triggerId5 = Guid.NewGuid();
        var triggerId6 = Guid.NewGuid();
        var triggerId7 = Guid.NewGuid();
        var triggerId8 = Guid.NewGuid();
        // set user ids
        var userId1 = Guid.NewGuid();
        var userId2 = Guid.NewGuid();
        var userId3 = Guid.NewGuid();

        // set work groups
        var workgroupId1 = Guid.NewGuid();
        var workgroupId2 = Guid.NewGuid();
        var workGroupResponse = new List<GroupModel> {
            new GroupModel{ Id = workgroupId1, Users = [new UserModel { Id = userId1 }, new UserModel { Id = userId2 }] },
            new GroupModel{ Id = workgroupId2, Users = [new UserModel { Id = userId1 }, new UserModel { Id = userId3 }] }

        };
        var workGroupEntites = new List<WorkgroupSubscriptionEntity> {
            new (){ NotificationTriggerId = triggerId1, WorkgroupId = workgroupId1 },
            new (){ NotificationTriggerId = triggerId2, WorkgroupId = workgroupId2 },
            new (){ NotificationTriggerId = triggerId3, WorkgroupId = workgroupId1 },
            new (){ NotificationTriggerId = triggerId4, WorkgroupId = workgroupId2 },
        };



       

       // set notification message
        var notificationMessage1 = new NotificationMessage
        {
            Title = "Notification1",
            TwinId = "Twin1",
            SkillId = "Skill10",
            SkillCategoryId = 1,
            ModelId = "TwinCategory2",
            Source = NotificationSource.Insight,
            Priority = 1,
            Locations = ["Location1"]
        };

        var notificationMessage2 = new NotificationMessage
        {
            Title = "Notification2",
            TwinId = "Twin2",
            SkillId = "Skill1",
            SkillCategoryId = 20,
            ModelId = "TwinCategory20",
            Source = NotificationSource.Insight,
            Priority = 2
        };

        var notificationMessage3 = new NotificationMessage
        {
            Title = "Notification3",
            TwinId = "Twin1",
            SkillId = "Skill2",
            SkillCategoryId = 20,
            ModelId = "TwinCategory1",
            Source = NotificationSource.Insight,
            Priority = 2
        };

        var notificationMessage4 = new NotificationMessage
        {
            Title = "Notification4",
            TwinId = "Twin2",
            SkillId = "Skill20",
            SkillCategoryId = 1,
            ModelId = "TwinCategory2",
            Source = NotificationSource.Insight,
            Priority = 2
        };

        var notificationMessage5 = new NotificationMessage
        {
            Title = "Notification5",
            TwinId = "Twin1",
            SkillId = "Skill20",
            SkillCategoryId = 2,
            ModelId = "TwinCategory2",
            Source = NotificationSource.Insight,
            Priority = 2,
            Locations = ["Location1", "Location2"]
        };
        var twinTriggerList = new List<NotificationTriggerTwinEntity> {
            new (){ NotificationTriggerId = triggerId1, TwinId = "Twin1" },
            new (){ NotificationTriggerId = triggerId1, TwinId = "TwinA-B" },
            new (){ NotificationTriggerId = triggerId2, TwinId = "Twin2" }
        };

        var twinCategoryTrigger = new List<NotificationTriggerTwinCategoryEntity> {
            new (){ NotificationTriggerId = triggerId3, CategoryId = "TwinCategory1" },
            new (){ NotificationTriggerId = triggerId3, CategoryId = "TwinCategory-AB" },
            new (){ NotificationTriggerId = triggerId4, CategoryId = "TwinCategory2" }
        };

        var skillTriggerList = new List<NotificationTriggerSkillEntity> {
            new (){ NotificationTriggerId = triggerId5, SkillId = "Skill1" },
            new (){ NotificationTriggerId = triggerId5, SkillId = "Skill-A-B" },
            new (){ NotificationTriggerId =triggerId6, SkillId = "Skill2" }
        };

        var skillCategoryTrigger = new List<NotificationTriggerSkillCategoryEntity> {
            new (){ NotificationTriggerId = triggerId7, CategoryId = 1 },
            new (){ NotificationTriggerId = triggerId7, CategoryId = 99 },
            new (){ NotificationTriggerId = triggerId8, CategoryId = 2 }
        };

        var locationTriggerList = new List<LocationEntity> {
            new (){ NotificationTriggerId = triggerId1, Id = "Location1" },
            new (){ NotificationTriggerId = triggerId1, Id = "Location2" },
            new (){ NotificationTriggerId = triggerId4, Id = "Location1" },
            new (){ NotificationTriggerId = triggerId4, Id = "Location2" },
            new (){ NotificationTriggerId = triggerId8, Id = "Location1" },
            new (){ NotificationTriggerId = triggerId8, Id = "Location2" }
        };
        var trigger1 = new NotificationTriggerEntity
        {
            Id = triggerId1,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.Twin,
            IsEnabled = true,
            PriorityJson = new List<int> { 1, 2 },
            Type = NotificationType.Workgroup,
            CreatedBy = Guid.NewGuid(),
            Twins = twinTriggerList.Where(x => x.NotificationTriggerId == triggerId1).ToList(),
            ChannelJson = [NotificationChannel.InApp],
            Locations = locationTriggerList.Where(x => x.NotificationTriggerId == triggerId1).ToList(),
            WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == triggerId1).ToList()
        };
        var trigger2 = new NotificationTriggerEntity
        {
            Id = triggerId2,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.Twin,
            IsEnabled = true,
            PriorityJson = new List<int> {2, 3 },
            Type = NotificationType.Workgroup,
            CreatedBy = Guid.NewGuid(),
            Twins = twinTriggerList.Where(x => x.NotificationTriggerId == triggerId2).ToList(),
            ChannelJson = [NotificationChannel.InApp],
            WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == triggerId2).ToList()
        };

        var trigger3 = new NotificationTriggerEntity
        {
            Id = triggerId3,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.TwinCategory,
            IsEnabled = true,
            PriorityJson = new List<int> { 1, 2 },
            Type = NotificationType.Workgroup,
            CreatedBy = Guid.NewGuid(),
            TwinCategories = twinCategoryTrigger.Where(x => x.NotificationTriggerId == triggerId3).ToList(),
            ChannelJson = [NotificationChannel.InApp],
            WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == triggerId3).ToList()
        };
        var trigger4 = new NotificationTriggerEntity
        {
            Id = triggerId4,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.TwinCategory,
            IsEnabled = true,
            PriorityJson = new List<int> { 1,2 },
            Type = NotificationType.Workgroup,
            CreatedBy = Guid.NewGuid(),
            TwinCategories = twinCategoryTrigger.Where(x => x.NotificationTriggerId == triggerId4).ToList(),
            ChannelJson = [NotificationChannel.InApp],
            Locations = locationTriggerList.Where(x => x.NotificationTriggerId == triggerId4).ToList(),
            WorkgroupSubscriptions = workGroupEntites.Where(x => x.NotificationTriggerId == triggerId4).ToList()

        };

        var trigger5 = new NotificationTriggerEntity
        {
            Id = triggerId5,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.Skill,
            IsEnabled = true,
            PriorityJson = new List<int> { 1, 2 },
            Type = NotificationType.Personal,
            CreatedBy = userId1,
            Skills = skillTriggerList.Where(x => x.NotificationTriggerId == triggerId5).ToList(),
            ChannelJson = [NotificationChannel.InApp]
        };

        var trigger6 = new NotificationTriggerEntity
        {
            Id = triggerId6,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.Skill,
            IsEnabled = true,
            PriorityJson = new List<int> { 2 },
            Type = NotificationType.Personal,
            CreatedBy = userId2,
            Skills = skillTriggerList.Where(x => x.NotificationTriggerId == triggerId6).ToList(),
            ChannelJson = [NotificationChannel.InApp]
        };

        var trigger7 = new NotificationTriggerEntity
        {
            Id = triggerId7,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.SkillCategory,
            IsEnabled = true,
            PriorityJson = new List<int> { 1, 2 },
            Type = NotificationType.Personal,
            CreatedBy = userId3,
            SkillCategories = skillCategoryTrigger.Where(x => x.NotificationTriggerId == triggerId7).ToList(),
            ChannelJson = [NotificationChannel.InApp]
        };

        var trigger8 = new NotificationTriggerEntity
        {
            Id = triggerId8,
            Source = NotificationSource.Insight,
            Focus = NotificationFocus.SkillCategory,
            IsEnabled = true,
            PriorityJson = new List<int> { 2 },
            Type = NotificationType.Personal,
            CreatedBy = userId1,
            SkillCategories = skillCategoryTrigger.Where(x => x.NotificationTriggerId == triggerId8).ToList(),
            ChannelJson = [NotificationChannel.InApp],
            Locations = locationTriggerList.Where(x => x.NotificationTriggerId == triggerId8).ToList()
        };

        return new List<object[]> {
            new object[] {
                 new List<NotificationTriggerEntity> { trigger1, trigger4,trigger8, trigger5 },notificationMessage1,workGroupResponse, new ExpectedResult([trigger1.Id, trigger4.Id],[userId1,userId2, userId3])
            },
            new object[] {
                 new List<NotificationTriggerEntity> { trigger2, trigger5, trigger3,trigger8 },notificationMessage2,workGroupResponse ,new ExpectedResult([trigger2.Id, trigger5.Id],[userId1, userId3])
            },
            new object[] {
                 new List<NotificationTriggerEntity> { trigger1, trigger3, trigger6, trigger4 },notificationMessage3,workGroupResponse , new ExpectedResult([trigger1.Id, trigger3.Id, trigger6.Id],[userId1, userId2])
            },
            new object[] {
                 new List<NotificationTriggerEntity> { trigger2, trigger4, trigger7, trigger1 },notificationMessage4,workGroupResponse , new ExpectedResult([trigger2.Id, trigger4.Id, trigger7.Id],[userId1,userId3])
            },
             new object[] {
                 new List<NotificationTriggerEntity> { trigger1, trigger4, trigger8, trigger1 },notificationMessage5,workGroupResponse , new ExpectedResult([trigger1.Id, trigger4.Id, trigger8.Id],[userId1, userId2, userId3])
            },

        };
    }

    [Fact]
    public async Task TriggerWithoutLocationExists_CreateNotification_MuteShouldNotCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }



        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var trigger1 = new NotificationTriggerEntity
            {
                Id = t1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Personal,
                IsMuted = true,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new() { NotificationTriggerId = t1, TwinId = "Twin1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = t1, Id = "Location1" } ,
                    new () { NotificationTriggerId = t1, Id = "Location2" }
                }
            };

            var trigger2 = new NotificationTriggerEntity
            {
                Id = t2,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Skill,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Skills = new List<NotificationTriggerSkillEntity> { new() { NotificationTriggerId = t2, SkillId = "Skill1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };

            db.Add(trigger1);
            db.Add(trigger2);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 5)
                                      .With(x => x.ModelId, "TwinCategory1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1", "Loc10", "LocA"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(1);
            var expectedTriggerIds = new List<Guid> { t2 };
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedTriggerIds);
            var expectedNotificationUsers = ( from userId in db.NotificationTriggers.Where(x => expectedTriggerIds.Contains(x.Id)).Select(x => x.CreatedBy)
                                              select new NotificationUserEntity
                                              {
                                                  NotificationId = notificationsEntities[0].Id,
                                                  UserId = userId,
                                                  State = NotificationUserState.New
                                              } )
                                                                                 .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }


    [Fact]
    public async Task TriggerWithoutTwinCategoriesExists_CreateNotification_ShouldCreateNotification()
    {

        var workgroupIds = new List<Guid> { workgroup1, workgroup2 };
        var workgroupList = new List<GroupModel>();
        var t1 = Guid.NewGuid();
        var t2 = Guid.NewGuid();
        var t3 = Guid.NewGuid();
        var t4 = Guid.NewGuid();
        foreach (var workgroupId in workgroupIds)
        {
            var workgroup = Fixture.Build<GroupModel>()
                .With(x => x.Id, workgroupId)
                .With(x => x.Users, [new UserModel { Id = Guid.NewGuid() }, new UserModel { Id = Guid.NewGuid() }])
                .Create();
            workgroupList.Add(workgroup);
        }



        var userAuthorizationService = new Mock<IUserAuthorizationService>();
        userAuthorizationService.Setup(x => x.GetApplicationGroupsAsync(It.IsAny<BatchRequestDto>()))
                                .ReturnsAsync(new BatchDto<GroupModel> { Items = workgroupList.ToArray() });

        using (var server = CreateServerFixture(ServerFixtureConfigurations.InMemoryDb))
        using (var client = server.CreateClient(null))
        {

            var db = server.Arrange().CreateDbContext<NotificationDbContext>();
            var trigger1 = new NotificationTriggerEntity
            {
                Id = t1,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.TwinCategory,
                IsEnabled = true,
                PriorityJson = new List<int> { 1, 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                TwinCategories = new List<NotificationTriggerTwinCategoryEntity> { new() { NotificationTriggerId = t1, CategoryId = "TwinModel1" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = new List<LocationEntity> {
                    new () { NotificationTriggerId = t1, Id = "Location1" } ,
                    new () { NotificationTriggerId = t1, Id = "Location2" }
                }
            };

            var trigger2 = new NotificationTriggerEntity
            {
                Id = t2,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.TwinCategory,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };

            var trigger3 = new NotificationTriggerEntity
            {
                Id = t3,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.Twin,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                Twins = new List<NotificationTriggerTwinEntity> { new() { NotificationTriggerId = t3, TwinId = "Twin2" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };
            var trigger4 = new NotificationTriggerEntity
            {
                Id = t4,
                Source = NotificationSource.Insight,
                Focus = NotificationFocus.TwinCategory,
                IsEnabled = true,
                PriorityJson = new List<int> { 2 },
                Type = NotificationType.Personal,
                CreatedBy = Guid.NewGuid(),
                TwinCategories = new List<NotificationTriggerTwinCategoryEntity> { new() { NotificationTriggerId = t1, CategoryId = "TwinModel2" } },
                ChannelJson = [NotificationChannel.InApp],
                Locations = []
            };
            db.Add(trigger1);
            db.Add(trigger2);
            db.Add(trigger3);
            db.Add(trigger4);
            db.SaveChanges();

            var triggerFilterRules = new List<ITriggerFilterRule>
            {
               new SkillFilter(db),
               new SkillCategoryFilter(db),
               new TwinCategoryFilter(db),
               new TwinFilter(db)
            };
            var notificationsRepository = new NotificationsRepository(db);
            var notificationTriggerRepository = new NotificationTriggerRepository(db, triggerFilterRules);
            var notificationService = new NotificationService(notificationsRepository, userAuthorizationService.Object, notificationTriggerRepository);

            var notificationMessage = Fixture.Build<NotificationMessage>()
                                      .With(x => x.TwinId, "Twin1")
                                      .With(x => x.SkillId, "Skill1")
                                      .With(x => x.SkillCategoryId, 5)
                                      .With(x => x.ModelId, "TwinModel1")
                                      .With(x => x.Source, NotificationSource.Insight)
                                      .With(x => x.Priority, 2)
                                      .With(x => x.Locations, ["Location1", "Loc10", "LocA"])
                                      .Create();
            await notificationService.CreateNotificationAsync(notificationMessage);

            var notificationsEntities = db.Notifications
                            .Where(x => x.Source == NotificationSource.Insight && x.Title == notificationMessage.Title)
                            .ToList();

            notificationsEntities.Should().HaveCount(1);
            var expectedTriggerIds = new List<Guid> { t1, t2 };
            notificationsEntities.First().TriggerIdsJson.Should().BeEquivalentTo(expectedTriggerIds);
            var expectedNotificationUsers = (from userId in db.NotificationTriggers.Where(x => expectedTriggerIds.Contains(x.Id)).Select(x => x.CreatedBy)
                                             select new NotificationUserEntity
                                             {
                                                 NotificationId = notificationsEntities[0].Id,
                                                 UserId = userId,
                                                 State = NotificationUserState.New
                                             })
                                                                                 .ToList();
            var notificationsUsers = db.NotificationsUsers.ToList();
            notificationsUsers.Should().BeEquivalentTo(expectedNotificationUsers);
        }
    }


    private List<NotificationTriggerEntity> GetNotificationTriggerEntities(List<GroupModel> workgroupList)
    {
        var notificationTriggerEntities = new List<NotificationTriggerEntity>();


        var twinTriggerList = new List<NotificationTriggerTwinEntity> {
            new NotificationTriggerTwinEntity{ NotificationTriggerId = triggerId1, TwinId = "Twin1" },
            new NotificationTriggerTwinEntity{ NotificationTriggerId = triggerId2, TwinId = "Twin1" }
        };

        var skillTriggers = new List<NotificationTriggerSkillEntity> {
            new NotificationTriggerSkillEntity{ NotificationTriggerId = triggerId3, SkillId = "Skill-A" }
        };

        var LocationTriggerList = new List<LocationEntity> {
            new LocationEntity{ NotificationTriggerId = triggerId1, Id = "Location1" },
            new LocationEntity{ NotificationTriggerId = triggerId1, Id = "Location2" },
            new LocationEntity{ NotificationTriggerId = triggerId2, Id = "Location1" },
            new LocationEntity{ NotificationTriggerId = triggerId2, Id = "Location3" }
        };
        var workGroupSubscriptions = new List<WorkgroupSubscriptionEntity> {
            new WorkgroupSubscriptionEntity{ NotificationTriggerId = triggerId1, WorkgroupId = workgroupList[0].Id },
            new WorkgroupSubscriptionEntity{ NotificationTriggerId = triggerId2, WorkgroupId = workgroupList[1].Id }
        };
        // build notification trigger entities
        var notificationTriggerEntity1 = Fixture.Build<NotificationTriggerEntity>()
        .With(x => x.Id, triggerId1)
        .With(x => x.Source, NotificationSource.Insight)
        .With(x => x.IsEnabled, true)
        .With(x => x.IsMuted, false)
        .With(x => x.PriorityJson, new List<int> { 1, 2 })
        .With(x => x.Type, NotificationType.Workgroup)
        .With(x => x.CreatedBy, Guid.NewGuid())
        .With(x => x.Twins, twinTriggerList.Where(x => x.NotificationTriggerId == triggerId1).ToList())
        .Without(x => x.TwinCategories)
        .Without(x => x.Skills)
        .Without(x => x.SkillCategories)
        .With(x => x.Locations, LocationTriggerList.Where(x => x.NotificationTriggerId == triggerId1).ToList())
        .With(x => x.WorkgroupSubscriptions, workGroupSubscriptions.Where(x => x.WorkgroupId == workgroup1).ToList())
        .Create();


        var notificationTriggerEntity2 = Fixture.Build<NotificationTriggerEntity>()
       .With(x => x.Id, triggerId2)
       .With(x => x.Source, NotificationSource.Insight)
       .With(x => x.IsEnabled, true)
       .With(x => x.IsMuted, false)
       .With(x => x.PriorityJson, new List<int> { 2 })
       .With(x => x.Type, NotificationType.Workgroup)
       .With(x => x.CreatedBy, Guid.NewGuid())
       .With(x => x.Twins, twinTriggerList.Where(x => x.NotificationTriggerId == triggerId2).ToList())
       .Without(x => x.TwinCategories)
       .Without(x => x.Skills)
       .Without(x => x.SkillCategories)
       .With(x => x.Locations, LocationTriggerList.Where(x => x.NotificationTriggerId == triggerId2).ToList())
       .With(x => x.WorkgroupSubscriptions, workGroupSubscriptions.Where(x => x.WorkgroupId == workgroup2).ToList())
       .Create();

       var notificationTriggerEntity3 = Fixture.Build<NotificationTriggerEntity>()
      .With(x => x.Id, triggerId3)
      .With(x => x.Source, NotificationSource.Insight)
      .With(x => x.IsEnabled, true)
      .With(x => x.PriorityJson, new List<int> { 2 })
      .With(x => x.Type, NotificationType.Workgroup)
      .With(x => x.CreatedBy, Guid.NewGuid())
      .Without(x => x.Twins)
      .Without(x => x.TwinCategories)
      .With(x => x.Skills, skillTriggers.Where(x => x.NotificationTriggerId == triggerId3).ToList())
      .Without(x => x.SkillCategories)
      .With(x => x.Locations, LocationTriggerList.Where(x => x.NotificationTriggerId == triggerId2).ToList())
      .With(x => x.WorkgroupSubscriptions, workGroupSubscriptions.Where(x => x.WorkgroupId == workgroup2).ToList())
      .Create();



        notificationTriggerEntities.Add(notificationTriggerEntity1);
        notificationTriggerEntities.Add(notificationTriggerEntity2);

        return notificationTriggerEntities;
    }

    public record ExpectedResult(List<Guid> triggerIds, List<Guid> userIds);
}

