using System;
using System.Collections.Generic;

namespace Willow.Workflow
{
    public class TicketEqualityComparer : IEqualityComparer<Ticket>
    {
        public bool Equals(Ticket x, Ticket y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (ReferenceEquals(x, null)) return false;
            if (ReferenceEquals(y, null)) return false;
            if (x.GetType() != y.GetType()) return false;
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(Ticket obj)
        {
            var hashCode = new HashCode();
            hashCode.Add(obj.Id);
           return hashCode.ToHashCode();
        }
    }
}