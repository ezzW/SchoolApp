using DomainModels;
using Infrastructure.EntityFrameWorkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Repositories
{
    public class StudentRepository : GenericRepository<Student>, IStudentRepository
    {
        public StudentRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
