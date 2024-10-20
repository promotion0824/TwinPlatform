using System;
using System.Collections.Generic;
using System.Text;

namespace Willow.Api.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class PolicyAttribute : Attribute
    {
        public PolicyAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; private set; }
    }
}
