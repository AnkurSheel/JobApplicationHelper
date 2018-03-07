using System.Collections;
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
    }
}
