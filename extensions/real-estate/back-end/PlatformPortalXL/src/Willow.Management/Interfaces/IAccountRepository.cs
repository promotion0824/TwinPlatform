using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Willow.Management
{
    public interface IAccountRepository
    {
        Task<Account> GetAccount(string emailAddress);
    }
}
