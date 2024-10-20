using System;
using System.Linq;

namespace DigitalTwinCore.Dto.Adx;

public class AdxTableSchemaResponse
{
    public string Schema { get; set; }

    public Tuple<string, string>[] GetColumns() =>
        Schema.Split(',').Select(x =>
        {
            var keyValue = x.Split(':');
            return new Tuple<string, string>(keyValue[0], keyValue[1]);
        }).ToArray();
}