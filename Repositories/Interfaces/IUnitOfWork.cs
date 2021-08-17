using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IStudentRepository Students { get; }
        IRefreshTokenRepository RefreshTokens { get; } 
        int Complete();
    }
}
