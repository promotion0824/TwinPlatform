using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Willow.ExpressionParser;
using Willow.Expressions;

namespace Willow.Rules.Cache
{
	/// <summary>
	/// Converts token expressions to/from JSON for serialization
	/// </summary>
	public class TokenExpressionJsonConverter : JsonConverter<TokenExpression>
	{
		public override void WriteJson(JsonWriter writer, TokenExpression? value, JsonSerializer serializer)
		{
			var expr = (value as TokenExpression) ?? TokenExpression.Null;
			writer.WriteValue(expr.Serialize());
		}

		public override TokenExpression ReadJson(JsonReader reader, Type objectType, TokenExpression? existingValue, bool hasExistingValue, JsonSerializer serializer)
		{
			TokenExpression result = TokenExpression.Null;

			if (reader.TokenType != JsonToken.Null)
			{
				if (reader.TokenType == JsonToken.StartArray)
				{
					JToken token = JToken.Load(reader);
					List<string> items = token.ToObject<List<string>>()!;
					// not used
				}
				else
				{
					JValue jValue = new JValue(reader.Value);
					switch (reader.TokenType)
					{
						case JsonToken.String:
							{
								try
								{
									result = Parser.Deserialize((string)jValue!);
									break;
								}
								catch (ParserException)
								{
									// Error converting value "OPTION(FAILED([dtmi:com:willowinc:AirHumiditySetpoint;1]))" to type 'Willow.Expressions.TokenExpression'.
									return TokenExpressionConstantString.Create("Could not parse " + (string)jValue!);
								}
							}
						default:
							break;
					}
				}
			}
			return result;
		}

		public override bool CanRead
		{
			get { return true; }
		}
	}
}
