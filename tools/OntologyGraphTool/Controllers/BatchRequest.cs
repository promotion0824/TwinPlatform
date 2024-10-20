using System.ComponentModel.DataAnnotations;

namespace Willow.Rules.Model;

/// <summary>
/// A batch request
/// </summary>
public class BatchRequestDto
{
    /// <summary>
    /// Specifications on how to sort the batch
    /// </summary>
    public SortSpecificationDto[] SortSpecifications { get; init; } = Array.Empty<SortSpecificationDto>();

    /// <summary>
    /// Specification on how to filter the batch
    /// </summary>
    public FilterSpecificationDto[] FilterSpecifications { get; init; } = Array.Empty<FilterSpecificationDto>();

    /// <summary>
    /// The page number to return for the batch (one-based)
    /// </summary>
    public int? Page { get; init; }

    /// <summary>
    /// The amount of items in the batch
    /// </summary>
    public int? PageSize { get; init; }
}


/// <summary>
/// Sort specification component used by MUI grid
/// </summary>
public class SortSpecificationDto
{
    /// <summary>
    /// field name
    /// </summary>
    public string field { get; set; }

    /// <summary>
    /// "asc", "desc" or empty
    /// </summary>
    public string sort { get; set; }

    /// <summary>
    /// Create a <see cref="SortSpecificationDto" />
    /// </summary>
    public SortSpecificationDto(string field, string sort)
    {
        this.field = field;
        this.sort = sort;
    }

    public SortSpecificationDto() { this.field = ""; this.sort = ""; }
}



/// <summary>
/// Filter specification component used by MUI grid
/// </summary>
public class FilterSpecificationDto : IConvertible
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
    public const string IsNot = "not";

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
            IsNot,
            IsEmpty,
            IsNotEmpty
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

    public TypeCode GetTypeCode()
    {
        return TypeCode.Object;
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

    public short ToInt16(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToInt16 not implemented");
    }

    public byte ToByte(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToByte not implemented");
    }

    public char ToChar(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToChar not implemented");
    }

    public DateTime ToDateTime(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToDateTime not implemented");
    }

    public decimal ToDecimal(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToDecimal not implemented");
    }

    public sbyte ToSByte(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToSByte not implemented");
    }

    public float ToSingle(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToSingle not implemented");
    }

    public object ToType(Type conversionType, IFormatProvider? provider)
    {
        throw new NotImplementedException("ToType not implemented");
    }

    public ushort ToUInt16(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToUInt16 not implemented");
    }

    public uint ToUInt32(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToUInt32 not implemented");
    }

    public ulong ToUInt64(IFormatProvider? provider)
    {
        throw new NotImplementedException("ToUInt64 not implemented");
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
}
