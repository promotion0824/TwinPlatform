using System.Threading;
using System.Threading.Tasks;
using Willow.Rules.Model;
using Willow.Rules.Sources;

namespace Willow.Rules.Processor;

/// <summary>
/// Base class for rule processors
/// </summary>
public abstract class RuleProcessorBase
{
	/// <summary>
	/// Get the willow environment for this processor
	/// </summary>
	public abstract WillowEnvironment WillowEnvironment { get; }

	/// <summary>
	/// Handle incoming message
	/// </summary>
	public abstract Task Execute(RuleExecutionRequest request, bool isRealtime, CancellationToken cancellationToken);

	/// <inheritdoc />
	public override string ToString()
	{
		return $"{this.GetType().Name} for {this.WillowEnvironment.Id}";
	}
}
