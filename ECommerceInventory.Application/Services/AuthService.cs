using AutoMapper;
using ECommerceInventory.Application.DTOs;
using ECommerceInventory.Application.Interfaces;
using ECommerceInventory.Core.Entities;
using ECommerceInventory.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ECommerceInventory.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AuthService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public async Task<UserDto?> RegisterAsync(UserRegisterDto dto)
        {
            var exists = await _uow.Users.GetByEmailAsync(dto.Email);
            if (exists != null) return null; // already exists

            CreatePasswordHash(dto.Password, out byte[] passwordHash, out byte[] passwordSalt);

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role = string.IsNullOrWhiteSpace(dto.Role) ? "User" : dto.Role
            };

            await _uow.Users.AddAsync(user);
            await _uow.CompleteAsync();

            return _mapper.Map<UserDto>(user);
        }

        public async Task<UserDto?> LoginAsync(UserLoginDto dto)
        {
            var user = await _uow.Users.GetByEmailAsync(dto.Email);
            if (user == null) return null;

            if (!VerifyPasswordHash(dto.Password, user.PasswordHash, user.PasswordSalt))
                return null;

            return _mapper.Map<UserDto>(user);
        }

        // helpers
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                return computed.SequenceEqual(storedHash);
            }
        }
    }
}
