namespace Willow.Rules.Model;

/// <summary>
/// Simple statistics result
/// </summary>
public record Stat(double min, double average, double max, string unit)
{
	public static Stat None(string unit) => new Stat(double.NaN, double.NaN, double.NaN, unit);

	public override string ToString()
	{
		return
			double.IsNaN(this.min) ? "no values" :
			max == min ? $"constant {min:0.0}{unit}" : $"min={min:0.0}{unit} ave={average:0.0}{unit} max={max:0.0}{unit}";
	}
}
