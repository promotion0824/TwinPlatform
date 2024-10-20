namespace ConnectorCore.Entities.Validators
{
    using System.Collections.Generic;

    internal interface IJsonSchemaValidator
    {
        bool IsValid(ICollection<SchemaColumnEntity> columns, string jsonValue, out IList<string> errorMessages);
    }
}
