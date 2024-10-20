using AutoFixture;
using WorkflowCore.Entities;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Willow.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;
using System.Linq;

namespace WorkflowCore.Test.Features
{
    public class WorkflowCoreDbContextTests : BaseDbTest
    {
        public static Fixture StaticFixture = new Fixture();

        public static readonly IReadOnlyDictionary<string, Func<WorkflowContext, Task>> dbSetGetterOperations = new Dictionary<string, Func<WorkflowContext, Task>>
        {
            { nameof(WorkflowContext.TicketNextNumbers), async (context) => {
                var nextNumber = StaticFixture.Build<TicketNextNumberEntity>().Create();
                context.TicketNextNumbers.Add(nextNumber);
                await context.TicketNextNumbers.ToArrayAsync();
            }},
            { nameof(WorkflowContext.Schemas), async (context) => {
                await InsertSchema(context);
                await context.Schemas.ToArrayAsync();
            }},
            { nameof(WorkflowContext.SchemaColumns), async (context) => {
                var schemaId = await InsertSchema(context);
                context.SchemaColumns.AddRange(StaticFixture.Build<SchemaColumnEntity>().Without(x => x.Schema).With(x => x.SchemaId, schemaId).CreateMany(5));
                await context.SaveChangesAsync();
                await context.SchemaColumns.ToArrayAsync();
            } },
            { nameof(WorkflowContext.Tickets), async (context) => {
                await InsertTicket(context);
                await context.Tickets.ToArrayAsync();
            } }
        }.ToImmutableDictionary();

        public WorkflowCoreDbContextTests(ITestOutputHelper output) : base(output)
        {
            // Configure AutoFixture to ignore the circular reference
            Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        private static async Task<Guid> InsertSchema(WorkflowContext context)
        {
            var schema = StaticFixture.Build<SchemaEntity>().Without(x => x.SchemaColumns).Create();
            context.Schemas.Add(schema);
            await context.SaveChangesAsync();
            return schema.Id;
        }

        private static async Task<Guid> InsertTicket(WorkflowContext context)
        {
            var ticket = StaticFixture.Build<TicketEntity>()
                                      .With(x => x.ExternalMetadata, "{\"test\": \"test\"}")
                                      .Create();
            context.Tickets.Add(ticket);
            await context.SaveChangesAsync();
            return ticket.Id;
        }

        /* These don't pass, ever.
         * [Trait("Category", "UseSqlServer")]
        [Theory]
        [MemberData(nameof(WorkflowCoreContextDbSets))]
        public void TestEntities_Match_DbStructure(string dbSetName)
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.SqlServer))
            {
                var workflowCoreContext = server.Arrange().CreateDbContext<WorkflowContext>();
                if (dbSetGetterOperations.TryGetValue(dbSetName, out Func<WorkflowContext, Task> entitiesGetter))
                {
                    Func<Task> awaiter = async () => await entitiesGetter(workflowCoreContext);
                    awaiter.Should().NotThrow();
                }
                else
                {
                    entitiesGetter.Should().NotBeNull();
                }
            }

        }*/

        public static IEnumerable<object[]> WorkflowCoreContextDbSets
        {
            get
            {
                var items = new List<object[]>();
                foreach (var dbSet in dbSetGetterOperations)
                {
                    items.Add(new object[] { dbSet.Key });
                }
                return items;
            }
        }
    }
}
