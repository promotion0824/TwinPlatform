using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities
{
    [Table("WF_Schema")]
    public class SchemaEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }

        public List<SchemaColumnEntity> SchemaColumns { get; set; } = new List<SchemaColumnEntity>();
    }
}
