using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Caching.Memory;

using Xunit;
using Moq;

using Willow.Common;
using Willow.Platform.Localization;

namespace Willow.Platform.Localization.UnitTests
{
    public class StringExtensionsTests
    {
        [Fact]
        public void StringExtensions_ConvertDiacrits()
        {
            Assert.Equal("Nom",                         "Nom".ConvertDiacrits());                
            Assert.Equal("Unite de traitement d’air",   "Unité de traitement d’air".ConvertDiacrits());  
            Assert.Equal("Ventilateur d’extraction",    "Ventilateur d’extraction".ConvertDiacrits());  
            Assert.Equal("deja",                        "déjà".ConvertDiacrits());  
            Assert.Equal("pate",                        "pâte".ConvertDiacrits());  
            Assert.Equal("ile",                         "île".ConvertDiacrits());  
            Assert.Equal("hote",                        "hôte".ConvertDiacrits());  
            Assert.Equal("foret",                       "forêt".ConvertDiacrits());  
            Assert.Equal("naive",                       "naïve".ConvertDiacrits());  
            Assert.Equal("francais",                    "français".ConvertDiacrits());  
        }
    }
}
