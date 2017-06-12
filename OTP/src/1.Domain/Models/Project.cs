using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace OTP.Domain.Models
{

    /* class used for saving information about user projects in the Database */
    public class Project
    {
        /* the project id  == User id */ 
        public Guid ProjectId { get; set; }

        /* project name */ 
        public string Name { get; set; }

        /* path used to find project on local server */
        public string Path { get; set; }

        /* the type of file, ex: python project, c project, cpp project */
        public string Type { get; set; } 

        /* every project can have multiple files */
        public ICollection<File> files { get; set; }

        [ForeignKey("ProjectId")]
        public User User { get; set; }

    }
}
