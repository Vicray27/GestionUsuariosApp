# Sistema de Gestión de Usuarios

Este proyecto es una aplicación web desarrollada con **ASP.NET Core MVC** como parte de una prueba técnica. Su objetivo principal es gestionar un directorio de usuarios de forma segura, eficiente y con una experiencia de usuario (UX) moderna y adaptativa.

## 🚀 Características Principales

La aplicación implementa flujos avanzados de seguridad y validación, destacando las siguientes funcionalidades:

* **Directorio de Usuarios:** Un dashboard interactivo (CRUD) con búsqueda en tiempo real mediante JavaScript.
* **Sistema de Autenticación:** Login seguro con manejo de sesión mediante cookies.
* **Bloqueo por Intentos Fallidos:** Seguridad integrada que bloquea temporalmente (15 minutos) el acceso a una cuenta si supera los 5 intentos de contraseña fallidos.
* **Control de Inactividad:** Sistema que monitorea el tiempo de inactividad del usuario (20 minutos por defecto). Antes de cerrar la sesión automáticamente, despliega un *Modal* con cuenta regresiva en vivo ofreciendo renovar la sesión.
* **Desactivación Lógica de Usuarios (Soft-Delete):** En lugar de borrar registros de la base de datos, los usuarios se desactivan de forma segura a través de una validación por modal, conservando su historial y perdiendo sus privilegios de acceso.
* **Validaciones Híbridas:** Validaciones del lado del servidor (C# y atributos de modelo) y validaciones instantáneas en el cliente (JavaScript) que impiden caracteres inválidos (ej: ingresar letras en el número de móvil) y proveen feedback inmediato mediante *Toasts*.

## 🛠️ Tecnologías y Herramientas

* **Framework Base:** ASP.NET Core MVC
* **ORM:** Entity Framework Core
* **Base de Datos:** SQL Server
* **Front-end:** HTML5, CSS puro, JavaScript
* **Librerías de UI:** Bootstrap 5, Bootstrap Icons

## ⚙️ Estructura y Patrones de Diseño

El proyecto sigue una arquitectura **MVC** clásica para garantizar limpieza y escalabilidad:
* **Models:** Define la entidad `Usuario` y sus anotaciones.
* **Controllers:** Contiene la lógica de autenticación y de negocio (CRUD) en `UsuariosController.cs`.
* **Views:** Pantallas dinámicas procesadas con *Razor*, complementadas con CSS personalizado para lograr un nivel estético *premium*, empleando gradientes, glassmorfismo y micro-animaciones.

## 💻 Instrucciones de Ejecución Local

Para correr el proyecto en un entorno local, sigue estos pasos:

1. **Clonar el repositorio:**
   ```bash
   git clone https://github.com/Vicray27/GestionUsuariosApp.git
   ```

2. **Configurar la Base de Datos:**
   - Abre el archivo `appsettings.json`.
   - Modifica el string de conexión (`DefaultConnection`) para que apunte a tu servidor de SQL Server local.
   - Restaura el script de la base de datos `GestionUsuariosDB` si dispones del *bak* o *script SQL*, o en su defecto asegúrate de que exista la tabla `Usuarios` con los campos correspondientes.

3. **Ejecutar el proyecto:**
   - Abre la solución `GestionUsuariosApp.sln` con Visual Studio.
   - Restaura los paquetes NuGet.
   - Compila y ejecuta el proyecto (IIS Express o Kestrel).

## 🎨 Vistazo al Diseño

La aplicación se diseñó priorizando la limpieza visual. No se utilizaron constructores visuales por defecto; se implementaron hojas de estilos personalizadas integradas a las clases de Bootstrap para que coincidan con la guía de Figma establecida, brindando *Toasts* flotantes, *Empty States* y botones de gradiente.
