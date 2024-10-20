using System;
using Xunit;

namespace Willow.Common.UnitTests
{
    public class ExceptionExtensions
    {
        [Fact]
        public void ExceptionExtensions_WithData()
        {
            try
            {
                throw new UnauthorizedAccessException().WithData(new { UserId = "Bob", SiteId = "Fresno" } );
            }
            catch(UnauthorizedAccessException ex)
            {
                Assert.Equal("Bob",    ex.Data["UserId"].ToString());
                Assert.Equal("Fresno", ex.Data["SiteId"].ToString());
            }
        }

        [Fact]
        public void ExceptionExtensions_WithNamedData()
        {
            try
            {
                throw new UnauthorizedAccessException().WithData("Yo", new { UserId = "Bob", SiteId = "Fresno" });
            }
            catch (UnauthorizedAccessException ex)
            {
                Assert.Equal(new { UserId = "Bob", SiteId = "Fresno" }, ex.Data["Yo"]);
            }
        }
    }
}
