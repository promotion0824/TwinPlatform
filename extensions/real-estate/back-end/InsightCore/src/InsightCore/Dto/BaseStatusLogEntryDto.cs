using InsightCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace InsightCore.Dto
{
	public class BaseStatusLogEntryDto
	{
		/// <summary>
		/// Status Log Id
		/// </summary>
		public Guid Id { get; set; }
		/// <summary>
		/// Insight Id that this status log belongs to
		/// </summary>
		public Guid InsightId { get; set; }
		/// <summary>
		/// SourceType of the status log
		/// </summary>
		public SourceType? SourceType { get; set; }
		/// <summary>
		/// SourceId of the status log
		/// </summary>
		public Guid? SourceId { get; set; }
        /// <summary>
        /// Source name of the status log
        /// </summary>
        public string SourceName { get; set; }
        /// <summary>
        /// Status of the Insight
        /// </summary>
        public InsightStatus Status { get; set; }
		/// <summary>
		/// CreatedDateTime of the status log
		/// </summary>
		public DateTime CreatedDateTime { get; set; }
		/// <summary>
		/// Reason for the status change
		/// </summary>
		public string Reason { get; set; }

		public static BaseStatusLogEntryDto MapFromModel(StatusLog statusLogEntry, Func<SourceType?, Guid?, string> getSourceName = null)
		{
			if (statusLogEntry == null)
			{
				return null;
			}
            return new BaseStatusLogEntryDto
            {
                Status = statusLogEntry.Status,
                CreatedDateTime = statusLogEntry.CreatedDateTime,
                Reason = statusLogEntry.Reason,
                Id = statusLogEntry.Id,
                SourceType = statusLogEntry.SourceType,
                SourceId = statusLogEntry.SourceId,
                SourceName = getSourceName != null ? getSourceName(statusLogEntry.SourceType, statusLogEntry.SourceId) : (string)null,
				InsightId = statusLogEntry.InsightId
			};
		}
		public static List<BaseStatusLogEntryDto> MapFromModels(List<StatusLog> statusLogEntries, Func<SourceType?, Guid?, string> getSourceName = null)
		{
			return statusLogEntries?.Select(x => MapFromModel(x, getSourceName)).ToList();
		}
	};
}
