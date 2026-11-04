# Sistema de Venta (PIA - FIME UANL)

Aplicación web ASP.NET Core MVC (.NET 10) con SQL Server para gestionar usuarios, categorías, productos, ventas y reportes.

## Requisitos
- Visual Studio 2026 con la carga de trabajo "ASP.NET y desarrollo web"
- .NET 10 SDK
- SQL Server Express (autenticación de Windows) y SQL Server Management Studio (SSMS)

## Puesta en marcha
1. En SSMS ejecuta, en este orden, los scripts de `BaseDatos/`:
   1. `01_crear_tablas.sql`: crea la base `DBVENTA` y las tablas del tutorial.
   2. `02_datos_iniciales.sql`: datos iniciales. Se puede correr varias veces porque limpia y vuelve a cargar; si lo vuelves a correr, corre después el `05`.
   3. `03_catalogos_sat.sql`: catálogos del SAT para CFDI 4.0 (régimen fiscal, uso de CFDI, forma y método de pago, etc.).
   4. `04_catalogo_claveprodserv.sql`: catálogo de claves de producto del SAT (52 mil claves, tarda unos segundos).
   5. `05_version_mexico.sql`: cambios de la Versión México (Negocio, Clientes, Producto, Venta, Factura y la opción Clientes del menú).

   Los scripts 03 y 04 se generan con el Excel de catálogos del SAT que dio el profesor (`catCFDI_V_4_20260212.xls`). Del 03 al 05 se pueden correr varias veces sin duplicar nada.
2. Para que se envíen correos, pon tu Gmail y una **contraseña de aplicación** de Google en la tabla `Configuracion` (filas `Servicio_Correo` / `correo` y `clave`). Sin esto la app funciona pero no manda correos.
3. Revisa la cadena de conexión `CadenaSQL` en `SolucionSistemaVenta/SistemaVenta.AplicacionWeb/appsettings.json` (por defecto `Server=localhost\SQLEXPRESS;Database=DBVENTA;Integrated Security=True;TrustServerCertificate=True`).
4. Abre `SolucionSistemaVenta/SolucionSistemaVenta.slnx` en Visual Studio, marca `SistemaVenta.AplicacionWeb` como proyecto de inicio y presiona F5.

## Usuario de prueba
- Correo: `admin@mitienda.com`
- Contraseña: `123`

## Archivos subidos
Las imágenes de productos, fotos de usuario y logo del negocio se guardan localmente en `wwwroot/archivos/` (`producto/`, `usuario/`, `logo/`); no se usa ningún servicio externo.

## Facturación (timbrado)

Los datos del servicio de timbrado del profesor están en la sección `Timbrado` de
`appsettings.json`. La contraseña de la cuenta `FIME` va en
`SistemaVenta.AplicacionWeb/appsettings.Development.json`, que no se sube a git: copiarla
de `Fime/Form1.cs` del material del profesor si ese archivo no existe.

El certificado HTTPS de `ws.urbansa.com` venció el 19-ene-2026. Con
`AceptarCertificadoVencido: false` el sistema no se conecta y guarda el motivo en la factura.
Hoy esta en `true`: no es produccion, y solo se perdona la fecha vencida.

El servicio no acepta repetir `idLocal` (matricula + numero de venta + intento). Si se reinicia la
base con `02`, los numeros de venta vuelven a empezar y sus intentos ya gastados se rechazan: hay que
reintentar hasta pasar esos numeros o cambiar `PrefijoIdLocal`.
