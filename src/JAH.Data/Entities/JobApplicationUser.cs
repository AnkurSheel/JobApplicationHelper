using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace JAH.Data.Entities
{
    public sealed class JobApplicationUser : IdentityUser
    {
        // needed for EF
        public JobApplicationUser()
        {
        }

        public JobApplicationUser(string email)
            : base(email)
        {
            Email = email;
        }

        public ICollection<JobApplicationEntity> Applications { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((JobApplicationUser)obj);
        }

        public override int GetHashCode()
        {
            var hashCode = Id != null ? Id.GetHashCode(StringComparison.Ordinal) : 0;
            return hashCode;
        }

        public bool Equals(JobApplicationUser other)
        {
            return Id.Equals(other.Id, StringComparison.Ordinal);
        }
    }
}
