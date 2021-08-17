using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public class StudentModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int Age { get; set; }
        public bool Accepted { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
