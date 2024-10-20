using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WorkflowCore.Entities
{
    [Table("WF_SchemaColumn")]
    public class SchemaColumnEntity
    {
        public Guid Id { get; set; }
        public Guid SchemaId { get; set; }
        public bool IsRequired { get; set; }
        [MaxLength(255)]
        public string Name { get; set; }
        [MaxLength(64)]
        public string DataType { get; set; }
        public bool IsDetail { get; set; }
        [MaxLength(64)]
        public string GroupName { get; set; }
        public string ReferenceColumn { get; set; }
        public int OrderInGroup { get; set; }

        public SchemaEntity Schema { get; set; }
    }
}
