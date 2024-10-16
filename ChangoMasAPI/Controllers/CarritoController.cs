using ChangoMasAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChangoMasAPI.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CarritoController : ControllerBase
    {
        private readonly ChangoMasDBContext _context;

        public CarritoController(ChangoMasDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("lista")]
        public async Task<ActionResult<IEnumerable<Carrito>>> ListaProductos()
        {
            var carritos = await _context.Carritos.ToListAsync();

            return Ok(carritos);
        }

        [HttpPost]
        [Route("agregar")]
        public async Task<ActionResult> AgregarProductoAlCarrito([FromBody] CarritoProductoRequest request)
        {
            // Obtener el carrito del usuario actual
            var carrito = await _context.Carritos
                .FirstOrDefaultAsync(c => c.IdUsuario == request.IdUsuario);

            // Si el carrito no existe, creamos uno nuevo
            if (carrito == null)
            {
                carrito = new Carrito
                {
                    IdUsuario = request.IdUsuario,
                    Estado = 1,
                    Total = 0
                };

                _context.Carritos.Add(carrito);
                await _context.SaveChangesAsync();
            }

            // Obtener el producto de la base de datos
            var producto = await _context.Productos.FindAsync(request.IdProducto);
            if (producto == null)
            {
                return NotFound("Producto no encontrado.");
            }

            // Verificar si hay suficiente stock
            if (producto.Stock < request.Cantidad)
            {
                return BadRequest("No hay suficiente stock para este producto.");
            }

            // Buscar si el producto ya está en el carrito
            var carritoProducto = await _context.CarritoProductos
                .FirstOrDefaultAsync(cp => cp.IdCarrito == carrito.IdCarrito && cp.IdProducto == request.IdProducto);

            if (carritoProducto != null)
            {
                // Si el producto ya está en el carrito, actualizamos la cantidad
                carritoProducto.Cantidad += request.Cantidad;
            }
            else
            {
                // Si el producto no está en el carrito, lo agregamos
                carritoProducto = new CarritoProducto
                {
                    IdCarrito = carrito.IdCarrito,
                    IdProducto = request.IdProducto,
                    Cantidad = request.Cantidad
                };

                _context.CarritoProductos.Add(carritoProducto);
            }

            // Actualizar el total del carrito
            carrito.Total += producto.Precio * request.Cantidad;

            // Reducir el stock del producto
            producto.Stock -= request.Cantidad;

            // Guardar los cambios en la base de datos
            await _context.SaveChangesAsync();

            return Ok("Producto agregado al carrito exitosamente.");
        }

        [HttpDelete]
        [Route("eliminar")]
        public async Task<bool> EliminarCarritoProducto(int idCarrito, int idProducto)
        {
            // Busca el producto en el carrito usando idCarrito y idProducto
            var carritoProducto = await _context.CarritoProductos
                .FirstOrDefaultAsync(cp => cp.IdCarrito == idCarrito && cp.IdProducto == idProducto);

            if (carritoProducto != null)
            {
                // Recupera el producto para incrementar el stock
                var producto = await _context.Productos.FindAsync(carritoProducto.IdProducto);
                if (producto != null)
                {
                    // Incrementa el stock basado en la cantidad en el carrito
                    producto.Stock += carritoProducto.Cantidad;
                }

                // Elimina el producto del carrito
                _context.CarritoProductos.Remove(carritoProducto);
                await _context.SaveChangesAsync();

                return true; // Éxito
            }

            return false; // No se encontró el producto en el carrito
        }


    }
}

