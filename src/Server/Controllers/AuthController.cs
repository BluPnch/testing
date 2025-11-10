using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;
using Server.Controllers.Models;
using Server.Controllers.Converters;
using Swashbuckle.AspNetCore.Annotations;


namespace Server.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Аутентификация пользователя
    /// </summary>
    /// <response code="200">Успешный вход в систему</response>
    /// <response code="401">Неверные учетные данные или ошибка валидации</response>
    /// <response code="500">Ошибка на стороне сервера.</response>
    [HttpPost("login")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(LoginResponseDto), Description = "Успешный вход в систему")]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, Description = "Неверные учетные данные или ошибка валидации")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto requestDto)
    {
        try
        {
            if (requestDto == null)
            {
                return Unauthorized(new { error = "Запрос не может быть пустым" });
            }

            if (string.IsNullOrEmpty(requestDto.Username))
            {
                return Unauthorized(new { error = "Имя пользователя не может быть пустым" });
            }

            if (string.IsNullOrEmpty(requestDto.Password))
            {
                return Unauthorized(new { error = "Пароль не может быть пустым" });
            }

            var token = await _authService.LoginAsync(requestDto.Username, requestDto.Password);
            var user = await _authService.GetUserByUsernameAsync(requestDto.Username);

            LoginResponseDto loginResponseDto = new LoginResponseDto { Token = token, Username = user.Username };

            return Ok(loginResponseDto);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized(new { error = "Неверные учетные данные" });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <response code="201">Пользователь успешно зарегистрирован</response>
    /// <response code="400">Ошибка валидации</response>
    /// <response code="409">Пользователь с таким email уже существует</response>
    [HttpPost("register")]
    [SwaggerResponse(StatusCodes.Status201Created, Type = typeof(LoginResponseDto), Description = "Пользователь успешно зарегистрирован")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, Description = "Ошибка валидации")]
    [SwaggerResponse(StatusCodes.Status409Conflict, Description = "Пользователь с таким email уже существует")]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto requestDto)
    {
        try
        {
            if (requestDto == null)
            {
                return BadRequest(new { error = "Запрос не может быть пустым" });
            }

            if (string.IsNullOrEmpty(requestDto.Email))
            {
                return BadRequest(new { error = "Email не может быть пустым" });
            }

            if (string.IsNullOrEmpty(requestDto.Password))
            {
                return BadRequest(new { error = "Пароль не может быть пустым" });
            }

            var user = await _authService.RegisterAsync(requestDto.Email, requestDto.Password);
            return CreatedAtAction(nameof(Register), new { id = user.Id }, AuthUserConverter.ToDTO(user));
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("уже существует") || ex.Message.Contains("already exists"))
            {
                return Conflict(new { error = ex.Message });
            }
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
}