using System;
using System.Data;

namespace Willow.KPI.Repository
{
	public class RepositoryNull
	{
		public DbType DbType { get; set; }

		public DBNull Value { get { return DBNull.Value; } }

		public RepositoryNull(DbType dbType)
		{
			DbType = dbType;
		}
	}

	public static class RepositoryNullTypes
	{
		public static RepositoryNull Boolean => new(DbType.Boolean);
	}
}
