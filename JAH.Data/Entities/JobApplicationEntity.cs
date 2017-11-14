using System;

namespace JAH.Data.Entities
{
    public class JobApplicationEntity
    {
        public int Id { get; set; }

        public string CompanyName { get; set; }

        public DateTime ApplicationDate { get; set; }
    }
}