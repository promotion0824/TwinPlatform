using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Authorization.TwinPlatform.Persistence.Extensions;

/// <summary>
/// Extension class for defining model property
/// </summary>
public static class BuilderExtensions
{
	public static void UseNewSeqIdasDefault(this PropertyBuilder<Guid> guidPropBuilder)
	{
		guidPropBuilder.HasDefaultValueSql("newsequentialid()");
	}
}

