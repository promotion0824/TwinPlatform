using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Newtonsoft.Json;

namespace Willow.Rules.DTO;

/// <summary>
/// Filter specification component used by MUI grid
/// </summary>
[JsonObject]
public class FilterSpecificationDto : IValidatableObject
{
	public const string Contains = "contains";
	public const string NotContains = "notcontains";
	public const string StartsWith = "startsWith";
	public const string EndsWith = "endsWith";
	public const string IsEmpty = "isEmpty";
	public const string IsNotEmpty = "isNotEmpty";
	public const string EqualsLiteral = "equals";
	public const string NotEqualsLiteral = "notequals";
	public const string EqualsShort = "=";
	public const string NotEquals = "!=";
	public const string GreaterThan = ">";
	public const string GreaterThanOrEqual = ">=";
	public const string LessThan = "<";
	public const string LessThanOrEqual = "<=";
	public const string Is = "is";
	public const string Not = "not";
	public const string After = "after";
	public const string OnOrAfter = "onOrAfter";
	public const string Before = "before";
	public const string OnOrBefore = "onOrBefore";

	/// <summary>
	/// field name
	/// </summary>
	[Required]
	public string field { get; set; }

	/// <summary>
	/// "contains", "starts with", "ends with" and "equals"
	/// </summary>
	[Required]
	public string @operator { get; set; }

	/// <summary>
	/// "AND", "OR", defaults to "AND"
	/// </summary>
	public string @logicalOperator { get; set; } = "AND";

	/// <summary>
	/// The value for the filter.
	/// </summary>
	/// <remarks>
	/// Valid values are string, int, double and bool
	/// </remarks>
	public object value { get; set; }

	/// <summary>
	/// Create a <see cref="FilterSpecificationDto" />
	/// </summary>
	public FilterSpecificationDto()
	{
		this.field = string.Empty;
		this.@operator = string.Empty;
		this.value = string.Empty;
	}

	/// <summary>
	/// Validates the value for the <see cref="FilterSpecificationDto"/>
	/// </summary>
	public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
	{
		var supportedOperators = new string[]
		{
			Contains,
			NotContains,
			StartsWith,
			EndsWith,
			EqualsLiteral,
			NotEqualsLiteral,
			EqualsShort,
			NotEquals,
			GreaterThan,
			GreaterThanOrEqual,
			LessThan,
			LessThanOrEqual,
			Is,
			Not,
			IsEmpty,
			IsNotEmpty,
			After,
			OnOrAfter,
			Before,
			OnOrBefore
		};

		if (!supportedOperators.Contains(@operator))
		{
			return new ValidationResult[]
			{
				new ValidationResult($"Unsupported expression of type '{@operator}'. Supported expression are: {string.Join(',', supportedOperators)}")
			};
		}

		var nullValueOperators = new string[]
		{
			IsEmpty,
			IsNotEmpty
		};

		if (value is null && !nullValueOperators.Contains(@operator))
		{
			return new ValidationResult[]
			{
				new ValidationResult($"Value is required")
			};
		}

		if (!(value is IConvertible))
		{
			return new ValidationResult[]
			{
				new ValidationResult($"Unexpected value of type '{value?.GetType().Name}'")
			};
		}

		return Array.Empty<ValidationResult>();
	}

	public bool ToBoolean(IFormatProvider? provider)
	{
		if (value is string stringValue)
		{
			bool.TryParse(stringValue, out var value);
			return value;
		}

		return ((IConvertible)value).ToBoolean(provider);
	}

	public double ToDouble(IFormatProvider? provider)
	{
		if (value is string stringValue)
		{
			double.TryParse(stringValue, out var value);
			return value;
		}

		return ((IConvertible)value).ToDouble(provider);
	}

	public int ToInt32(IFormatProvider? provider)
	{
		if (value is string stringValue)
		{
			int.TryParse(stringValue, out var value);
			return value;
		}

		return ((IConvertible)value).ToInt32(provider);
	}

	public long ToInt64(IFormatProvider? provider)
	{
		if (value is string stringValue)
		{
			long.TryParse(stringValue, out var value);
			return value;
		}
		return ((IConvertible)value).ToInt64(provider);
	}

	public string ToString(IFormatProvider? provider)
	{
		return ((IConvertible)value).ToString(provider);
	}

	public Guid ToGuid(IFormatProvider? provider)
	{
		if (Guid.TryParse(ToString(provider), out var guidValue))
		{
			return guidValue;
		}

		return Guid.Empty;
	}

	/// <summary>
	/// Checks if object value exists in the specified enumeration then converts and returns the value as an enum member
	/// </summary>
	public TEnum ToEnum<TEnum>(IFormatProvider? provider) where TEnum : struct
	{
		if (Enum.TryParse<TEnum>(ToString(provider), out var result))
		{
			return result;
		}

		return default;
	}

	/// <summary>
	/// Converts to date time
	/// </summary>
	public DateTime ToDateTime(IFormatProvider? provider)
	{
		if (DateTime.TryParse(ToString(provider), out var dateValue))
		{
			return dateValue;
		}

		return new DateTime(1970, 1, 1);
	}

	/// <summary>
	/// Converts to date time
	/// </summary>
	public DateTimeOffset ToDateTimeOffset(IFormatProvider? provider)
	{
		if (DateTimeOffset.TryParse(ToString(provider), out var dateValue))
		{
			return dateValue;
		}

		return new DateTimeOffset(new DateTime(1970, 1, 1));
	}
}
