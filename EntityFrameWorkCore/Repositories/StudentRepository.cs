using DomainModels;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.EntityFrameWorkCore.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
