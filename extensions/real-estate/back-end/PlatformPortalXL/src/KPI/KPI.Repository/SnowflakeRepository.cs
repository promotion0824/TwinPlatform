using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Snowflake.Data.Client;
using Willow.ExceptionHandling.Exceptions;


namespace Willow.KPI.Repository
{
    public class SnowflakeRepository : IQueryRepository
    {
        private readonly string _connectionString;

        public SnowflakeRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Task<IEnumerable<IEnumerable<object>>> Query(string query, object[] parms = null)
        {
            var lines = new List<IEnumerable<object>>();

            try
            { 
                using(IDbConnection conn = new SnowflakeDbConnection())
                {
                    conn.ConnectionString = _connectionString;

                    conn.Open();

                    using IDbCommand cmd = conn.CreateCommand();

                    cmd.CommandText = query;

                    if(parms != null)
                        SetParameters(cmd, parms);

                    using IDataReader reader = cmd.ExecuteReader();

                    var numFields = reader.FieldCount;                    

                    while(reader.Read())
                    {
                        var fields = new List<object>();

                        for(var i = 0; i < numFields; ++i)
                        { 
                            var val = reader.GetString(i);

                            if(double.TryParse(val, out double dVal))
                                fields.Add(dVal);
                            else if(DateTime.TryParse(val, out DateTime dtVal))
                                fields.Add(dtVal.ToString("s")); // Convert back to string so that it doesn't get converted to UTC
                            else
                                fields.Add(val);

                        }

                        lines.Add(fields);
                    }
                }
            }
            catch(SnowflakeDbException ex)
            {
                if(ex.Message.Contains("does not exist", StringComparison.InvariantCultureIgnoreCase))
                    throw new NotFoundException("View not found");
            }

            IEnumerable<IEnumerable<object>> eLines = lines;

            return Task.FromResult(eLines);
        }

        #region Private

        private void SetParameters(IDbCommand cmd, object[] parms)
        {
			for (var index = 0; index < parms.Length; index++)
			{
				SetParameter(cmd, parms[index], index);
			}
        }

        private void SetParameter(IDbCommand cmd, object val, int index)
        {
            var param = cmd.CreateParameter();

            param.ParameterName = (index + 1).ToString();

			if (val == null)
			{
				param.Value = DBNull.Value;
				param.DbType = DbType.String;
			}
			else if (val.GetType() == typeof(DateTime))
            {
                param.Value = (DateTime)val;
                param.DbType = DbType.Date;
            }
			else if(val.GetType() == typeof(int))
            {
                param.Value = (int)val;
                param.DbType = DbType.Int32;
            }
			else if(val.GetType() == typeof(double))
            {
                param.Value = (double)val;
                param.DbType = DbType.Double;
            }
			else if (val.GetType() == typeof(bool))
			{
				param.Value = (bool)val;
				param.DbType = DbType.Boolean;
			}
			else if(DateTime.TryParse(val.ToString(), out DateTime dtValue))
            {
                param.Value = dtValue;
                param.DbType = DbType.Date;
            }
			else if(val.GetType() == typeof(RepositoryNull))
			{
				param.Value = DBNull.Value;
				param.DbType = ((RepositoryNull)val).DbType;
			}
			else
			{ 
                param.Value = val.ToString();
                param.DbType = DbType.String;
            }

            cmd.Parameters.Add(param);
        }

        #endregion
    }
}
