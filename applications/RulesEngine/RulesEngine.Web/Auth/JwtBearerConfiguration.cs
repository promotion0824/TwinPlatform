using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace RulesEngine.Web;

/// <summary>
/// Configuration for JwtBearer configuration 
/// </summary>
public static class JwtBearerConfiguration
{
	/// <summary>
	/// Adds the JwtBearer config to the builder
	/// </summary>
	public static AuthenticationBuilder AddJwtBearerConfiguration(this AuthenticationBuilder builder,
		JwtBearerConfig jwtBearerOptions)
	{
		return builder.AddJwtBearer(options =>
		{
			options.Authority = jwtBearerOptions.Authority;
			options.Audience = jwtBearerOptions.Audience;
			options.ClaimsIssuer = jwtBearerOptions.Issuer;
			options.TokenValidationParameters = new TokenValidationParameters()
			{
				ClockSkew = new System.TimeSpan(0, 0, 30),
				IgnoreTrailingSlashWhenValidatingAudience = true,
				ValidIssuer = jwtBearerOptions.Issuer
			};

			options.Events = new JwtBearerEvents()
			{
				OnChallenge = context =>
				{
					context.HandleResponse();
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					context.Response.ContentType = "application/json";

					// Ensure we always have an error and error description.
					if (string.IsNullOrEmpty(context.Error))
						context.Error = "invalid_token";

					if (string.IsNullOrEmpty(context.ErrorDescription))
						context.ErrorDescription = "This request requires a valid JWT access token to be provided";

					// Add some extra context for expired tokens.
					if (context.AuthenticateFailure != null && context.AuthenticateFailure.GetType() == typeof(SecurityTokenExpiredException))
					{
						var authenticationException = context.AuthenticateFailure as SecurityTokenExpiredException;
						context.Response.Headers.Add("x-token-expired", authenticationException.Expires.ToString("o"));
						context.ErrorDescription = $"The token expired on {authenticationException.Expires.ToString("o")}";
					}

					return context.Response.WriteAsync(JsonSerializer.Serialize(new
					{
						error = context.Error,
						error_description = context.ErrorDescription
					}));
				}
			};
		});
	}
}