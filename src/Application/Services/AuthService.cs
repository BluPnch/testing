using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Domain.Interfaces;
using Domain.Interfaces.Repositories;
using Domain.Models;
using Domain.Models.Enums;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Application.Validators;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthUserRepository _authUserRepository;
        private readonly IAdministratorRepository _administratorRepository;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly IClientRepository _clientRepository;
        private readonly ILogger<AuthService> _logger;
        private readonly AuthUserValidator _authUserValidator;

        public AuthService(
            IConfiguration configuration, 
            IAuthUserRepository authUserRepository,
            IAdministratorRepository administratorRepository,
            IEmployeeRepository employeeRepository,
            IClientRepository clientRepository,
            ILogger<AuthService> logger,
            AuthUserValidator authUserValidator)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _authUserRepository = authUserRepository ?? throw new ArgumentNullException(nameof(authUserRepository));
            _administratorRepository = administratorRepository ?? throw new ArgumentNullException(nameof(administratorRepository));
            _employeeRepository = employeeRepository ?? throw new ArgumentNullException(nameof(employeeRepository));
            _clientRepository = clientRepository ?? throw new ArgumentNullException(nameof(clientRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _authUserValidator = authUserValidator ?? throw new ArgumentNullException(nameof(authUserValidator));
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            _logger.LogInformation("Попытка аутентификации пользователя: {Username}", username);
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Попытка аутентификации с пустым именем пользователя");
                throw new ArgumentException("Имя пользователя не может быть пустым", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                _logger.LogWarning("Попытка аутентификации с пустым паролем");
                throw new ArgumentException("Пароль не может быть пустым", nameof(password));
            }

            try
            {
                var user = await _authUserRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с именем {Username} не найден", username);
                    throw new InvalidOperationException("Неверное имя пользователя или пароль");
                }

                if (!VerifyPassword(password, user.PasswordHash))
                {
                    _logger.LogWarning("Неверный пароль для пользователя {Username}", username);
                    throw new InvalidOperationException("Неверное имя пользователя или пароль");
                }

                var token = GenerateJwtToken(user);
                _logger.LogInformation("Успешная аутентификация пользователя {Username}", username);
                return token;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при аутентификации пользователя {Username}", username);
                throw;
            }
        }

        public async Task<AuthUser> RegisterAsync(string email, string password)
        {
            if (await _authUserRepository.UsernameExistsAsync(email))
            {
                throw new InvalidOperationException("Email already exists");
            }

            var authUser = new AuthUser
            {
                Id = Guid.NewGuid(),
                Username = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role = EnumAuth.Client
            };

            await _authUserRepository.CreateAsync(authUser);

            await _clientRepository.CreateClientAsync(
                new Client(
                    id: authUser.Id,
                    companyName: email,
                    phoneNumber: email));

            return authUser;
        }

        public async Task<AuthUser> UpdateUserRoleAsync(Guid userId, EnumAuth newRole, Guid? administratorId = null)
        {
            var authUser = await _authUserRepository.GetByUserIdAsync(userId);
            if (authUser == null)
            {
                throw new InvalidOperationException("Пользователь не найден");
            }

            _logger.LogInformation("Attempting to change role for user {UserId} from {CurrentRole} to {NewRole}", 
                                   userId, authUser.Role, newRole);

            
            if (administratorId.HasValue && administratorId.Value == userId)
            {
                throw new InvalidOperationException("Нельзя изменить роль самому себе");
            }

            
            var existingEmployee = await _employeeRepository.GetEmployeeByIdAsync(userId);
            if (existingEmployee != null)
            {
                throw new InvalidOperationException("Пользователь уже является сотрудником");
            }

            
            if (authUser.Role == EnumAuth.Administrator)
            {
                throw new InvalidOperationException("Нельзя изменить роль администратора");
            }

            if (authUser.Role != EnumAuth.Client)
            {
                throw new InvalidOperationException("Только клиенты могут быть повышены до сотрудников");
            }

            if (newRole != EnumAuth.Employee)
            {
                throw new InvalidOperationException("Можно повысить только до роли Сотрудника");
            }

            if (!administratorId.HasValue)
            {
                throw new InvalidOperationException("ID администратора обязателен для роли Сотрудника");
            }

            
            var administrator = await _authUserRepository.GetByUserIdAsync(administratorId.Value);
            if (administrator == null || administrator.Role != EnumAuth.Administrator)
            {
                throw new InvalidOperationException("Указанный администратор не найден или не имеет соответствующих прав");
            }

            authUser.Role = newRole;
            await _authUserRepository.UpdateAsync(authUser);

            var client = await _clientRepository.GetClientByIdAsync(userId);
            if (client == null)
            {
                throw new InvalidOperationException("Запись клиента не найдена");
            }

            var employee = new Employee(
                id: userId,
                surname: "Employee",
                name: "User",     
                patronymic: null,
                task: "General",
                plantDomain: "General",
                phoneNumber: client.PhoneNumber,
                administratorId: administratorId.Value);

            await _employeeRepository.CreateEmployeeAsync(employee);

            await _clientRepository.DeleteClientAsync(userId);

            _logger.LogInformation("Роль пользователя {UserId} успешно изменена на {NewRole}", userId, newRole);
            
            return authUser;
        }

        private string GenerateJwtToken(AuthUser user)
        {
            _logger.LogInformation("Генерация JWT токена для пользователя: {Username}", user.Username);
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
        
                // Проверить что секретный ключ не null
                var secretKey = _configuration["Jwt:SecretKey"];
                if (string.IsNullOrEmpty(secretKey))
                {
                    throw new InvalidOperationException("JWT secret key is not configured");
                }
        
                var key = Encoding.ASCII.GetBytes(secretKey); // ← теперь secretKey гарантированно не null
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role.ToString())
                    }),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(key),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                _logger.LogInformation("JWT токен успешно сгенерирован для пользователя: {Username}", user.Username);
                return tokenHandler.WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при генерации JWT токена для пользователя: {Username}", user.Username);
                throw;
            }
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при проверке пароля");
                throw;
            }
        }

        public async Task<AuthUser> GetUserByIdAsync(Guid userId)
        {
            var user = await _authUserRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw new Exception("Пользователь не найден");
            }

            return user;
        }

        public async Task<AuthUser> GetUserByUsernameAsync(string username)
        {
            _logger.LogInformation("Получение пользователя по имени: {Username}", username);
            
            if (string.IsNullOrWhiteSpace(username))
            {
                _logger.LogWarning("Попытка получить пользователя с пустым именем");
                throw new ArgumentException("Имя пользователя не может быть пустым", nameof(username));
            }

            try
            {
                var user = await _authUserRepository.GetByUsernameAsync(username);
                if (user == null)
                {
                    _logger.LogWarning("Пользователь с именем {Username} не найден", username);
                    throw new InvalidOperationException("Пользователь не найден");
                }

                _logger.LogInformation("Успешно получен пользователь с именем {Username}", username);
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении пользователя с именем {Username}", username);
                throw;
            }
        }

        public async Task<IEnumerable<AuthUser>> GetAllAuthUsersAsync()
        {
            _logger.LogInformation("Retrieving all auth users");
            try
            {
                var users = await _authUserRepository.GetAllAsync();
                _logger.LogInformation("Successfully retrieved {Count} auth users", users.Count());
                return users;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all auth users");
                throw;
            }
        }
    }
} 