using System;
using System.ComponentModel.DataAnnotations;

namespace JAH.DomainModels
{
    public class JobApplication
    {
        public JobApplication()
        {
            Status = Status.Applied;
        }

        [Display(Name = "Company Name")]
        [StringLength(100)]
        [Required(ErrorMessage = "Please enter the company name")]
        public string CompanyName { get; set; }

        [Display(Name = "Application Date")]
        [Required(ErrorMessage = "Please enter the application date")]
        [DataType(DataType.Date)]
        public DateTime ApplicationDate { get; set; }

        [EnumDataType(typeof(Status))]
        public Status Status { get; set; }

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

            return Equals((JobApplication)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = CompanyName != null ? CompanyName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ ApplicationDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (int)Status;
                return hashCode;
            }
        }

        private bool Equals(JobApplication other)
        {
            return string.Equals(CompanyName, other.CompanyName) && ApplicationDate.Equals(other.ApplicationDate) && Status == other.Status;
        }
    }
}
