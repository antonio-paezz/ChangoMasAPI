using ChangoMasAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;


[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly ChangoMasDBContext _context;
    private readonly string _secretKey; // Define tu clave secreta aquí

    public AuthController(ChangoMasDBContext context, IConfiguration config)
    {
        _context = context;
        _secretKey = config.GetSection("settings").GetSection("secretkey").ToString(); // Cambia esto por una clave segura
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] Usuario request)
    {
        
        // Verifica si el usuario existe
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (usuario == null || usuario.Contraseña != request.Contraseña)
        {
            return StatusCode(StatusCodes.Status401Unauthorized, new {token = ""});
        }
        var keyBytes = Encoding.ASCII.GetBytes(_secretKey);
        var claims = new ClaimsIdentity();

        claims.AddClaim(new Claim(ClaimTypes.NameIdentifier, request.Email));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = claims,
            Expires = DateTime.UtcNow.AddMinutes(5),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenConfig = tokenHandler.CreateToken(tokenDescriptor);

        string tokencreado = tokenHandler.WriteToken(tokenConfig);

        // Aquí puedes devolver información del usuario si lo deseas
        return StatusCode(StatusCodes.Status200OK, new { token = tokencreado, login = usuario });
    }

    
}

