﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace ChangoMasAPI.Models;

public partial class Carrito
{
    public int IdCarrito { get; set; }

    public int IdUsuario { get; set; }

    public int Estado { get; set; }

    public decimal Total { get; set; }

    public DateTime? FechaCreacion { get; set; }

    public virtual ICollection<CarritoProducto> CarritoProductos { get; set; } = new List<CarritoProducto>();

    public virtual Usuario IdUsuarioNavigation { get; set; }
}