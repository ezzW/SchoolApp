using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace WebAPI.Models
{
    public enum UserRoles
    {
        [EnumMember(Value = "Admin")]
        Admin,
        [EnumMember(Value = "HR")]
        HR,
        [EnumMember(Value = "Staff")]
        Staff
    }
}
