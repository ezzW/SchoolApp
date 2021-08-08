using DomainModels;
using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Infrastructure.EntityFrameWorkCore.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
