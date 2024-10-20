using System;

namespace AssetCoreTwinCreator.Models
{
    public class ChangeHistoryRecord
    {
        public string Change { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public string Name { get; set; }
        public DateTime Date { get; set; }
    }
}