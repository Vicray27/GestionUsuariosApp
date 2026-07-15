using System;
using System.ComponentModel.DataAnnotations;

namespace GestionUsuariosApp.Models
{
    public class Usuario
    {
        [Key]
        public int Id { get; set; }
        public string TipoDocumento { get; set; } = null!;
        public string NumeroDocumento { get; set; } = null!;
        public string Contrasena { get; set; } = null!;

        // Perfil
        public string Nombres { get; set; } = null!;
        public string PrimerApellido { get; set; } = null!;
        public string SegundoApellido { get; set; } = null!;
        public string? Cargo { get; set; }
        public string? Area { get; set; }
        public string? Nacionalidad { get; set; }
        public string? Sexo { get; set; }
        public string? CorreoPrincipal { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? TipoContratacion { get; set; }
        public DateTime? FechaContratacion { get; set; }

        // Control de Estado y Bloqueos
        public bool Estado { get; set; }
        public int CVF { get; set; } = 0;
        public DateTime? FechaBloqueo { get; set; }
    }
}