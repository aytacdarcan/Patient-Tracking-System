using HastaTakip.api.Dtos;
using HastaTakip.Api.Data;
using HastaTakip.Api.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HastaTakip.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HastaDbContext _db;

    public AuthController(HastaDbContext db)
    {
        _db = db;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResultDto>> Login(LoginDto dto)
    {
        var user = await _db.Users
            .FirstOrDefaultAsync(x =>
                x.Username == dto.Username &&
                x.Password == dto.Password);

        if (user is null)
        {
            return Unauthorized(new LoginResultDto
            {
                Success = false,
                Message = "Kullanıcı adı veya şifre hatalı."
            });
        }

        return Ok(new LoginResultDto
        {
            Success = true,
            Username = user.Username,
            Role = user.Role,
            Message = "Giriş başarılı."
        });
    }
}