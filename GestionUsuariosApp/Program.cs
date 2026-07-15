using Microsoft.EntityFrameworkCore;
using GestionUsuariosApp.Data;
using Microsoft.AspNetCore.Authentication.Cookies; // Requerido para manejar el Login

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. Inyectar la base de datos (Soluciona el error del generador)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configurar la autenticación de usuarios (Para cumplir el requisito de Figma)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Usuarios/Login"; // Ruta a la que te enviará si no estás logueado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20); // Expiración por inactividad
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. Habilitar la Autenticación (Debe ir SIEMPRE antes de Authorization)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Usuarios}/{action=Activacion}/{id?}");

app.Run();