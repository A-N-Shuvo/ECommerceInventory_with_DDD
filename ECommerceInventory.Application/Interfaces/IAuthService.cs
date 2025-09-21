using ECommerceInventory.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Interfaces
{
    public interface IAuthService
    {
        Task<UserDto?> RegisterAsync(UserRegisterDto dto);
        Task<UserDto?> LoginAsync(UserLoginDto dto);
    }
}
