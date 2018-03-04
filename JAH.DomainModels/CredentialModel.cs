using System.ComponentModel.DataAnnotations;

namespace JAH.DomainModels
{
    public class CredentialModel
    {
        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string UserName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 5)]
        public string Password { get; set; }
    }
}
