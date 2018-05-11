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

        public JobApplicationUser Owner { get; set; }

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

            return Equals((JobApplicationEntity)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ApplicationDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (CompanyName != null ? CompanyName.GetHashCode(StringComparison.Ordinal) : 0);
                hashCode = (hashCode * 397) ^ (int)CurrentStatus;
                hashCode = (hashCode * 397) ^ Id;
                hashCode = (hashCode * 397) ^ (Owner != null ? Owner.GetHashCode() : 0);
                return hashCode;
            }
        }

        protected bool Equals(JobApplicationEntity other)
        {
            return ApplicationDate.Equals(other.ApplicationDate)
                   && string.Equals(CompanyName, other.CompanyName, StringComparison.Ordinal)
                   && CurrentStatus == other.CurrentStatus
                   && Id == other.Id
                   && Equals(Owner, other.Owner);
        }
    }
}
