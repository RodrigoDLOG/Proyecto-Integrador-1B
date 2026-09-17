# Sistema de Venta (PIA - FIME UANL)

Aplicación web ASP.NET Core MVC (.NET 10) con SQL Server para gestionar usuarios, categorías, productos, ventas y reportes.

## Requisitos
- Visual Studio 2026 con la carga de trabajo "ASP.NET y desarrollo web"
- .NET 10 SDK
- SQL Server Express (autenticación de Windows) y SQL Server Management Studio (SSMS)

## Puesta en marcha
1. En SSMS ejecuta, en este orden, `BaseDatos/01_crear_tablas.sql` y `BaseDatos/02_datos_iniciales.sql` (el segundo se puede correr varias veces: limpia y vuelve a cargar los datos).
2. Para que se envíen correos, pon tu Gmail y una **contraseña de aplicación** de Google en la tabla `Configuracion` (filas `Servicio_Correo` / `correo` y `clave`). Sin esto la app funciona pero no manda correos.
3. Revisa la cadena de conexión `CadenaSQL` en `SolucionSistemaVenta/SistemaVenta.AplicacionWeb/appsettings.json` (por defecto `Server=localhost\SQLEXPRESS;Database=DBVENTA;Integrated Security=True;TrustServerCertificate=True`).
4. Abre `SolucionSistemaVenta/SolucionSistemaVenta.slnx` en Visual Studio, marca `SistemaVenta.AplicacionWeb` como proyecto de inicio y presiona F5.

## Usuario de prueba
- Correo: `admin@mitienda.com`
- Contraseña: `123`

## Archivos subidos
Las imágenes de productos, fotos de usuario y logo del negocio se guardan localmente en `wwwroot/archivos/` (`producto/`, `usuario/`, `logo/`); no se usa ningún servicio externo.
