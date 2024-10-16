using ChangoMasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangoMasAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CarritoProductoController : Controller
    {
        private readonly ChangoMasDBContext _context;

        public CarritoProductoController(ChangoMasDBContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Route("crear")]
        public async Task<IActionResult> CrearCarritoProducto(CarritoProducto carritoProducto)
        {
            await _context.CarritoProductos.AddAsync(carritoProducto);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<IEnumerable<CarritoProducto>>> ListaCarritoProductos()
        {
            var carritos = await _context.CarritoProductos.ToListAsync();

            return Ok(carritos);
        }

        [HttpGet]
        [Route("ver")]
        public async Task<IActionResult> VerProducto(int id)
        {
            CarritoProducto carritoProducto = await _context.CarritoProductos
                .FirstOrDefaultAsync(cp => cp.IdProducto == id); 

            if (carritoProducto == null)
            {
                return NotFound();
            }
            return Ok(carritoProducto);
        }

        [HttpGet]
        [Route("ObtenerProductoCarrito")]
        public async Task<ActionResult<IEnumerable<Producto>>> ObtenerProductoCarrito()
        {
            // Realizamos un Join entre CarritoProductos y Productos para obtener los productos del carrito
            var productos = await (from carrito in _context.CarritoProductos
                                   join producto in _context.Productos
                                   on carrito.IdProducto equals producto.IdProducto
                                   select producto).ToListAsync();

            if (productos.Any())
            {
                return Ok(productos);
            }

            return BadRequest("No se encontraron productos en el carrito.");
        }

        [HttpDelete]
        [Route("eliminar")]
        public async Task<IActionResult> EliminarProductoDeCarrito(int id)
        {
            var productoAEliminar = await _context.CarritoProductos.FirstOrDefaultAsync(p => p.IdProducto == id);

            _context.CarritoProductos.Remove(productoAEliminar);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
