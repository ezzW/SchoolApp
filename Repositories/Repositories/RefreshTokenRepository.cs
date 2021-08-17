using DomainModels;
using Infrastructure.EntityFrameWorkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationContext context) : base(context)
        {
        }
    }
}
