using Authentication;
using DomainModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.EntityFrameWorkCore
{  
    public class ApplicationContext : IdentityDbContext<ApplicationUser>  
    {
        public DbSet<Student> Students { get; set; }
        public virtual DbSet<RefreshToken> RefreshTokens { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)  
        {    
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }

    }  
}                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              