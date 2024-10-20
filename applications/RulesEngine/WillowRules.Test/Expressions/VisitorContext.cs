using System;
using System.Linq.Expressions;

namespace Willow.Rules.Test.Expressions;

public record TwinMoniker(string TwinId);

public class VisitorContext
{
	public string Id { get; }

	public bool UNDER(TwinMoniker twinMoniker)
	{
		return true;
	}

	public bool IS_OF_MODEL(TwinMoniker twinMoniker, string modelType)
	{
		return true;
	}

	public VisitorContext(string id)
	{
		this.Id = id;
	}

	public static Expression GetterForVariableName(Expression instance, string variableName)
	{
		Expression<Func<VisitorContext, string, object>> getter = (VisitorContext context, string x) => new TwinMoniker(x);
		return getter;
		// // TODO: Use this and the variable name to call a method on self (!)
		// return Expression.Constant(new VisitorContext(variableName + " call " + instance.ToString()));
	}

	// Will have methods to traverse the graph from a Twin etc.

	public override string ToString() => $"Context: {this.Id}";
}
