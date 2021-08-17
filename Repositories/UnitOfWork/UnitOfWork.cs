using Infrastructure.EntityFrameWorkCore;
using Repositories.Interfaces;
using Repositories.Repositories;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationContext _context;
        public UnitOfWork(ApplicationContext context)
        {
            _context = context;
            Students = new StudentRepository(_context);
            RefreshTokens = new RefreshTokenRepository(_context);
        }
        public IStudentRepository Students { get; private set; }
        public IRefreshTokenRepository RefreshTokens { get; private set; }
        public int Complete()
        {
            return _context.SaveChanges();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
