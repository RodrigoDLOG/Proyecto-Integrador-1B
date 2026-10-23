-- =====================================================================
-- 02_datos_iniciales.sql  -  Sistema de Venta (FIME-UANL)
-- Ejecutar DESPUES de 01_crear_tablas.sql.
--
-- IDEMPOTENTE: al inicio borra el contenido de todas las tablas (en orden
-- seguro por las llaves foraneas) y reinicia los IDENTITY a 1. Asi se puede
-- correr las veces que haga falta y los ids (menus 1..12, rol 1, etc.)
-- quedan siempre iguales, que es lo que RolMenu y el usuario admin esperan.
-- =====================================================================
USE DBVENTA;
GO

-- ---------------------------------------------------------------------
-- LIMPIEZA (hijos primero, padres despues)
-- Si ya se aplico 05_version_mexico.sql, hay que volver a correrlo despues de este.
-- ---------------------------------------------------------------------
IF OBJECT_ID('Factura') IS NOT NULL DELETE FROM Factura;
DELETE FROM DetalleVenta;
DELETE FROM Venta;
DELETE FROM Producto;
DELETE FROM Categoria;
DELETE FROM RolMenu;
DELETE FROM Menu;
DELETE FROM Usuario;
DELETE FROM Rol;
DELETE FROM TipoDocumentoVenta;
DELETE FROM NumeroCorrelativo;
DELETE FROM Negocio;
DELETE FROM Configuracion;

-- Reinicio de IDENTITY. Ojo: en una tabla que NUNCA tuvo filas, RESEED 0 hace
-- que la primera fila salga con id 0; en una que ya tuvo filas, RESEED 0 da 1.
-- Por eso se mira sys.identity_columns.last_value y se reseedea a 1 o a 0.
DECLARE @t sysname;
DECLARE tablas CURSOR LOCAL FAST_FORWARD FOR
    SELECT name FROM (VALUES ('DetalleVenta'),('Venta'),('Producto'),('Categoria'),
        ('RolMenu'),('Menu'),('Usuario'),('Rol'),('TipoDocumentoVenta'),
        ('NumeroCorrelativo'),('Negocio')) v(name);
OPEN tablas;
FETCH NEXT FROM tablas INTO @t;
WHILE @@FETCH_STATUS = 0
BEGIN
    IF EXISTS (SELECT 1 FROM sys.identity_columns WHERE object_id = OBJECT_ID(@t) AND last_value IS NULL)
        DBCC CHECKIDENT (@t, RESEED, 1) WITH NO_INFOMSGS;
    ELSE
        DBCC CHECKIDENT (@t, RESEED, 0) WITH NO_INFOMSGS;
    FETCH NEXT FROM tablas INTO @t;
END
CLOSE tablas;
DEALLOCATE tablas;
GO

--________________________________ INSERTAR ROLES ________________________________
INSERT INTO Rol(descripcion, esActivo) VALUES
('Administrador', 1),
('Empleado', 1),
('Supervisor', 1);

--________________________________ INSERTAR USUARIO ________________________________
-- clave: 123  (SHA256)
INSERT INTO Usuario(nombre, correo, telefono, idRol, urlFoto, nombreFoto, clave, esActivo) VALUES
('Administrador', 'admin@mitienda.com', '8100000000', 1, '', '',
 'a665a45920422f9d417e4867efdc4fb8a04a1f3fff1fa07e998e86f7f7a27ae3', 1);

--________________________________ CONFIGURACION: SERVICIO DE CORREO ________________________________
-- #####################################################################
-- #                                                                   #
-- #   PON AQUI TU GMAIL Y TU CONTRASENA DE APLICACION                 #
-- #                                                                   #
-- #   'correo' -> tu cuenta de Gmail (ej. mitienda@gmail.com)          #
-- #   'clave'  -> contrasena de APLICACION de Google (16 caracteres),  #
-- #               NO la contrasena normal de la cuenta. Se genera en   #
-- #               Cuenta de Google > Seguridad > Verificacion en dos   #
-- #               pasos > Contrasenas de aplicaciones.                 #
-- #                                                                   #
-- #   Sin esto NO se envian los correos de venta.                     #
-- #                                                                   #
-- #####################################################################
INSERT INTO Configuracion(recurso, propiedad, valor) VALUES
('Servicio_Correo', 'correo', ''),
('Servicio_Correo', 'clave',  ''),
('Servicio_Correo', 'alias',  'MiTienda.com'),
('Servicio_Correo', 'host',   'smtp.gmail.com'),
('Servicio_Correo', 'puerto', '587');

--________________________________ INSERTAR NEGOCIO ________________________________
-- idNegocio es IDENTITY: queda en 1 tras el RESEED.
-- Sin numeroDocumento ni porcentajeImpuesto: 05_version_mexico.sql los elimina de la tabla.
INSERT INTO Negocio(urlLogo, nombreLogo, nombre, correo, direccion, telefono, simboloMoneda)
VALUES ('', '', 'Mi Tienda', '', '', '', '$');

--________________________________ INSERTAR CATEGORIAS ________________________________
INSERT INTO Categoria(descripcion, esActivo) VALUES
('Computadoras', 1),
('Laptops', 1),
('Teclados', 1),
('Monitores', 1),
('Microfonos', 1);

--________________________________ INSERTAR PRODUCTOS ________________________________
-- Imagenes en wwwroot/archivos/producto/
INSERT INTO Producto(codigoBarra, marca, descripcion, idCategoria, stock, urlImagen, nombreImagen, precio, esActivo) VALUES
('101010', 'HALion',   'Computadora PC Core I5',                   (SELECT idCategoria FROM Categoria WHERE descripcion = 'Computadoras'), 15, '/archivos/producto/001_computadora.jpg', '001_computadora.jpg', 1699, 1),
('202020', 'INTEL',    'Computadora PC Core I5, Ram 8gb',          (SELECT idCategoria FROM Categoria WHERE descripcion = 'Computadoras'), 15, '/archivos/producto/002_computadora.jpg', '002_computadora.jpg', 1799, 1),
('303030', 'ASUS',     'Laptop ROG Zephyrus Ryzen 9, Ram 16 gb',   (SELECT idCategoria FROM Categoria WHERE descripcion = 'Laptops'),      10, '/archivos/producto/003_laptop.jpg',      '003_laptop.jpg',      8000, 1),
('404040', 'HUAWEI',   'Laptop Matebook D15, Ram 8 gb',            (SELECT idCategoria FROM Categoria WHERE descripcion = 'Laptops'),      10, '/archivos/producto/004_laptop.jpg',      '004_laptop.jpg',      3599, 1),
('505050', 'REDRAGON', 'Teclado Semi Mecanico Gamer Shiva K12',    (SELECT idCategoria FROM Categoria WHERE descripcion = 'Teclados'),     20, '/archivos/producto/005_teclado.jpg',     '005_teclado.jpg',      159, 1),
('606060', 'CYBERTEL', 'Teclado gamer Xtreme GK100',               (SELECT idCategoria FROM Categoria WHERE descripcion = 'Teclados'),     20, '/archivos/producto/006_teclado.jpg',     '006_teclado.jpg',      250, 1),
('707070', 'SAMSUNG',  'Monitor Mon Gamer de 32. 165HZ',           (SELECT idCategoria FROM Categoria WHERE descripcion = 'Monitores'),    15, '/archivos/producto/007_monitor.jpg',     '007_monitor.jpg',     1399, 1),
('808080', 'GIGABYTE', 'Monitor Gamer G27 165HZ',                  (SELECT idCategoria FROM Categoria WHERE descripcion = 'Monitores'),    15, '/archivos/producto/008_monitor.jpg',     '008_monitor.jpg',     1699, 1),
('909090', 'HYPERX',   'Microfono Quadcast Streaming PC,PS4, PS5', (SELECT idCategoria FROM Categoria WHERE descripcion = 'Microfonos'),   12, '/archivos/producto/009_microfono.jpg',   '009_microfono.jpg',    799, 1),
('100100', 'TRUST',    'Microfono Podcast PC',                     (SELECT idCategoria FROM Categoria WHERE descripcion = 'Microfonos'),   12, '/archivos/producto/010_microfono.jpg',   '010_microfono.jpg',    439, 1);

--________________________________ INSERTAR TIPO DOCUMENTO VENTA ________________________________
-- Mexico: Ticket / Factura (en vez de Boleta)
INSERT INTO TipoDocumentoVenta(descripcion, esActivo) VALUES
('Ticket', 1),
('Factura', 1);

--________________________________ INSERTAR NUMERO CORRELATIVO ________________________________
-- 000001
INSERT INTO NumeroCorrelativo(ultimoNumero, cantidadDigitos, gestion, fechaActualizacion) VALUES
(0, 6, 'venta', GETDATE());

--________________________________ INSERTAR MENUS ________________________________
-- * menu padre (id 1)
INSERT INTO Menu(descripcion, icono, controlador, paginaAccion, esActivo) VALUES
('DashBoard', 'fas fa-fw fa-tachometer-alt', 'DashBoard', 'Index', 1);

-- * menus padre sin pagina (ids 2..5)
INSERT INTO Menu(descripcion, icono, esActivo) VALUES
('Administracion', 'fas fa-fw fa-cog', 1),
('Inventario',     'fas fa-fw fa-clipboard-list', 1),
('Ventas',         'fas fa-fw fa-tags', 1),
('Reportes',       'fas fa-fw fa-chart-area', 1);

-- * hijos Administracion (ids 6,7)
INSERT INTO Menu(descripcion, idMenuPadre, controlador, paginaAccion, esActivo) VALUES
('Usuarios', 2, 'Usuario', 'Index', 1),
('Negocio',  2, 'Negocio', 'Index', 1);

-- * hijos Inventario (ids 8,9)
INSERT INTO Menu(descripcion, idMenuPadre, controlador, paginaAccion, esActivo) VALUES
('Categorias', 3, 'Categoria', 'Index', 1),
('Productos',  3, 'Producto',  'Index', 1);

-- * hijos Ventas (ids 10,11)
INSERT INTO Menu(descripcion, idMenuPadre, controlador, paginaAccion, esActivo) VALUES
('Nueva Venta',     4, 'Venta', 'NuevaVenta', 1),
('Historial Venta', 4, 'Venta', 'HistorialVenta', 1);

-- * hijos Reportes (id 12)
INSERT INTO Menu(descripcion, idMenuPadre, controlador, paginaAccion, esActivo) VALUES
('Reporte de Ventas', 5, 'Reporte', 'Index', 1);

-- los padres se apuntan a si mismos
UPDATE Menu SET idMenuPadre = idMenu WHERE idMenuPadre IS NULL;

--________________________________ INSERTAR ROL MENU ________________________________
-- * Administrador
INSERT INTO RolMenu(idRol, idMenu, esActivo) VALUES
(1, 1, 1),
(1, 6, 1),
(1, 7, 1),
(1, 8, 1),
(1, 9, 1),
(1, 10, 1),
(1, 11, 1),
(1, 12, 1);

-- * Empleado
INSERT INTO RolMenu(idRol, idMenu, esActivo) VALUES
(2, 10, 1),
(2, 11, 1);

-- * Supervisor
INSERT INTO RolMenu(idRol, idMenu, esActivo) VALUES
(3, 8, 1),
(3, 9, 1),
(3, 10, 1),
(3, 11, 1);
GO
