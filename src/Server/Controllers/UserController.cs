using Microsoft.AspNetCore.Mvc;
using Domain.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Annotations;
using Server.Controllers.Models;
using Server.Controllers.Converters;



namespace Server.Controllers;
[ApiController]
[Route("api/v1/users")]


public class UserController : ControllerBase
{
    private readonly IAuthService _authService;

    public UserController(IAuthService authService)
    {
        _authService = authService;
    }
    
    /// <summary>
    /// Получение информации о текущем пользователе
    /// </summary>
    /// <response code="200">Информация о пользователе</response>
    /// <response code="400">Ошибка обработки запроса</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="404">Пользователь не найден</response>
    [Authorize]
    [HttpGet("me")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(UserDTO))]
    [SwaggerResponse(StatusCodes.Status400BadRequest)]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status404NotFound)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError, "Ошибка на стороне сервера.")]
    public async Task<IActionResult> GetCurrentUser()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { error = "Пользователь не авторизован" });
            }
    
            var user = await _authService.GetUserByIdAsync(Guid.Parse(userId));
    
            return Ok(new {
                id = user.Id,
                username = user.Username,
                role = user.Role.ToString()
            });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }
    
    /// <summary>
    /// Получение списка всех пользователей (только для администраторов)
    /// </summary>
    /// <response code="200">Список пользователей получен успешно</response>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="403">Недостаточно прав</response>
    /// <response code="500">Ошибка на стороне сервера</response>
    [Authorize(Roles = "Administrator")]
    [HttpGet("")]
    [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(List<AuthUserDTO>))]
    [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    [SwaggerResponse(StatusCodes.Status403Forbidden)]
    [SwaggerResponse(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllAuthUsers()
    {
        try
        {
            var users = await _authService.GetAllAuthUsersAsync();
            return Ok(AuthUserConverter.ToDTO(users));
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
        }
    }

    // [HttpPut("role/{userId}")]
    // [SwaggerResponse(StatusCodes.Status200OK, Type = typeof(User))]
    // [SwaggerResponse(StatusCodes.Status400BadRequest)]
    // [SwaggerResponse(StatusCodes.Status401Unauthorized)]
    // [SwaggerResponse(StatusCodes.Status403Forbidden)]
    // [SwaggerResponse(StatusCodes.Status404NotFound)]
    // [SwaggerResponse(StatusCodes.Status409Conflict)]
    // public async Task<IActionResult> UpdateRole(Guid userId, [FromBody] UpdateRoleRequest request)
    // {
    //     try
    //     {
    //         var user = await _authService.UpdateUserRoleAsync(userId, request.NewRole, request.AdministratorId);
    //         return Ok(user);
    //     }
    //     catch (InvalidOperationException ex)
    //     {
    //         if (ex.Message.Contains("Нельзя изменить роль самому себе") ||
    //             ex.Message.Contains("Пользователь уже является сотрудником") ||
    //             ex.Message.Contains("Нельзя изменить роль администратора") ||
    //             ex.Message.Contains("уже существует") ||
    //             ex.Message.Contains("конфликт"))
    //         {
    //             return Conflict(new { error = ex.Message });
    //         }
    //
    //         if (ex.Message.Contains("не найден") || ex.Message.Contains("not found"))
    //         {
    //             return NotFound(new { error = ex.Message });
    //         }
    //
    //         return BadRequest(new { error = ex.Message });
    //     }
    //     catch (Exception ex)
    //     {
    //         return BadRequest(new { error = ex.Message });
    //     }
    // }
}

// public class UpdateRoleRequest
// {
//     public EnumAuth NewRole { get; set; }
//     public Guid? AdministratorId { get; set; }
// }