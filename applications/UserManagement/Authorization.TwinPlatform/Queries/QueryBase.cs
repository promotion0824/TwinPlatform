namespace Authorization.TwinPlatform.Queries;

public abstract class QueryBase<T> : SqlBase
{
	public string Sql { get; }
	public object? Parameters { get; protected init; }

	public Type ReturnType => typeof(T);

	protected QueryBase(string sql)
	{
		Sql = sql;
	}
}