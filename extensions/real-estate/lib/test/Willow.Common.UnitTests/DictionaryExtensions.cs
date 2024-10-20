using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

using Willow.Common;

namespace Willow.Common.UnitTests
{
    public class DictionaryExtensions
    {
        [Fact]
        public void DictionaryExtensions_Merge()
        {
            var dest = new Dictionary<string, string> { { "Make", "Chevy" }, { "Model", "Corvette" } };
            var src  = new Dictionary<string, string> { { "Cylinders", "8" }, { "Displacement", "350" } };

            dest.Merge(src);

            Assert.Equal("Chevy",    dest["Make"]);
            Assert.Equal("Corvette", dest["Model"]);
            Assert.Equal("8",        dest["Cylinders"]);
            Assert.Equal("350",      dest["Displacement"]);
        }

        [Fact]
        public void DictionaryExtensions_Merge_overwrite()
        {
            var dest = new Dictionary<string, string> { { "Make", "Chevy" }, { "Model", "Corvette" } };
            var src  = new Dictionary<string, string> { { "Model", "Camaro" }, { "Cylinders", "8" }, { "Displacement", "350" } };

            dest.Merge(src);

            Assert.Equal("Chevy",    dest["Make"]);
            Assert.Equal("Camaro",   dest["Model"]);
            Assert.Equal("8",        dest["Cylinders"]);
            Assert.Equal("350",      dest["Displacement"]);
        }
    }
}
