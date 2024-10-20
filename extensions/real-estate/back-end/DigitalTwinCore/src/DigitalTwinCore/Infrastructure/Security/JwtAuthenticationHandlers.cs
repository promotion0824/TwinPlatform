using Azure.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System;

namespace DigitalTwinCore.Infrastructure.Security;
public class JwtAuthenticationHandlers(
    IOptionsMonitor<JwtBearerOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : JwtBearerHandler(options, logger, encoder)
{
    //When using multiple JwtBearer schemes we can run into "OnAuthenticationFailed" for instance when logging in via IdentityServer the AuthenticationHandler will still check in these events, this can be ignored...
    //Cfr => https://github.com/dotnet/aspnetcore/issues/13046
    //If you are catching AuthenticationFailed events and using anything but the first AddJwtBearer policy, you may see: Signature validation failed.Unable to match key... This is caused by the system checking each AddJwtBearer in turn until it gets a match. The error can usually be ignored.
    //We managed to fix this issue by adding separate AuthenticationHandlers for each type of bearer token
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            var authorityConfig = await this.Options.ConfigurationManager.GetConfigurationAsync(this.Context.RequestAborted);
            //Determine the issuer from the configuration
            var authorityIssuer = authorityConfig.Issuer;

            var jwtToken = this.ReadTokenFromHeader();

            if (string.IsNullOrEmpty(jwtToken))
            {
                return AuthenticateResult.NoResult();
            }

            var jwtHandler = new JwtSecurityTokenHandler();
            //Check if we can read the token as a valid JWT, if not let the JwtBearerHandler do it's thing...
            if (!jwtHandler.CanReadToken(jwtToken))
                return await base.HandleAuthenticateAsync();

            var token = jwtHandler.ReadJwtToken(jwtToken);
            if (string.Equals(token.Issuer, authorityIssuer, StringComparison.OrdinalIgnoreCase))
            {
                // means the token was issued by this authority, we make sure full validation runs as normal
                return await base.HandleAuthenticateAsync();
            }

            // Skip validation since the token as issued by an issuer that this instance doesn't know about
            // That has zero of success, so we will not issue a "fail" since it crowds the logs with failures of type IDX10501 
            // which are not really true and certainly not useful.
            this.Logger.LogDebug($"Skipping jwt token validation because token issuer was {token.Issuer} but the authority issuer is: {authorityIssuer}");
            return AuthenticateResult.NoResult();
        }
        catch (Exception ex)
        {
            //let the JwtBearerHandler do it's thing...
            return await base.HandleAuthenticateAsync();
        }

    }

    private string ReadTokenFromHeader()
    {
        //Fetch the bearer token from the authorization header on the request!
        if (Request.Headers.TryGetValue("Authorization", out var authorization) &&
            !string.IsNullOrEmpty(authorization) &&
            authorization.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization.ToString().Substring("Bearer ".Length).Trim();
        }

        return null;
    }
}
