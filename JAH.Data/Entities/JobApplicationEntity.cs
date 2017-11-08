using System.ComponentModel.DataAnnotations;

namespace JAH.Data.Entities
{
    public class JobApplicationEntity
    {
        public int Id { get; set; }

        [Required]
        public string CompanyName { get; set; }
    }
}