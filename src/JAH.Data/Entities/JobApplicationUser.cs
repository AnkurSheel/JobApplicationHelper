using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Identity;

namespace JAH.Data.Entities
{
    public class JobApplicationUser : IdentityUser
    {
        /// <inheritdoc />
        public JobApplicationUser()
        {
        }

        /// <inheritdoc />
        public JobApplicationUser(string userName)
            : base(userName)
        {
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
            int hashCode = Id != null ? Id.GetHashCode(StringComparison.Ordinal) : 0;
            return hashCode;
        }

        protected bool Equals(JobApplicationUser other)
        {
            return Id.Equals(other.Id, StringComparison.Ordinal);
        }
    }
}
