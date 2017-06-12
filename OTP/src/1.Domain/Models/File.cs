using System.ComponentModel.DataAnnotations.Schema;

namespace OTP.Domain.Models
{   

    /* class used for saving specific files from a specific project in the database */
    public class File
    {   
        /* filename */ 
        string Name { get; set; }

        /* type of file, ex: python file, c file, cpp file  == Project Type*/
        string FileType { get; set; }

        [ForeignKey("FileType")]
        public Project Project { get; set; }
    }
}
