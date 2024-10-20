using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Willow.Directory.Models;

namespace Willow.Communications.Resolvers;
public interface IRecipientResolver
{
    Task<(string Address, string Language)> GetRecipient(Guid customerId, Guid userId, UserType? userType, string notificationType, IDictionary<string, object> data);
}
