namespace DirectoryCore.Infrastructure.Configuration
{
    public class AzureADB2CConfiguration
    {
        /// <summary>
        /// Gets or sets the client Id.
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the client secret.
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Gets or sets the Azure Active Directory B2C instance.
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Gets or sets the domain of the Azure Active Directory B2C tenant.
        /// </summary>
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the edit profile policy name.
        /// </summary>
        public string EditProfilePolicyId { get; set; }

        /// <summary>
        /// Gets or sets the sign up or sign in policy name.
        /// </summary>
        public string SignUpSignInPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the reset password policy id.
        /// </summary>
        public string ResetPasswordPolicyId { get; set; }

        /// <summary>
        /// Gets or sets the sign in callback path.
        /// </summary>
        public string CallbackPath { get; set; }

        /// <summary>
        /// Gets or sets the default policy.
        /// </summary>
        public string DefaultPolicy => SignUpSignInPolicyId;

        /// <summary>
        /// Gets or sets the Azure Active Directory B2C tenant ID.
        /// </summary>
        public string TenantId { get; set; }
    }
}
