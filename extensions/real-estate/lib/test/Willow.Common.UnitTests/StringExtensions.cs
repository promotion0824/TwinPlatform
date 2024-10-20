using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Xunit;

namespace Willow.Common.UnitTests
{
    public class StringExtensions
    {
        [Fact]
        public void StringExtensions_SubstringBefore()
        {
            Assert.Equal("Chevy",            "Chevy Corvette".SubstringBefore(" Corvette"));
            Assert.Equal("Pontiac",          "Pontiac Firebird".SubstringBefore(" Firebird"));
            Assert.Equal("Pontiac Firebird", "Pontiac Firebird".SubstringBefore("fred"));
            Assert.Equal("", "".SubstringBefore("fred"));
            Assert.Equal((string)null, ((string)null).SubstringBefore("fred"));
        }    
        
        [Fact]
        public void StringExtensions_Substitute()
        {
            Assert.Null(((string)null).Substitute(new { Adjective = "good" } ));
            Assert.Equal("Bob is a good guy", "Bob is a {Adjective} guy".Substitute(new { Adjective = "good" } ));
            Assert.Equal("Bob is a good guy", "Bob is a {Adjective} guy".Substitute(new { adjective = "good" } )); // Data case is ignored
            Assert.Equal("Bob is a good guy", "Bob is a {adjective} guy".Substitute(new { Adjective = "good" } )); // Data case is ignored
            Assert.Equal("Bob is a good guy and Fred is a good guy", "Bob is a {Adjective} guy and Fred is a {Adjective} guy".Substitute(new { Adjective = "good" } ));
            Assert.Equal("Bob is a {Adjective} guy", "Bob is a {Adjective} guy".Substitute(new { Verb = "good" } ));
            Assert.Equal("Bob is a fast running guy", "Bob is a {Adjective} {Verb} guy".Substitute(new { Adjective = "fast", Verb = "running" } ));
            Assert.Equal("Bob is a good guy", "Bob is a //Adjective// guy".Substitute(new { Adjective = "good" }, "//", "//" ));
            Assert.Equal("Bob is a good guy", "Bob is a good guy".Substitute(new { Adjective = "good" }, "//", "//" ));
            Assert.Equal("Bob is a  guy", "Bob is a {Adjective} guy".Substitute(new Data()));
        }

        #region ParseCSV

        [Fact]
        public void StringExtensions_ParseCSV()
        {
            var csv    = "ModelId,EnglishValue,TranslatedValue\r\nfoo,Name,Nom\r\nfoo,\"Air \"\"Handling\"\" Unit\",\"Unité \"\"de\"\" traitement, d’air\"\r\nfoo,Exhaust Fan,Ventilateur d’extraction";
            var parsed = csv.ParseCSV();

            Assert.Equal("ModelId",         parsed[0][0]);
            Assert.Equal("EnglishValue",     parsed[0][1]);
            Assert.Equal("TranslatedValue",  parsed[0][2]);

            Assert.Equal("foo",                             parsed[2][0]);
            Assert.Equal("Air \"Handling\" Unit",           parsed[2][1]);
            Assert.Equal("Unité \"de\" traitement, d’air",  parsed[2][2]);
        }

        [Fact]
        public void StringExtensions_ParseCSVLine()
        {
            var csv    = "foo,\"Air \"\"Handling\"\" Unit\",\"Unité \"\"de\"\" traitement, d’air\"";
            var parsed = csv.ParseCSVLine();

            Assert.Equal("foo",                             parsed[0]);
            Assert.Equal("Air \"Handling\" Unit",           parsed[1]);
            Assert.Equal("Unité \"de\" traitement, d’air",  parsed[2]);
        }        

        [Theory]
        [InlineData("frank,\"b\"\"ob\"", "b\"ob")]
        [InlineData("frank,\"b\"\"o\"\"b\"", "b\"o\"b")]
        [InlineData("frank,\"\"\"Bob's your uncle\"", "\"Bob's your uncle")]
        [InlineData("frank,\"\"\"Bob's your uncle\"\"\"", "\"Bob's your uncle\"")]
        public void StringExtensions_ParseCSVLine_double_double_quotes(string csv, string parsed1)
        {
            var parsed = csv.ParseCSVLine();

            Assert.Equal(parsed1,  parsed[1]);
        }        

        #endregion

        public class Data
        {
            public string Adjective { get; set; } = null;
        }
    }
}
