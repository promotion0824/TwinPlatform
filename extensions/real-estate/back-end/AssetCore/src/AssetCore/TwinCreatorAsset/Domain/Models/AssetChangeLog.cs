using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AssetCoreTwinCreator.Domain.Models
{
    public class AssetChangeLog
    {
        public int Id { get; set; }
        public Guid TransactionId { get; set; }
        public int AssetRegisterId { get; set; }
        public string ColumnName { get; set; }
        public string ColumnDisplayName { get; set; }
        public string ValueOld { get; set; }
        public string ValueNew { get; set; }
        public Guid ChangedBy { get; set; }
        public DateTime ChangedOn { get; set; }

        [ForeignKey(nameof(AssetRegisterId))]
        public Asset Asset { get; set; }
    }
}
