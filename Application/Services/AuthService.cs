using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Application.DTOs.Auth;
using Application.Interfaces;
using Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using BC = BCrypt.Net.BCrypt;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly INotificationService _notificationService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(
            IUserRepository userRepository, 
            IConfiguration configuration,
            INotificationService notificationService,
            ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            
            if (user == null || !BC.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }

            // Actualizar último login
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("User {Username} logged in", user.Username);

            _logger.LogInformation("FCM token: {FcmToken}", request.FcmToken);

            // Manejar FCM token si se proporciona
            if (!string.IsNullOrEmpty(request.FcmToken))
            {
                _logger.LogInformation("Adding FCM token to user {UserId}", user.Id);
                await _notificationService.AddTokenAsync(user.Id, request.FcmToken);
            }

            // Generar token JWT
            var token = GenerateJwtToken(user);

            return new LoginResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                StudentId = user.StudentId,
                HasChangedPassword = user.HasChangedPassword
            };
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // Verificar si el usuario ya existe
            var existingUser = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUser != null)
            {
                return false;
            }

            // Verificar si el correo ya está registrado
            var existingEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingEmail != null)
            {
                return false;
            }

            // Crear nuevo usuario
            var passwordHash = BC.HashPassword(request.Password);
            var user = new User(
                request.Username,
                passwordHash,
                request.Email,
                request.FullName,
                request.Role
            )
            {
                StudentId = request.StudentId,
                HasChangedPassword = false // Por defecto, la contraseña no ha sido cambiada
            };

            await _userRepository.AddAsync(user);
            return true;
        }
        
        public async Task<bool> ChangePasswordAsync(string userId, ChangePasswordRequest request)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            
            if (user == null)
            {
                return false;
            }
            
            // Verificar la contraseña actual
            if (!BC.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return false;
            }
            
            // Actualizar la contraseña
            user.PasswordHash = BC.HashPassword(request.NewPassword);
            user.HasChangedPassword = true;
            
            await _userRepository.UpdateAsync(user);
            return true;
        }
        
        public async Task<bool> AddFcmTokenAsync(string userId, string fcmToken)
        {
            return await _notificationService.AddTokenAsync(userId, fcmToken);
        }
        
        public async Task<bool> RemoveFcmTokenAsync(string userId, string fcmToken)
        {
            return await _notificationService.RemoveTokenAsync(userId, fcmToken);
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSecret = _configuration["JwtSettings:Secret"] 
                            ?? throw new InvalidOperationException("JWT Secret is not configured.");
            
            var key = Encoding.ASCII.GetBytes(jwtSecret);
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("FullName", user.FullName ?? string.Empty)
            };

            // Agregar StudentId solo si no es nulo
            if (!string.IsNullOrEmpty(user.StudentId))
            {
                claims.Add(new Claim(ClaimTypes.Sid, user.StudentId));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["JwtSettings:Issuer"] ?? "https://tuapi.com",
                Audience = _configuration["JwtSettings:Audience"] ?? "https://tucliente.com"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            
            return tokenHandler.WriteToken(token);
        }
    }
}