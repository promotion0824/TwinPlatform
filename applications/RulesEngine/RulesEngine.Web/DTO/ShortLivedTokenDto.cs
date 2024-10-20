
namespace WillowRules.DTO;

#pragma warning disable CS8618 // Nullable fields in DTO

/// <summary>
/// Sort-lived token for file download requests
/// </summary>
public class ShortLivedTokenDto
{
	/// <summary>
	/// The crypto random token
	/// </summary>
	public string Token { get; set; }
}

#pragma warning restore CS8618 // Nullable fields in DTO
