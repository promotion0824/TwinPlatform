using System.Runtime.Serialization;

namespace Authorization.TwinPlatform.Persistence.Exceptions;

[Serializable]
public class EntityNotFoundException : ApplicationException
{
	public object? Id { get; protected set; }
	public string? Name { get; protected set; }

	public EntityNotFoundException(string name, object? id = null) : base($"Entity not found {name}: {id}")
	{
		Id = id;
		Name = name;
	}

	protected EntityNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
	{
		Name = info.GetString(nameof(Name));
		var id = info.GetValue(nameof(Id), typeof(int));
		if (id != null)
		{
			Id = id as int?;
		}
	}
}