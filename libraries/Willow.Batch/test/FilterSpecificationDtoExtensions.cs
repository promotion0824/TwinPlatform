namespace Willow.Batch.UnitTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using AutoFixture;
    using FluentAssertions;
    using Xunit;

    public class FilterSpecificationDtoExtensions
    {
        private static DateTime timestamp = DateTime.UtcNow;
        private static List<Person> people = new Fixture().CreateMany<Person>(10).ToList();
        private static List<PersonGender> genders = new List<PersonGender> { PersonGender.Female, PersonGender.Male };
        private static List<string> notes = new List<string> { n1 };
        private static List<int> emptyList = new List<int>();
        private static List<int> nullList = null;
        private static string n1 = "n1";

        private static Expression<Func<Person, bool>>[] expressions = new Expression<Func<Person, bool>>[]
        {
            x => x.Name != null && x.Name.Trim().ToLower() == people.First().Name.Trim().ToLower(),
            x => x.Name != null && x.Name.Trim().ToLower().EndsWith(people.First().Name.Substring(1)),
            x => x.Name != null && x.Name.Trim().ToLower().Contains(people.First().Name.Substring(1, 1)),
            x => x.Id == 10,
            x => x.Id >= 5,
            x => x.Birth != null && x.Birth.Date == timestamp,
            x => x.Birth != null && x.Birth.Date < timestamp,
            x => x.UniqueId == Guid.Empty,
            x => x.UniqueId != Guid.Empty,
            x => x.Gender == PersonGender.Female,
            x => genders.Contains(x.Gender),
            x => x.Notes != null && notes.Contains(x.Notes),
            x => x.Contacts != null && x.Contacts.Any(i => i.IsDeleted == true),
            _ => true,
            x => x.Name.Trim().ToLower().Contains(people.First().Name.Substring(1, 2)) || x.Notes.Trim().ToLower().Contains(people.First().Name.Substring(1, 2)),
            x => (x.Birth != null && x.Birth.Country.Trim().ToLower().Contains(people.First().Notes.Substring(1, 2))) || x.Notes.Trim().ToLower().Contains(people.First().Notes.Substring(1, 2)),
        };

        public enum PersonGender
        {
            /// <summary>
            /// Male Gender.
            /// </summary>
            Male,

            /// <summary>
            /// Female Gender.
            /// </summary>
            Female,
        }

        public enum ContactType
        {
            /// <summary>
            /// Contact Type: Telephone.
            /// </summary>
            Telephone,

            /// <summary>
            /// Contact Type: Email.
            /// </summary>
            Email,
        }

        public static IEnumerable<object[]> GetTestData()
        {
            yield return new object[] { "Name", FilterOperators.EqualsLiteral, people.First().Name.Trim().ToLower(), expressions[0] };
            yield return new object[] { "Name", FilterOperators.EndsWith, people.First().Name.Substring(1), expressions[1] };
            yield return new object[] { "Name", FilterOperators.Contains, people.First().Name.Substring(1, 1), expressions[2] };
            yield return new object[] { "Id", FilterOperators.EqualsShort, 10, expressions[3] };
            yield return new object[] { "Id", FilterOperators.GreaterThanOrEqual, 5, expressions[4] };
            yield return new object[] { "Birth.Date", FilterOperators.EqualsLiteral, timestamp, expressions[5] };
            yield return new object[] { "Birth.Date", FilterOperators.LessThan, timestamp, expressions[6] };
            yield return new object[] { "UniqueId", FilterOperators.EqualsLiteral, Guid.Empty, expressions[7] };
            yield return new object[] { "UniqueId", FilterOperators.NotEquals, Guid.Empty, expressions[8] };
            yield return new object[] { "Gender", FilterOperators.EqualsLiteral, PersonGender.Female, expressions[9] };
            yield return new object[] { "Gender", FilterOperators.ContainedIn, genders, expressions[10] };
            yield return new object[] { "Notes", FilterOperators.ContainedIn, notes, expressions[11] };
            yield return new object[] { "Contacts[IsDeleted]", FilterOperators.EqualsLiteral, true, expressions[12] };
            yield return new object[] { "Id", FilterOperators.ContainedIn, emptyList, expressions[13] };
            yield return new object[] { "Id", FilterOperators.ContainedIn, nullList, expressions[13] };
            yield return new object[] { "Name, Notes", FilterOperators.Contains, people.First().Notes.Substring(1, 2), expressions[14] };
            yield return new object[] { "Birth.Country, Notes", FilterOperators.Contains, people.First().Notes.Substring(1, 2), expressions[15] };
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void FilterSpecificationDtoExtensions_Validate(string field, string op, object value, Expression<Func<Person, bool>> expectedExpression)
        {
            var filter = new FilterSpecificationDto { Field = field, Operator = op, Value = value };

            if (expectedExpression.Parameters.FirstOrDefault().Name == "_")
            {
                Assert.True(filter.Validate(null).Any());
            }
            else
            {
                Assert.False(filter.Validate(null).Any());
            }
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void FilterSpecificationDtoExtensions_Build(string field, string op, object value, Expression<Func<Person, bool>> expectedExpression)
        {
            var filter = new FilterSpecificationDto { Field = field, Operator = op, Value = value };

            var expression = filter.Build<Person>();

            expression.Should().BeEquivalentTo(expectedExpression);
        }

        [Theory]
        [MemberData(nameof(GetTestData))]
        public void FilterSpecificationDtoExtensions_FilterBy(string field, string op, object value, Expression<Func<Person, bool>> expectedExpression)
        {
            var filter = new FilterSpecificationDto { Field = field, Operator = op, Value = value };

            var result = people.AsQueryable().FilterBy(new FilterSpecificationDto[] { filter });

            result.Should().BeEquivalentTo(people.AsQueryable().Where(expectedExpression));
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        public async Task FilterSpecificationDtoExtensions_Paginate(int page, int pageSize)
        {
            var result = await people.AsQueryable().Paginate(page, pageSize);

            result.Total.Should().Be(people.Count());
            result.Before.Should().Be(people.Take((page - 1) * pageSize).Count());
            result.After.Should().Be(people.Skip(page * pageSize).Count());
            result.Items.Count().Should().Be(pageSize);
        }

        public class Person
        {
            public int Id { get; set; }

            public Guid UniqueId { get; set; }

            public string Name { get; set; }

            public PersonGender Gender { get; set; }

            public BirthData Birth { get; set; }

            public List<Contact> Contacts { get; set; }

            public string Notes { get; set; }

            public class BirthData
            {
                public DateTime? Date { get; set; }

                public string Country { get; set; }
            }
        }

        public class Contact
        {
            public ContactType Type { get; set; }

            public string Value { get; set; }

            public string Comments { get; set; }

            public bool IsDeleted { get; set; }
        }
    }
}
