using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

using Willow.Common;

namespace Willow.Common.UnitTests
{
    public class ObjectExtensions
    {
        [Fact]
        public void ObjectExtensions_GetValue()
        {
            var myCar = new Automobile { Make = "Chevy", Model = "Corvette", Year = 1961 };

            Assert.Equal("Chevy", myCar.GetValue<string>("Make"));
            Assert.Equal("Corvette", myCar.GetValue<string>("Model"));
            Assert.Equal(1961, myCar.GetValue<int>("Year"));
        }
         
        [Fact]
        public void ObjectExtensions_GetValue2()
        {
            var myCar = new { Make = "Chevy", Model = "Corvette", Year = 1961 };

            Assert.Equal("Chevy", myCar.GetValue<string>("Make"));
            Assert.Equal("Corvette", myCar.GetValue<string>("Model"));
            Assert.Equal(1961, myCar.GetValue<int>("Year"));
        }
         
        [Fact]
        public void ObjectExtensions_ToDictionary()
        {
            var myCar = new { Make = "Chevy", Model = "Corvette", Year = 1961 };
            var dict  = myCar.ToDictionary();

            Assert.Equal(3, dict.Count);
            Assert.Equal("Chevy",    dict["Make"]);
            Assert.Equal("Corvette", dict["Model"]);
            Assert.Equal(1961,       dict["Year"]);
        }
         
        [Fact]
        public void ObjectExtensions_ToDictionary2()
        {
            var myCar = new Automobile { Make = "Chevy", Model = "Corvette", Year = 1961 };
            var dict  = myCar.ToDictionary();

            Assert.Equal(3, dict.Count);
            Assert.Equal("Chevy",    dict["Make"]);
            Assert.Equal("Corvette", dict["Model"]);
            Assert.Equal(1961,       dict["Year"]);
        }
         
        [Fact]
        public void ObjectExtensions_ToDictionary3()
        {
            var myCar = new Dictionary<string, string> { { "Make", "Chevy" }, {"Model", "Corvette" }, {"Year", "1961" }};
            var dict  = myCar.ToDictionary();

            Assert.Equal(3, dict.Count);
            Assert.Equal("Chevy",    dict["Make"]);
            Assert.Equal("Corvette", dict["Model"]);
            Assert.Equal("1961",     dict["Year"]);
        }
         
        [Fact]
        public void ObjectExtensions_ToDictionary_null()
        {
            var myCar = new { Make = "Chevy", Model = (string)null, Year = 1961 };
            var dict  = myCar.ToDictionary();

            Assert.Equal(3, dict.Count);
            Assert.Equal("Chevy",      dict["Make"]);
            Assert.Equal((string)null, dict["Model"]);
            Assert.Equal(1961,         dict["Year"]);
        }
         
        [Fact]
        public void ObjectExtensions_Throw()
        {
            try
            {
                this.Throw<UnauthorizedAccessException>("Yo", new { UserId = "Bob", SiteId = "Fresno" } );
                Assert.False(true, "Exception not thrown");
            }
            catch(UnauthorizedAccessException ex)
            {
                Assert.Equal("Yo",     ex.Message);
                Assert.Equal("Bob",    ex.Data["UserId"].ToString());
                Assert.Equal("Fresno", ex.Data["SiteId"].ToString());
            }
        }
         
        internal class Automobile
        {
            public string Make  { get; set; }
            public string Model { get; set; }
            public int    Year  { get; set; }
        }
    }
}
