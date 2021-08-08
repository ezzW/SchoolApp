using AutoMapper;
using DomainModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Models;

namespace WebAPI.Mapping
{
    public class MappingProfile:Profile
    {
        public MappingProfile()
        {
            CreateMap<RefreshToken, RefreshTokenModel>();
            CreateMap<RefreshTokenModel,RefreshToken>();

        }
    }

}
