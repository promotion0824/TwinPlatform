using AutoFixture;
using FluentAssertions;
using System;
using System.Linq;
using Xunit;

namespace WorkflowCore.Test.Extensions
{
    public class IQueryableExtensions
    {
        public enum InventoryStatus
        {
            InStock, 
            OnOrder,
            Discontinued
        }

        public class InventoryCategory
        {
            public string Name { get; set; }
        }

        public class Inventory
        {
            public InventoryCategory Category { get; set; }
            public int Count { get; set; }
            public DateTime Timestamp { get; set; }
            public InventoryStatus Status { get; set; }
        }

        [Fact]
        public void IQueryableExtensions_OrderByString()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Category.Name";

            var orderedInventory = inventory.OrderBy(x => x.Category.Name).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Name.Should().Be(orderedInventory.ElementAt(i).Category.Name); 
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByStringDesc()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Category.Name desc";

            var orderedInventory = inventory.OrderByDescending(x => x.Category.Name).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Name.Should().Be(orderedInventory.ElementAt(i).Category.Name);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByInt()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Count";

            var orderedInventory = inventory.OrderBy(x => x.Count).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByIntDesc()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Count desc";

            var orderedInventory = inventory.OrderByDescending(x => x.Count).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByDateTime()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Timestamp";

            var orderedInventory = inventory.OrderBy(x => x.Timestamp).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByDateTimeDesc()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Timestamp desc";

            var orderedInventory = inventory.OrderByDescending(x => x.Timestamp).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByEnum()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Status";

            var orderedInventory = inventory.OrderBy(x => x.Status).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByDateEnumDesc()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Status desc";

            var orderedInventory = inventory.OrderByDescending(x => x.Status).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Should().Be(orderedInventory.ElementAt(i).Category);
            }
        }

        [Fact]
        public void IQueryableExtensions_ThenByEnum()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Category.Name desc, Timestamp";

            var orderedInventory = inventory.OrderByDescending(x => x.Category.Name).ThenBy(x => x.Timestamp).ToList();

            var result = inventory.AsQueryable().OrderBy(orderBy);

            for (var i = 0; i < orderedInventory.Count(); i++)
            {
                result.ElementAt(i).Category.Name.Should().Be(orderedInventory.ElementAt(i).Category.Name);
            }
        }

        [Fact]
        public void IQueryableExtensions_OrderByInvalid()
        {
            var inventory = new Fixture().Build<Inventory>().CreateMany(10);

            var orderBy = "Date";

            var exception = Assert.Throws<ArgumentException>(() => inventory.AsQueryable().OrderBy(orderBy));
            Assert.True(exception.Data.Contains("OrderByField"));

        }
    }
}
