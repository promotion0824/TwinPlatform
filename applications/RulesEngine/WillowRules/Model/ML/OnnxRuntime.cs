using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System;
using System.Linq;
using Willow.Expressions;

namespace Willow.Rules.Model;

/// <summary>
/// An ML model implementation using MS ONNX libraries
/// </summary>
public class OnnxRuntime : IMLRuntime
{
	private readonly InferenceSession session;

	/// <summary>
	/// Constructor
	/// </summary>
	public OnnxRuntime(InferenceSession session)
	{
		this.session = session ?? throw new ArgumentNullException(nameof(session));
	}

	public IConvertible Run(IConvertible[][] input)
	{
		var container = new NamedOnnxValue[input.Length];

		int index = 0;

		foreach ((var key, var metadata) in session.InputMetadata)
		{
			if (index < input.Length)
			{
				var inputs = input[index];
				var onnxValue = CreateOnnxValue(metadata, key, inputs);
				container[index] = onnxValue;
			}

			index++;
		}

		using (var results = session.Run(container))
		{
			var elementType = session.OutputMetadata.First().Value.ElementType;

			var result = GetResult(results.First(), elementType);

			return result;
		}
	}

	private static Type Int32Type = typeof(int);
	private static Type Int64Type = typeof(long);
	private static Type SingleType = typeof(float);
	private static Type StringType = typeof(string);

	private static NamedOnnxValue CreateOnnxValue(NodeMetadata metadata, string name, IConvertible[] inputs)
	{
		var dimensions = metadata.Dimensions.Select(v => Math.Abs(v)).ToArray();
		var targetType = metadata.ElementType;

		if (targetType == SingleType)
		{
			return NamedOnnxValue.CreateFromTensor(name, new DenseTensor<float>(inputs.Select(v => v.ToSingle(null)).ToArray(), dimensions));
		}
		else if (targetType == Int64Type)
		{
			return NamedOnnxValue.CreateFromTensor(name, new DenseTensor<long>(inputs.Select(v => v.ToInt64(null)).ToArray(), dimensions));
		}
		else if (targetType == Int32Type)
		{
			return NamedOnnxValue.CreateFromTensor(name, new DenseTensor<int>(inputs.Select(v => v.ToInt32(null)).ToArray(), dimensions));
		}
		else if (targetType == StringType)
		{
			return NamedOnnxValue.CreateFromTensor(name, new DenseTensor<string>(inputs.Select(v => v.ToString(null)).ToArray(), dimensions));
		}

		return NamedOnnxValue.CreateFromTensor(name, new DenseTensor<double>(inputs.Select(v => v.ToDouble(null)).ToArray(), dimensions));
	}

	private static IConvertible GetResult(DisposableNamedOnnxValue result, Type targetType)
	{
		if (targetType == SingleType)
		{
			//floats not handled yet, go to double
			return (double)result.AsTensor<float>().ToArray()[0];
		}
		else if (targetType == Int32Type)
		{
			return result.AsTensor<int>().ToArray()[0];
		}
		else if (targetType == Int64Type)
		{
			return result.AsTensor<long>().ToArray()[0];
		}

		return result.AsTensor<double>().ToArray()[0];
	}
}
