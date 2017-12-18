using System;
using JAH.DomainModels;

namespace JAH.Data.Entities
{
    public class JobApplicationEntity
    {
        public DateTime ApplicationDate { get; set; }

        public string CompanyName { get; set; }

        public Status CurrentStatus { get; set; }

        public int Id { get; set; }

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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }

            return Equals((JobApplicationEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CompanyName != null ? CompanyName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ ApplicationDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)CurrentStatus;
                return hashCode;
            }
        }

        protected bool Equals(JobApplicationEntity other)
        {
            return string.Equals(CompanyName, other.CompanyName) &&
                   ApplicationDate.Equals(other.ApplicationDate) &&
                   CurrentStatus == other.CurrentStatus;
        }
    }
}
