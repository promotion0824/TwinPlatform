using Microsoft.ML.OnnxRuntime;
using System.Linq;

namespace Willow.Rules.Model;

/// <summary>
/// Extensions for ML Farework SDK's
/// </summary>
public static class MLExtensions
{
	/// <summary>
	/// Gets the input array size for tensor based on the product of its dimension
	/// </summary>
	public static int GetArrayInputCount(this NodeMetadata metadata)
	{
		return metadata.Dimensions.Aggregate(1, (acc, val) => acc * val);
	}
}
