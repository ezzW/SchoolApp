using System;
using System.Collections.Generic;
using System.Text;

namespace DomainModels
{
    public class Student
    {
        public int StudentId { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
