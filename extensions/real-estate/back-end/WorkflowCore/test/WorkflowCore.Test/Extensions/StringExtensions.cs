using Xunit;

namespace WorkflowCore.Test.Extensions
{
    public class StringExtensions
    {
        [Fact]
        public void StringExtensions_SubstringBefore()
        {
            Assert.Equal("Chevy", "Chevy Corvette".SubstringBefore(" Corvette"));
            Assert.Equal("Pontiac", "Pontiac Firebird".SubstringBefore(" Firebird"));
            Assert.Equal("Pontiac Firebird", "Pontiac Firebird".SubstringBefore("fred"));
            Assert.Equal("", "".SubstringBefore("fred"));
            Assert.Equal((string)null, ((string)null).SubstringBefore("fred"));
        }

        [Fact]
        public void StringExtensions_ToDictionary()
        {
            var orderBy = "name asc, date desc";
            var orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 2);
            Assert.True(orderByDict.ContainsKey("name"));
            Assert.Equal("asc", orderByDict["name"]);
            Assert.True(orderByDict.ContainsKey("date"));
            Assert.Equal("desc", orderByDict["date"]);

            orderBy = "name asc, date desc, name desc";
            orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 2);
            Assert.True(orderByDict.ContainsKey("name"));
            Assert.Equal("asc", orderByDict["name"]);
            Assert.True(orderByDict.ContainsKey("date"));
            Assert.Equal("desc", orderByDict["date"]);

            orderBy = "name desc, date";
            orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 2);
            Assert.True(orderByDict.ContainsKey("name"));
            Assert.Equal("desc", orderByDict["name"]);
            Assert.True(orderByDict.ContainsKey("date"));
            Assert.Equal("", orderByDict["date"]);

            orderBy = "name";
            orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 1);
            Assert.True(orderByDict.ContainsKey("name"));
            Assert.Equal("", orderByDict["name"]);

            orderBy = null;
            orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 0);

            orderBy = string.Empty;
            orderByDict = orderBy.CsvToDictionary();

            Assert.True(orderByDict.Count == 0);
        }
    }
}
