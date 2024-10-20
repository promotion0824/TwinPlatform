using System;
using System.IO;

namespace Willow.Workflow.Models;

public class CreateAttachmentBase
{
	public Guid SiteId { get; set; }
	public Guid TicketId { get; set; }
	public string FileName { get; set; }

	public Guid SourceId { get; set; }
	public TicketSourceType SourceType { get; } = TicketSourceType.Platform;
}

public class CreateByteAttachmentDto : CreateAttachmentBase
{
	public byte[] FileBytes { get; set; }

	public static CreateByteAttachmentDto MapFrom(CreateStreamAttachmentDto dto, byte[] fileBytes)
	{
		return new CreateByteAttachmentDto
		{
			SiteId = dto.SiteId,
			TicketId = dto.TicketId,
			FileName = dto.FileName,
			SourceId = dto.SourceId,
			FileBytes = fileBytes
		};
	}
}


public class CreateStreamAttachmentDto : CreateAttachmentBase
{
	public Stream FileStream { get; set; }
}



