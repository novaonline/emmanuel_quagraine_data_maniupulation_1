using System;
using System.Collections.Generic;

namespace PatientsParser.Models
{
    public partial class Patients
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? Age { get; set; }
    }
}
