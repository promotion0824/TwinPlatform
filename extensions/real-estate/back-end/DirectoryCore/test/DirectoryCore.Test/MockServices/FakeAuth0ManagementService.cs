using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DirectoryCore.Services.Auth0;

namespace DirectoryCore.Test.MockServices
{
    public class FakeAuth0ManagementService : IAuth0ManagementService
    {
        public class Auth0User
        {
            public Guid Id { get; set; }
            public string Auth0Id { get; set; }
            public string Email { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string InitialPassword { get; set; }
            public string UserType { get; set; }
        }

        public List<Auth0User> CreatedUsers { get; } = new List<Auth0User>();
        public List<string> DeletedAuth0UserIds { get; } = new List<string>();
        public List<string> ChangePasswordEmails { get; } = new List<string>();
        public List<string> ChangedPasswords { get; } = new List<string>();
        public List<string> BlockedAuth0UserIds { get; } = new List<string>();

        public Task<string> CreateUser(
            Guid userId,
            string userEmail,
            string userFirstName,
            string userLastName,
            string initialPassword,
            string userTypeName
        )
        {
            var auth0UserId = Guid.NewGuid().ToString();
            CreatedUsers.Add(
                new Auth0User
                {
                    Id = userId,
                    Auth0Id = auth0UserId,
                    Email = userEmail,
                    FirstName = userFirstName,
                    LastName = userLastName,
                    InitialPassword = initialPassword,
                    UserType = userTypeName
                }
            );
            return Task.FromResult(auth0UserId);
        }

        public Task DeleteUser(string auth0UserId)
        {
            DeletedAuth0UserIds.Add(auth0UserId);
            return Task.FromResult(0);
        }

        public Task ChangeUserPassword(string auth0UserId, string password)
        {
            ChangedPasswords.Add(password);
            return Task.FromResult(0);
        }

        public Task<string> GetAuth0UserId(string userEmail)
        {
            var user = CreatedUsers.FirstOrDefault(u => u.Email.Equals(userEmail));
            var auth0UserId = user?.Auth0Id;
            return Task.FromResult(auth0UserId);
        }

        public Task<(string, Guid)> GetUserInfo(string userEmail)
        {
            throw new NotSupportedException();
        }

        public Task InactivateUser(string auth0UserId)
        {
            BlockedAuth0UserIds.Add(auth0UserId);
            return Task.FromResult(0);
        }
    }
}
