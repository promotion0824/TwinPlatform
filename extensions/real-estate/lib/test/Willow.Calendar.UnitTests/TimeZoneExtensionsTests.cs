using System;
using Xunit;

using Willow.Calendar;

namespace Willow.Calendar.UnitTests
{
    public class TimeZoneExtensionsTests
    {
        [Theory]
        [InlineData("Hawaiian Standard Time")]
        [InlineData("SA Pacific Standard Time")]
        [InlineData("UTC-11")]
        public void Windows_Return_Windows_Zone(string windowsZone)
        {
            Assert.Equal(windowsZone, windowsZone.FindEquivalentWindowsTimeZoneInfo().StandardName);
        }


        [Theory] // https://secure.jadeworld.com/JADETech/JADE2020/OnlineDocumentation/content/resources/encyclosys2/jadetimezone_class/ianawindowstimezonemapping.htm
        [InlineData("America/Bogota", "SA Pacific Standard Time")]
        [InlineData("America/Rio_Branco", "SA Pacific Standard Time")]
        [InlineData("America/Eirunepe", "SA Pacific Standard Time")]
        [InlineData("America/Coral_Harbour", "SA Pacific Standard Time")]
        [InlineData("America/Guayaquil", "SA Pacific Standard Time")]
        [InlineData("America/Jamaica", "SA Pacific Standard Time")]
        [InlineData("America/Cayman", "SA Pacific Standard Time")]
        [InlineData("America/Panama", "SA Pacific Standard Time")]
        [InlineData("America/Lima", "SA Pacific Standard Time")]
        [InlineData("Etc/GMT+5", "SA Pacific Standard Time")]
        public void Iana_Return_Windows_Zone(string ianaZone, string windowsZone)
        {
            Assert.Equal(windowsZone, ianaZone.FindEquivalentWindowsTimeZoneInfo().StandardName);
        }
        
        [Fact]
        public void Throws_For_Missing_TZ()
        {
            Assert.Throws<TimeZoneNotFoundException>(() => "Not A Zone".FindEquivalentWindowsTimeZoneInfo());
        }
        
        [Fact]
        public void Throws_For_Empty_TZ()
        {
            Assert.Throws<TimeZoneNotFoundException>(() => string.Empty.FindEquivalentWindowsTimeZoneInfo());
        }
    }
}