using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GestionUsuariosApp.Data;
using GestionUsuariosApp.Models;

namespace GestionUsuariosApp.Controllers
{
    // Asegura que todo el CRUD requiera sesión iniciada por defecto
    [Authorize]
    public class UsuariosController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Flujo de Autenticación y Seguridad

        [AllowAnonymous]
        public IActionResult Activacion()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult Login(bool expired = false)
        {
            // Valida si existe una sesión activa para omitir el login
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                return RedirectToAction(nameof(Details), new { id = userId });
            }

            if (expired)
            {
                ViewBag.SessionExpired = true;
            }

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string tipoDocumento, string numeroDocumento, string contrasena)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.TipoDocumento == tipoDocumento && u.NumeroDocumento == numeroDocumento);

            if (usuario == null)
            {
                ViewBag.ErrorUsuario = "Verifique sus credenciales";
                return View();
            }

            // Validación 1: Verificación de estado (Ahora usando bool, true = Activo, false = Inactivo/Bloqueado)
            if (!usuario.Estado)
            {
                if (usuario.FechaBloqueo.HasValue)
                {
                    // Es un bloqueo temporal por intentos fallidos
                    if (DateTime.Now < usuario.FechaBloqueo.Value.AddMinutes(15))
                    {
                        return RedirectToAction(nameof(CuentaBloqueada));
                    }
                    else
                    {
                        // Expiración del bloqueo temporal: Restauración de credenciales
                        usuario.Estado = true; // Cambiamos a true (Activo)
                        usuario.CVF = 0;
                        usuario.FechaBloqueo = null;
                        _context.Update(usuario);
                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    // Es una cuenta desactivada manualmente o inactiva (sin fecha de bloqueo)
                    ViewBag.ErrorUsuario = "La cuenta está desactivada o bloqueada.";
                    return View();
                }
            }

            // Validación 2: Verificación de credenciales (CVF)
            if (usuario.Contrasena != contrasena)
            {
                usuario.CVF += 1;

                if (usuario.CVF >= 5)
                {
                    usuario.Estado = false; // Cambiamos a false (Bloqueado)
                    usuario.FechaBloqueo = DateTime.Now;
                    _context.Update(usuario);
                    await _context.SaveChangesAsync();

                    // Simulación del envío de correo electrónico de bloqueo (N2)
                    SimularEnvioCorreoBloqueo(usuario);

                    return RedirectToAction(nameof(CuentaBloqueada));
                }

                _context.Update(usuario);
                await _context.SaveChangesAsync();

                ViewBag.ErrorCredenciales = "Verifique sus credenciales";
                return View();
            }

            // Autenticación exitosa: Reinicio de métricas y creación de sesión
            usuario.CVF = 0;
            _context.Update(usuario);
            await _context.SaveChangesAsync();

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
        new Claim(ClaimTypes.Name, $"{usuario.Nombres} {usuario.PrimerApellido}"),
        new Claim("Documento", usuario.NumeroDocumento),
        new Claim("Cargo", usuario.Cargo ?? "Usuario") // Agregué Cargo al claim para usarlo en el Layout
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            return RedirectToAction(nameof(Details), new { id = usuario.Id });
        }
        [AllowAnonymous]
        public IActionResult CuentaBloqueada()
        {
            return View();
        }

        // Método auxiliar para simular el envío de correos (MOCK)
        private void SimularEnvioCorreoBloqueo(Usuario usuario)
        {
            try
            {
                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs", "Emails");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"Bloqueo_{usuario.NumeroDocumento}_{timestamp}.txt";
                var filePath = Path.Combine(directoryPath, fileName);

                var correoDestino = string.IsNullOrEmpty(usuario.CorreoPrincipal) ? "SIN_CORREO" : usuario.CorreoPrincipal;

                var contenidoCorreo = $@"
=================PRUEBA DE SIMULACIÓN=================================
SIMULACIÓN DE ENVÍO DE CORREO - SISTEMA CEPLAN
==================================================
Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
Para: {correoDestino}
Asunto: ALERTA DE SEGURIDAD - Cuenta bloqueada temporalmente
==================================================

Hola {usuario.Nombres},

Le informamos que se ha excedido el número máximo de intentos fallidos (5) al intentar iniciar sesión en su cuenta.
Por motivos de seguridad, su acceso ha sido bloqueado temporalmente durante 15 minutos.

Si usted no intentó iniciar sesión, por favor contacte inmediatamente con el área de soporte técnico.

Atentamente,
El equipo de Seguridad
==================================================";

                System.IO.File.WriteAllText(filePath, contenidoCorreo);
            }
            catch (Exception ex)
            {
                // Solo logueamos en consola para que no interrumpa el flujo del usuario si falla el directorio
                Console.WriteLine($"Error al simular envío de correo: {ex.Message}");
            }
        }

        public async Task<IActionResult> Salir(bool expired = false)
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (expired)
            {
                return RedirectToAction(nameof(Login), new { expired = true });
            }
            return RedirectToAction(nameof(Login));
        }

        #endregion

        #region Módulo CRUD de Usuarios

        public async Task<IActionResult> Index()
        {
            return View(await _context.Usuarios.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FirstOrDefaultAsync(m => m.Id == id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TipoDocumento,NumeroDocumento,Contrasena,Nombres,PrimerApellido,SegundoApellido,Cargo,Area,Nacionalidad,Sexo,CorreoPrincipal,TelefonoMovil,TipoContratacion,FechaContratacion,Estado,CVF,FechaBloqueo")] Usuario usuario)
        {
            if (ModelState.IsValid)
            {
                // Validación: Evitar duplicados por Tipo y Número de Documento
                var existeUsuario = await _context.Usuarios.AnyAsync(u => u.TipoDocumento == usuario.TipoDocumento && u.NumeroDocumento == usuario.NumeroDocumento);
                if (existeUsuario)
                {
                    ModelState.AddModelError("NumeroDocumento", "El documento ya se encuentra registrado.");
                    ViewBag.ErrorMessage = "Ya existe un usuario registrado con este tipo y número de documento.";
                    return View(usuario);
                }

                _context.Add(usuario);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "El usuario se ha registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null) return NotFound();

            return View(usuario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TipoDocumento,NumeroDocumento,Contrasena,Nombres,PrimerApellido,SegundoApellido,Cargo,Area,Nacionalidad,Sexo,CorreoPrincipal,TelefonoMovil,TipoContratacion,FechaContratacion,Estado,CVF,FechaBloqueo")] Usuario usuario)
        {
            if (id != usuario.Id) return NotFound();

            // Hacer que la contraseña sea opcional en la edición
            if (string.IsNullOrEmpty(usuario.Contrasena))
            {
                ModelState.Remove("Contrasena");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // BUSCAMOS EL USUARIO ORIGINAL EN LA BASE DE DATOS
                    var usuarioOriginal = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
                    if (usuarioOriginal == null) return NotFound();

                    // Prevenir manipulación de datos sensibles desde el cliente
                    usuario.TipoDocumento = usuarioOriginal.TipoDocumento;
                    usuario.NumeroDocumento = usuarioOriginal.NumeroDocumento;

                    // Si el administrador activa la cuenta manualmente, reseteamos bloqueos
                    if (usuario.Estado && !usuarioOriginal.Estado)
                    {
                        usuario.CVF = 0;
                        usuario.FechaBloqueo = null;
                    }
                    else
                    {
                        // Preservar los valores reales de BD, no confiar en el formulario
                        usuario.CVF = usuarioOriginal.CVF;
                        usuario.FechaBloqueo = usuarioOriginal.FechaBloqueo;
                    }

                    // Si la contraseña viene vacía, mantenemos la anterior
                    if (string.IsNullOrEmpty(usuario.Contrasena))
                    {
                        usuario.Contrasena = usuarioOriginal.Contrasena;
                    }

                    _context.Update(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id)) return NotFound();
                    throw;
                }
                TempData["MensajeExito"] = "Los datos del usuario se han actualizado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            return View(usuario);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario != null)
            {
                usuario.Estado = false; // Deshabilitar usuario
                _context.Update(usuario);
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "El usuario ha sido desactivado correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateField(int id, string field, string value)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return Json(new { success = false, message = "Usuario no encontrado" });

            if (field == "correoPrincipal")
                usuario.CorreoPrincipal = value;
            else if (field == "telefonoMovil")
                usuario.TelefonoMovil = value;
            else
                return Json(new { success = false, message = "Campo no válido" });

            _context.Update(usuario);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        #endregion

        // ── Extiende la sesión (renueva el ticket de autenticación)
        [HttpPost]
        public async Task<IActionResult> ExtenderSesion()
        {
            // Solo si el usuario está autenticado
            if (!User.Identity?.IsAuthenticated ?? true)
                return Unauthorized();

            // Re-emitir el ticket para renovar la expiración de la cookie
            var principal = HttpContext.User;
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                new AuthenticationProperties
                {
                    IsPersistent = false,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
                    AllowRefresh = true
                });

            return Ok(new { ok = true });
        }

    }


}