using ChangoMasAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;


namespace ChangoMasAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly ChangoMasDBContext _context;

        public UsuariosController(ChangoMasDBContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("crear")]
        public async Task<IActionResult> CrearUsuario(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            Carrito carrito = new Carrito 
            { 
                IdUsuario = usuario.IdUsuario,
                Estado = 1,
                Total = 0
            };

            await _context.Carritos.AddAsync(carrito);
            await _context.SaveChangesAsync();


            return Ok();
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<IEnumerable<Usuario>>> ListaUsuarios()
        {
            var usuarios = await _context.Usuarios.ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet]
        [Route("ver")]
        public async Task<IActionResult> VerUsuario(int id)
        {
            Usuario usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }
            return Ok(usuario);
        }

        [HttpPut]
        [Route("editar")]
        public async Task<IActionResult> ActualizarUsuario(Usuario usuario)
        {
            var usuarioExistente = await _context.Usuarios.FindAsync(usuario.IdUsuario);

            usuarioExistente!.NombreCompleto = usuario.NombreCompleto;
            usuarioExistente!.Email = usuario.Email;
            usuarioExistente!.Contraseña = usuario.Contraseña;
            usuarioExistente!.Provincia = usuario.Provincia;
            usuarioExistente!.Ciudad = usuario.Ciudad;
            usuarioExistente!.Calle = usuario.Calle;
            usuarioExistente!.CodigoPostal = usuario.CodigoPostal;
            usuarioExistente!.NumeroCalle = usuario.NumeroCalle;
            usuarioExistente!.Departamento = usuario.Departamento;
            usuarioExistente!.IdRol = usuario.IdRol;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("eliminar")]
        public async Task<IActionResult> EliminarUsuario(int id)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var usuarioBorrado = await _context.Usuarios.FindAsync(id);

                    if (usuarioBorrado == null)
                    {
                        return NotFound("Usuario no encontrado.");
                    }

                    var carritoBorrado = await _context.Carritos.FirstOrDefaultAsync(c => c.IdUsuario == id);
                    if (carritoBorrado != null)
                    {
                        _context.Carritos.Remove(carritoBorrado);
                    }

                    _context.Usuarios.Remove(usuarioBorrado);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();
                    return Ok();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, $"Error al eliminar el usuario: {ex.Message}");
                }
            }
        }
    }
}
