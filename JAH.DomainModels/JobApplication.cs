using System;

namespace JAH.DomainModels
{
    public class JobApplication
    {
        public string Name { get; set; }

        public DateTime StartDate { get; set; }

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

            return Equals((JobApplication) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ StartDate.GetHashCode();
            }
        }

        protected bool Equals(JobApplication other)
        {
            return string.Equals(Name, other.Name) && StartDate.Equals(other.StartDate);
        }
    }
}
