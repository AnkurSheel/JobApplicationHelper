namespace JAH.DomainModels
{
    public class JobApplication
    {
        public string Name { get; set; }

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
            return Name != null ? Name.GetHashCode() : 0;
        }

        protected bool Equals(JobApplication other)
        {
            return string.Equals(Name, other.Name);
        }
    }
}
