using ChangoMasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Metadata.Ecma335;

namespace ChangoMasAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class ProductosController : ControllerBase
    {
        private readonly ChangoMasDBContext _context;

        public ProductosController(ChangoMasDBContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("crear")]
        public async Task<IActionResult> CrearProducto(Producto producto)
        {
            await _context.Productos.AddAsync(producto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<IEnumerable<Producto>>> ListaProductos() 
        {
            var productos = await _context.Productos.ToListAsync();

            return Ok(productos);
        }

        [HttpGet]
        [Route("ver")]
        public async Task<IActionResult> VerProducto(int id)
        {
            Producto producto = await _context.Productos.FindAsync(id);
            if(producto == null)
            {
                return NotFound();
            }
            return Ok(producto);
        }

        [HttpPut]
        [Route("editar")]
        public async Task<IActionResult> ActualizarProducto(Producto producto) 
        {
            var productoExistente = await _context.Productos.FindAsync(producto.IdProducto);

            productoExistente!.NombreProducto = producto.NombreProducto;
            productoExistente!.Descripcion = producto.Descripcion;
            productoExistente!.Precio = producto.Precio;
            productoExistente!.Stock = producto.Stock;

            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpDelete]
        [Route("eliminar")]
        public async Task<IActionResult> EliminarProducto(int id) 
        {
            var productoBorrado = await _context.Productos.FindAsync(id);

            _context.Productos.Remove(productoBorrado);
            
            await _context.SaveChangesAsync();
             
            return Ok();
        }

        [HttpPost("subirImagen")]
        public async Task<IActionResult> SubirImagen(IFormFile archivo)
        {
            if (archivo == null || archivo.Length == 0)
                return BadRequest("Debe seleccionar una imagen.");

            var rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "imagenes");
            if (!Directory.Exists(rutaCarpeta))
            {
                Directory.CreateDirectory(rutaCarpeta);
            }

            var nombreArchivo = $"{Guid.NewGuid()}_{archivo.FileName}";
            var rutaArchivo = Path.Combine(rutaCarpeta, nombreArchivo);

            using (var stream = new FileStream(rutaArchivo, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            var urlImagen = $"https://localhost:7165/api/Productos/imagenes/{nombreArchivo}";
            return Ok(new { Url = urlImagen });
        }

    }
}
