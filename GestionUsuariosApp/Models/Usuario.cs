using System;
using System.ComponentModel.DataAnnotations;

namespace GestionUsuariosApp.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        [StringLength(13)]
        public string TipoDocumento { get; set; } = null!;
        [StringLength(20)]
        public string NumeroDocumento { get; set; } = null!;
        [StringLength(12)]
        public string Contrasena { get; set; } = null!;

        // Perfil
        [StringLength(50)]
        public string Nombres { get; set; } = null!;
        [StringLength(20)]
        public string PrimerApellido { get; set; } = null!;
        [StringLength(20)]
        public string SegundoApellido { get; set; } = null!;
        [StringLength(30)]
        public string? Cargo { get; set; }
        [StringLength(30)]
        public string? Area { get; set; }
        [StringLength(40)]
        public string? Nacionalidad { get; set; }
        [StringLength(20)]
        public string? Sexo { get; set; }
        [StringLength(25)]
        public string? CorreoPrincipal { get; set; }
        [StringLength(20)]
        public string? TelefonoMovil { get; set; }
        [StringLength(20)]
        public string? TipoContratacion { get; set; }
        public DateTime? FechaContratacion { get; set; }

        // Control de Estado y Bloqueos
        public bool Estado { get; set; }
        public int CVF { get; set; } = 0;
        public DateTime? FechaBloqueo { get; set; }
    }
}