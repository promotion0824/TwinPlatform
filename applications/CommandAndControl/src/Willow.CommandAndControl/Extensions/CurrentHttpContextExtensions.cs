namespace Willow.CommandAndControl.Extensions
{
    /// <summary>
    /// Extensions for the <see cref="ICurrentHttpContext"/> interface.
    /// </summary>
    public static class CurrentHttpContextExtensions
    {
        /// <summary>
        /// Gets the full name of the user from the bearer token.
        /// </summary>
        /// <param name="currentHttpContext">The current context.</param>
        /// <returns>The user's name.</returns>
        public static string GetFullName(this ICurrentHttpContext currentHttpContext)
        {
            // Assuming the token is JWT and it has a claim named "name"
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(currentHttpContext.BearerToken);
            var nameClaim = token.Claims.First(claim => claim.Type == "name");
            return nameClaim.Value;
        }
    }
}
