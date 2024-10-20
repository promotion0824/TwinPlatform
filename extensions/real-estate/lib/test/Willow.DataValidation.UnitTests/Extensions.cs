using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;

using Xunit;
using Willow.DataValidation.Annotations;

namespace Willow.DataValidation.UnitTests
{
    public class ExtensionsTests
    {
        private readonly List<(string Name, string Message)> _errors;

        public ExtensionsTests()
        {
            _errors = new List<(string Name, string Message)>();
        }

        [Fact]
        public void Extension_NoAnnotations()
        {
            var obj = new NoAnnotations();

            ValidateProperty(obj);
        }
        
        [Fact]
        public void Extension_Validate_required_string()
        {
            ValidateProperty(new ScheduleRequest    { Name = null }, "Name", "The Name field is required.");
            ValidateProperty(new ScheduleRequest1a  { Name = null }, "Name", "You need to specify the Name");
            ValidateProperty(new ScheduleRequest1b  { Name = null }, "Name", "You need to specify the Name");
            ValidateProperty(new ScheduleRequest    { Name = "" },  "Name", "The Name field is required.");
            ValidateProperty(new ScheduleRequest    { Name = " " }, "Name", "The Name field is required.");
        }

        [Fact]
        public void Extension_Validate_required_nullable()
        {
            ValidateProperty(new Customer { }, "Id", "The Id field is required.");
        }

        [Fact]
        public void Extension_Validate_stringlength()
        {
            ValidateProperty(new ScheduleRequest  { Name = "bob" });
            ValidateProperty(new ScheduleRequest  { Name = "bobsyouruncle" }, "Name", "The field Name must be a string with a minimum length of 3 and a maximum length of 5.");
            ValidateProperty(new ScheduleRequest2 { Name = "bobsyouruncle" }, "Name", "The field Name must be a string with a maximum length of 5.");
        }

        [Fact]
        public void Extension_Validate_minlength()
        {
            ValidateProperty(new ScheduleRequest3 { Name = "bob" });
            ValidateProperty(new ScheduleRequest3 { Name = "bo" },            "Name", "The field Name must be a string or array type with a minimum length of '3'.");
            ValidateProperty(new ScheduleRequest3 { Name = "bobsyouruncle" }, "Name", "The field Name must be a string or array type with a maximum length of '5'.");
            
            ValidateProperty(new ScheduleRequest4 { Name = "bo" },            "Name", "Name must have a minimum length of 3");
            ValidateProperty(new ScheduleRequest4 { Name = "bobsyouruncle" }, "Name", "Name has a maximum length of 5");
        }

        [Fact]
        public void Extension_Validate_notrequired()
        {
            ValidateProperty(new ScheduleRequest5 { Name = null });
        }

        [Fact]
        public void Extension_Validate_dateasstring()
        {
            ValidateProperty(new ScheduleRequest6  { StartDate = "2021-01-01T00:00:00" });
            ValidateProperty(new ScheduleRequest6  { StartDate = "bob" },                   "StartDate", "StartDate is not a valid DateTime");
            ValidateProperty(new ScheduleRequest6a { StartDate = "bob" },                   "StartDate", "StartDate is not a valid DateTime");
        }

        [Fact]
        public void Extension_Validate_greaterthan()
        {
            ValidateProperty(new ScheduleRequest7 { StartDate = "2021-01-01T00:00:00" });
            ValidateProperty(new ScheduleRequest7 { StartDate = "2021-01-01T00:00:00", EndDate = "2019-01-01T00:00:00" }, "EndDate", "EndDate must be greater than StartDate");
        }

        [Fact]
        public void Extension_Validate_greaterthan2()
        {
            ValidateProperty(new Automobile { WheelWidth = 20, TireWidth = 24 });
            ValidateProperty(new Automobile { WheelWidth = 20, TireWidth = 19 }, "TireWidth", "TireWidth must be greater than WheelWidth");
        }

        [Fact]
        public void Extension_Validate_greaterthanequal()
        {
            ValidateProperty(new Automobile4 { WheelWidth = 20, TireWidth = 20 });
            ValidateProperty(new Automobile4 { WheelWidth = 20, TireWidth = 24 });
            ValidateProperty(new Automobile4 { WheelWidth = 20, TireWidth = 19 }, "TireWidth", "TireWidth must be greater than or equal to WheelWidth");
        }

        [Fact]
        public void Extension_Validate_lessthan()
        {
            ValidateProperty(new Automobile2 { WheelWidth = 20, TireWidth = 24 });
            ValidateProperty(new Automobile2 { WheelWidth = 20, TireWidth = 19 }, "WheelWidth", "WheelWidth must be less than TireWidth");
            ValidateProperty(new Automobile2 { WheelWidth = 20, TireWidth = 20 }, "WheelWidth", "WheelWidth must be less than TireWidth");
        }

        [Fact]
        public void Extension_Validate_lessthanequal()
        {
            ValidateProperty(new Automobile3 { WheelWidth = 20, TireWidth = 24 });
            ValidateProperty(new Automobile3 { WheelWidth = 20, TireWidth = 20 });
            ValidateProperty(new Automobile3 { WheelWidth = 20, TireWidth = 19 }, "WheelWidth", "WheelWidth must be less than or equal to TireWidth");
        }

        [Fact]
        public void Extension_Validate_equal()
        {
            ValidateProperty(new SomeObject { FirstValue = 20, SecondValue = 20 });
            ValidateProperty(new SomeObject { FirstValue = 20, SecondValue = 19 }, "FirstValue", "Values must be the same");
        }

        [Fact]
        public void Extension_Validate_Validate_child_object()
        {
            var obj = new Automobile5 { WheelWidth = 20, TireWidth = 24, Engine = new Engine { Displacement = 350, CylinderVolume = 351} };

            ValidateProperty(new Automobile5 { WheelWidth = 20, TireWidth = 24 }, "Engine", "The Engine field is required.");
            ValidateProperty(obj, "Engine.CylinderVolume", "Engine.CylinderVolume must be less than total Displacement");
        }

        [Fact]
        public void Extension_Validate_Validate_child_list()
        {
            var obj = new Automobile6 { WheelWidth = 20, TireWidth = 24, Tires = new List<Tire> { new Tire { Width = 20, Height = 14},
                                                                                                  new Tire { Width = 20, Height = 24},
                                                                                                  new Tire { Width = 20, Height = 24},
                                                                                                  new Tire { Width = 20, Height = 24} } };

            ValidateProperty(new Automobile6 { WheelWidth = 20, TireWidth = 24 }, "Tires", "The Tires field is required.");
            ValidateProperty(new Automobile6 { WheelWidth = 20, TireWidth = 24, Tires = new List<Tire> { } }, "Tires", "Tires must have a minimum count of 4");
            ValidateProperty(obj, "Tires[0].Height", "Tires[0].Height must be greater than Width");
        }

        [Fact]
        public void Extension_Validate_Validate_datetime()
        {
            var a1 = new Ancestor { Dob = new DateTime(1848, 5, 11, 0, 0, 0), Dod = new DateTime(1921, 3, 23, 0, 0, 0) };
            var a2 = new Ancestor { Dob = new DateTime(1848, 5, 11, 0, 0, 0), Dod = new DateTime(1821, 3, 23, 0, 0, 0) };

            ValidateProperty(a1);
            ValidateProperty(a2, "Dob", "Dob must be before Dod");
        }

        [Fact]
        public void Extension_Validate_RequiredIf()
        {
            var engine1 = new Engine2 { FuelType = "Gas" };
            var engine2 = new Engine2 { FuelType = "Gas", Carburator = new Carburator { FlowValveSize = 3.2 } };

            ValidateProperty(engine1, "Carburator", "Carburator is required");
            ValidateProperty(engine2);
        }

        [Fact]
        public void Extension_Validate_RequiredIfNotEmpty()
        {
            var engine1 = new Engine3 { FuelType = "Gas" };
            var engine2 = new Engine3 { FuelType = "Gas", Carburator = new Carburator { FlowValveSize = 3.2 } };
            var engine3 = new Engine3 { };
            var engine4 = new Engine3 { FuelType = "" };

            ValidateProperty(engine1, "Carburator", "Carburator is required");
            ValidateProperty(engine2);
            ValidateProperty(engine3);
            ValidateProperty(engine4);
        }

        [Theory]
        [InlineData("bob", null, false)]
        [InlineData(null, "bob", false)]
        [InlineData("",   "bob", false)]
        [InlineData("",   null)]
        [InlineData(null, null)]
        [InlineData("",   "")]
        public void Extension_Validate_RequiredIfEmpty_allowemptystrings(string fuelType, string carburator, bool shouldFail = true)
        {
            var engine = new Engine5 { FuelType = fuelType, Carburator = carburator };

            if(shouldFail)
                ValidateProperty(engine, "Carburator", "Carburator is required");
            else
                ValidateProperty(engine);
        }

        [Fact]
        public void Extension_Validate_RequiredIfEmpty()
        {
            var engine1 = new Engine4 { FuelType = "" };
            var engine2 = new Engine4 { Carburator = new Carburator { FlowValveSize = 3.2 } };
            var engine3 = new Engine4 { };

            ValidateProperty(engine1, "Carburator", "Carburator is required");
            ValidateProperty(engine2);
            ValidateProperty(engine3, "Carburator", "Carburator is required");
        }

        [Fact]
        public void Extension_Validate_ItemStringLength()
        {
            var p1 = new Person { Hobbies = new string[] { "Frog Walking", "Cricket Listening".PadRight(320, 'A') }  };

            ValidateProperty(p1, "Hobbies", "Hobby name is too long");
        }

        [Fact]
        public void Extension_Validate_MinCount_array()
        {
            var p2 = new Person2 { Hobbies = new string[] { "Frog Walking", "Cricket Listening" }  };

            ValidateProperty(p2, "Hobbies", "Hobbies must have a minimum count of 4");
        }

        [Fact]
        public void Extension_Validate_HtmlContent_success()
        {
            ValidateProperty(new TicketTemplate { Name = "Bobs Your Uncle" });
        }

        [Fact]
        public void Extension_Validate_HtmlContent_bad()
        {
            ValidateProperty(new TicketTemplate { Name = "Bobs Your <a>https://click.me</a>" }, "Name", "Name has invalid characters in it");
        }

        [Theory]
        [InlineData(CheckType.Numeric, null)]
        [InlineData(CheckType.Total, null)]
        public void Extension_Validate_invalid_decimalPlaces(CheckType type, int? decimalPlaces)
        {
            var request = new RandomRequest
            {
                Type = type,
                DecimalPlaces = decimalPlaces
            };

            ValidateProperty(request, "DecimalPlaces", "DecimalPlaces is required");
        }

        [Theory]
        [InlineData(CheckType.Numeric, null)]
        public void Extension_Validate_RequiredIfNot(CheckType type, int? decimalPlaces)
        {
            var request = new RandomRequest3
            {
                Type = type,
                DecimalPlaces = decimalPlaces
            };

            ValidateProperty(request, "DecimalPlaces", "DecimalPlaces is required");
        }

        [Theory]
        [InlineData(CheckType.Numeric, null)]
        [InlineData(CheckType.Total, null)]
        [InlineData(CheckType.Date, null, false)]
        [InlineData(CheckType.List, null, false)]
        public void Extension_Validate_RequiredIfNot3(CheckType type, int? decimalPlaces, bool shouldFail = true)
        {
            var request = new RandomRequest4
            {
                Type = type,
                DecimalPlaces = decimalPlaces
            };

            if(shouldFail)
                ValidateProperty(request, "DecimalPlaces", "DecimalPlaces is required");
            else 
                ValidateProperty(request);
        }

        [Theory]
        [InlineData(CheckType.Numeric, null)]
        [InlineData(CheckType.Total, null)]
        public void Extension_Validate_invalid_decimalPlaces2(CheckType type, int? decimalPlaces)
        {
            var request = new RandomRequest2
            {
                Type = type,
                DecimalPlaces = decimalPlaces
            };

            ValidateProperty(request, "DecimalPlaces", "DecimalPlaces is required");
        }

        [Theory]
        [InlineData(CheckType.Numeric, -1)]
        [InlineData(CheckType.Numeric, 32)]
        [InlineData(CheckType.Total, -1)]
        [InlineData(CheckType.Total, 32)]
        public void Extension_Validate_invalid_decimalPlaces_range(CheckType type, int? decimalPlaces)
        {
            var request = new RandomRequest
            {
                Type = type,
                DecimalPlaces = decimalPlaces
            };

            ValidateProperty(request, "DecimalPlaces", "DecimalPlaces must be between 1 and 6");
        }

        [Theory]
        [InlineData("Bob,Joe,Mary", false)]
        [InlineData("Joe,Joe,Mary", true)]
        public void Extension_Validate_Unique_children_fails(string children, bool shouldFail)
        {
            var request = new Parent
            {
                Children = new List<string>(children.Split(','))
            };

            if(shouldFail)
                ValidateProperty(request, "Children", "Children must have unique values");
            else
                ValidateProperty(request);
        }

        [Theory]
        [InlineData("Bob,Joe,Mary", false)]
        [InlineData("Joe,Joe,Mary", true)]
        public void Extension_Validate_Unique_children_fails2(string children, bool shouldFail)
        {
            var request = new Parent2
            {
                Children = (new List<string>(children.Split(','))).Select( i=> new Child {  Name = i }).ToList()
            };

            if(shouldFail)
                ValidateProperty(request, "Children", "Children must have unique values");
            else
                ValidateProperty(request);
        }

        [Theory]
        [InlineData("Bob,Joe,Mary", false)]
        [InlineData("Joe,Joe,Mary", true)]
        public void Extension_Validate_UniqueStringList_children_fails3(string children, bool shouldFail)
        {
            var request = new Parent3
            {
                Children = children
            };

            if(shouldFail)
                ValidateProperty(request, "Children", "Children must have unique values");
            else
                ValidateProperty(request);
        }

        [Theory]
        [InlineData("Bob,Joe,Mary", 50, false)]
        [InlineData("Joe,Joe,Mary", 50, true)]
        [InlineData("Joe,Joe,Mary", 100, false)]
        public void Extension_Validate_UniqueStringListIf_children(string children, int otherValue, bool shouldFail)
        {
            var request = new Parent4
            {
                NumBedrooms = otherValue,
                Children = children
            };

            if(shouldFail)
                ValidateProperty(request, "Children", "Children must have unique values");
            else
                ValidateProperty(request);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("\r")]
        public void Extension_Validate_Checks_Name_required(string name)
        {
            var parent = new Parent2
            {
                Children = new List<Child>
                {
                    new Child { Name = name }
                }
            };

           ValidateProperty(parent, "Children[0].Name", "Children[0].Name is required");
        }

        [Theory]
        [InlineData(null,  true)]
        [InlineData("bob@bob.bob",  true)]
        [InlineData("7bob@bob.bob",  true)]
        [InlineData("bob!jones@bob.bob", true)]
        [InlineData("bob#jones@bob.bob", true)]
        [InlineData("bob%jones@bob.bob", true)]
        [InlineData("\"bob(jones\"@bob.bob", true)]
        [InlineData("\"bob,jones\"@bob.bob", true)]
        [InlineData("\"bob<frank>jones\"@bob.bob", true)]
        [InlineData("bobjones@bob-bob.bob", true)]
        [InlineData("abc@xyz.com",  true)]
        [InlineData("bob.jones@bob.com", true)]

        [InlineData("",  false)]
        [InlineData(" ",  false)]
        [InlineData("bob<frank>jones@bob.bob", false)]
        [InlineData("bob(jones@bob.bob", false)]
        [InlineData("bob,jones@bob.bob", false)]
        [InlineData("bob@@bob.bob", false)]
        [InlineData("bob@bo@b.bob", false)]
        [InlineData("bob@bob..bob", false)]
        [InlineData(".bob@bob.bob", false)]

        [InlineData("bobjones@bob.#bob", false)]
        [InlineData("bobjones@bob.bob-", false)]
        [InlineData("bobjones@-bob.bob", false)]
        [InlineData("bobjones@bo}b.bob", false)]
        [InlineData("bobjones@bob", false)]
        [InlineData("bobjones@bob.b", false)]
        public void Extension_Email(string email, bool valid)
        {
            var template = new TicketTemplate
            {
                Name  = "bob",
                Email = email
            };

            if(valid)
                ValidateProperty(template);
            else
                ValidateProperty(template, "Email", "Email is invalid");
        }

        [Fact]
        public void Extension_Validate_alphanumeric()
        {
            ValidateProperty(new RandomRequest5 { Locale = "en-fr", TemplatName = "template100"});
            ValidateProperty(new RandomRequest5 { Locale = "en@", TemplatName = "template100" }, "Locale", "Locale has non alphanumeric characters in it");
            ValidateProperty(new RandomRequest5 { Locale = "en", TemplatName = "template@100" }, "TemplatName", "TemplatName has non alphanumeric characters in it");
            ValidateProperty(new RandomRequest5 { Locale = "enâê", TemplatName = "template100" }, "Locale", "Locale has non alphanumeric characters in it");
            ValidateProperty(new RandomRequest5 { Locale = "en", TemplatName = "你好" }, "TemplatName", "TemplatName has non alphanumeric characters in it");
        }

        [Fact]
        public void Extension_Validate_isoneof()
        {
            ValidateProperty(new Person3 { Gender = "Male" });
            ValidateProperty(new Person3 { Gender = "Female" });
            ValidateProperty(new Person5 { Gender = "Male" });
            ValidateProperty(new Person5 { Gender2 = null });
            ValidateProperty(new Person3 { Gender = "Bob" },"Gender", "Please enter one of the allowable values: Male, Female.");
        }


        [Fact]
        public void Extension_Validate_isoneofnumeric()
        {
            ValidateProperty(new Person4 { Age = 23});
            ValidateProperty(new Person6 { Age = 24 });
            ValidateProperty(new Person6 { Age2 = null });
            ValidateProperty(new Person4 { Age = 30 }, "Age", "Please enter one of the allowable values: 21, 22, 23, 24, 25.");
        }


        private void ValidateProperty(object data, string name = null, string msg = null)
        {
            _errors.Clear();

            if(msg != null)
            { 
                Assert.False(data.Validate(_errors));
                Assert.Single(_errors);
                Assert.Equal(name, _errors[0].Name);
                Assert.Equal(msg, _errors[0].Message);
            }
            else
            {
                Assert.True(data.Validate(_errors));
                Assert.Empty(_errors);
            }
        }

        #region Test Models

        public class ScheduleRequest
        {
            [Required(AllowEmptyStrings = false)]
            [StringLength(5, MinimumLength = 3)]  
            public string Name { get; set; }
        }

        public class ScheduleRequest2
        {
            [Required]
            [StringLength(5)]  
            public string Name { get; set; }
        }

        public class ScheduleRequest1a
        {
            [Required(ErrorMessage = "You need to specify the Name")]
            public string Name { get; set; }
        }

        public class ScheduleRequest1b
        {
            [Required(ErrorMessage = "You need to specify the {0}")]
            public string Name { get; set; }
        }
        
        public class ScheduleRequest3
        {
            [Required]
            [MinLength(3)]  
            [MaxLength(5)]  
            public string Name { get; set; }
        }
        
        public class ScheduleRequest4
        {
            [Required]
            [MinLength(3, ErrorMessage = "Name must have a minimum length of 3")]  
            [MaxLength(5, ErrorMessage = "Name has a maximum length of 5")]  
            public string Name { get; set; }
        }
        
        public class ScheduleRequest5
        {
            // Not required
            [MinLength(3, ErrorMessage = "Name must have a minimum length of 3")]  
            [MaxLength(5, ErrorMessage = "Name has a maximum length of 5")]  
            public string Name { get; set; }
        }
        
        public class ScheduleRequest6
        {
            [Required]
            [DateAsString(ErrorMessage = "StartDate is not a valid DateTime")]  
            public string StartDate { get; set; }
        }
        
        public class ScheduleRequest6a
        {
            [Required]
            [DateAsString]  
            public string StartDate { get; set; }
        }
        
        public class ScheduleRequest7
        {
            [Required]
            [DateAsString(ErrorMessage = "StartDate is not a valid DateTime")]  
            public string StartDate { get; set; }

            [DateAsString(ErrorMessage = "EndDate is not a valid DateTime")] 
            [GreaterThan(nameof(StartDate), ErrorMessage = "EndDate must be greater than StartDate")]
            public string EndDate { get; set; }
        }
        
        public class Automobile 
        {
            public Int16 WheelWidth { get; set; }

            [GreaterThan("WheelWidth", ErrorMessage = "TireWidth must be greater than WheelWidth")]
            public Int16 TireWidth { get; set; }
        }
        
        public class Automobile2 
        {
            [LessThan("TireWidth", ErrorMessage = "WheelWidth must be less than TireWidth")]
            public Int16 WheelWidth { get; set; }

            public Int16 TireWidth { get; set; }
        }
        
        public class Automobile3 
        {
            [LessThanEqual("TireWidth", ErrorMessage = "WheelWidth must be less than or equal to TireWidth")]
            public Int16 WheelWidth { get; set; }

            public Int16 TireWidth { get; set; }
        }

        public class Automobile4
        {
            public Int16 WheelWidth { get; set; }

            [GreaterThanEqual("WheelWidth", ErrorMessage = "TireWidth must be greater than or equal to WheelWidth")]
            public Int16 TireWidth { get; set; }
        }

        public class SomeObject
        {
            [EqualTo("SecondValue", ErrorMessage = "Values must be the same")]
            public long FirstValue { get; set; }

            public long SecondValue { get; set; }
        }

        public class Automobile5 
        {
            public int WheelWidth { get; set; }
            public int TireWidth { get; set; }

            [Required]
            public Engine Engine { get; set; }
        }

        public class Automobile6 
        {
            public int WheelWidth { get; set; }
            public int TireWidth { get; set; }

            [Required]
            [MinCount(4)]
            public IList<Tire> Tires { get; set; }
        }

        public class Engine 
        {
            public double Displacement    { get; set; }

            [LessThan("Displacement", ErrorMessage = "CylinderVolume must be less than total Displacement")]
            public double CylinderVolume  { get; set; }
        }

        public class Tire 
        {
            public double Width    { get; set; }

            [GreaterThan("Width", ErrorMessage = "Height must be greater than Width")]
            public double Height    { get; set; }
        }

        public class Customer 
        {
            [Required]
            public Guid? Id    { get; set; }
        }

        public class Ancestor 
        {
            [LessThanEqual("Dod", ErrorMessage = "Dob must be before Dod")]
            public DateTime Dob    { get; set; }

            public DateTime Dod    { get; set; }
        }

        public class Engine2 
        {
            public string FuelType    { get; set; }

            [RequiredIf("FuelType", "Gas", ErrorMessage = "Carburator is required")]
            public Carburator Carburator  { get; set; }
        }

        public class Engine3 
        {
            public string FuelType    { get; set; }

            [RequiredIfNotEmpty("FuelType", ErrorMessage = "Carburator is required")]
            public Carburator Carburator  { get; set; }
        }

        public class Engine4 
        {
            public string FuelType    { get; set; }

            [RequiredIfEmpty("FuelType", ErrorMessage = "Carburator is required")]
            public Carburator Carburator  { get; set; }
        }

        public class Engine5 
        {
            public string FuelType    { get; set; }

            [RequiredIfEmpty("FuelType", ErrorMessage = "Carburator is required", AllowEmptyStrings = false)]
            public string Carburator  { get; set; }
        }

        public class Carburator 
        {
            public double FlowValveSize    { get; set; }
        }

        public class Person 
        {
            [MinCount(2)]
            [ItemStringLength(300, ErrorMessage = "Hobby name is too long")]
            public string[] Hobbies  { get; set; }
        }

        public class Person2 
        {
            [MinCount(4)]
            [ItemStringLength(300, ErrorMessage = "Hobby name is too long")]
            public string[] Hobbies  { get; set; }
        }

        public class Person3
        {
            [IsOneOf(AllowableValues=new string[] {"Male","Female"})]
            public string Gender { get; set; }

        }

        public class Person4
        {
            [IsOneOfNumeric(AllowableValues = new int[] { 21,22,23,24,25 })]
            public int Age { get; set; }
        }

        public class Person5
        {
            [IsOneOf(AllowableValues = null)]
            public string Gender { get; set; }

            [IsOneOf(AllowableValues = new string[] { "Male", "Female" })]
            public string Gender2 { get; set; }
        }

        public class Person6
        {
            [IsOneOfNumeric(AllowableValues = null)]
            public int Age { get; set; }

            [IsOneOfNumeric(AllowableValues = new int[] { 21, 22, 23, 24, 25 })]
            public object Age2 { get; set; }
        }

        public class TicketTemplate 
        {
            [HtmlContent]
            public string Name { get; set; }

            [Email(ErrorMessage = "Email is invalid")]
            public string Email { get; set; }
        }

        public enum CheckType
        {
            Numeric,
            Total,
            List,
            Date
        }
        
        public class RandomRequest
        {
            [Required(ErrorMessage = "Type is required")]
            public CheckType   Type               { get; set; }

            [RequiredIf("Type", new CheckType[] { CheckType.Total, CheckType.Numeric }, ErrorMessage = "DecimalPlaces is required")]
            [Range(1, 6, ErrorMessage = "DecimalPlaces must be between 1 and 6")]
            public int?        DecimalPlaces      { get; set; }
        }
        
        
        public class RandomRequest2
        {
            [Required(ErrorMessage = "Type is required")]
            public CheckType   Type               { get; set; }

            [RequiredIf("Type", CheckType.Total, ErrorMessage = "DecimalPlaces is required")]
            [RequiredIf("Type", CheckType.Numeric, ErrorMessage = "DecimalPlaces is required")]
            [Range(1, 6, ErrorMessage = "DecimalPlaces must be between 1 and 6")]
            public int?        DecimalPlaces      { get; set; }
        }        public class RandomRequest3
        {
            [Required(ErrorMessage = "Type is required")]
            public CheckType   Type               { get; set; }

            [RequiredIfNot("Type", CheckType.List, ErrorMessage = "DecimalPlaces is required")]
            [Range(1, 6, ErrorMessage = "DecimalPlaces must be between 1 and 6")]
            public int?        DecimalPlaces      { get; set; }
        }
        
        public class RandomRequest4
        {
            [Required(ErrorMessage = "Type is required")]
            public CheckType   Type               { get; set; }

            [RequiredIfNot("Type", new CheckType[] { CheckType.List, CheckType.Date }, ErrorMessage = "DecimalPlaces is required")]
            [Range(1, 6, ErrorMessage = "DecimalPlaces must be between 1 and 6")]
            public int?        DecimalPlaces      { get; set; }
        }

        public class RandomRequest5
        {
            [AlphaNumeric(AllowDash = true)]
            public string Locale { get; set; }

            [AlphaNumeric(AllowDash = false)]
            public string TemplatName { get; set; }
        }


        public class Parent
        {
            [Unique]
            public IList<string>  Children  { get; set; }
        }

        public class Child
        {
            [Required(ErrorMessage = "{0} is required")]
            public string Name { get; set; }
        }

        public class Parent2
        {
            [Unique("Name")]
            public IList<Child>  Children  { get; set; }
        }

        public class Parent3
        {
            [UniqueStringList(",")]
            public string  Children  { get; set; }
        }

        public class Parent4
        {
            public int NumBedrooms { get; set; }

            [UniqueStringListIf("NumBedrooms", 50, ",", ErrorMessage = "Children must have unique values")]
            public string  Children  { get; set; }
        }

        public class NoAnnotations
        {
            public int NumBedrooms { get; set; }
            public string  Children  { get; set; }
        }

        #endregion
    }
}

