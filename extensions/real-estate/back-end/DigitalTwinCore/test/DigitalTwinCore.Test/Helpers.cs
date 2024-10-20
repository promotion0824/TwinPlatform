using System.Data;
using System.Linq;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace DigitalTwinCore.Test
{
    internal static class Helpers
    {
        public static Mock<IDataReader> CreateDataReader<T>(T[] data)
        {
            var properties = typeof(T).GetProperties();

            var mockDataTable =
                new MockDataTable(properties.Select(x => new MockDataColumn(x.Name, x.PropertyType, !x.IsNonNullableReferenceType())).ToList(),
                    data.Select(row => new MockDataRow(properties.Select(property => property.GetValue(row)).ToArray())).ToList());

            var dataReader = new Mock<IDataReader>();
            
            dataReader.SetupWithReturn(mockDataTable);

            return dataReader;
        }
    }
}
