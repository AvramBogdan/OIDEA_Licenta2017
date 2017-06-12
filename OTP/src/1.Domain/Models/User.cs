using System.Collections.Generic;

namespace OTP.Domain.Models
{   
    /* class used for saving User information in the database */
    public class User : BaseClass
    {   
        /* username */
        public string UserName { get; set; }

        /* first name / last name */ 
        public string CompleteName { get; set; }

        /* user email */
        public string Email { get; set; }

        /* user password */
        public string Password { get; set; }

        /* a user can create multiple projects */
        public ICollection<Project> Projects;
    }


}
