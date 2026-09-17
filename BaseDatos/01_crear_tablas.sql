-- =====================================================================
-- Sistema de Venta - Base de datos DBVENTA
-- Script 01: crear base y las 12 tablas del tutorial (video 1).
-- Reconstruido desde SistemaVenta.DAL\DBContext\DBVENTAContext.cs del
-- codigo fuente del curso (el ZIP con "Base de Datos" no vino en Recursos).
-- Ejecutar completo en SSMS (F5) conectado a localhost\SQLEXPRESS.
-- =====================================================================

IF DB_ID('DBVENTA') IS NULL
    CREATE DATABASE DBVENTA;
GO

USE DBVENTA;
GO

-- ---------------------------------------------------------------------
-- Menu: opciones del sistema. Un menu puede tener padre (idMenuPadre),
-- asi se arma el arbol: Administracion > Usuarios, Negocio.
-- controlador + paginaAccion = a donde navega (ej. Usuario / Index).
-- ---------------------------------------------------------------------
CREATE TABLE Menu (
    idMenu        INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(30),
    idMenuPadre   INT,
    icono         VARCHAR(30),
    controlador   VARCHAR(30),
    paginaAccion  VARCHAR(30),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Menu_MenuPadre FOREIGN KEY (idMenuPadre) REFERENCES Menu(idMenu)
);
GO

-- ---------------------------------------------------------------------
-- Rol: Administrador, Empleado, Supervisor...
-- ---------------------------------------------------------------------
CREATE TABLE Rol (
    idRol         INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(30),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- ---------------------------------------------------------------------
-- RolMenu: que menus ve cada rol (relacion muchos a muchos).
-- ---------------------------------------------------------------------
CREATE TABLE RolMenu (
    idRolMenu     INT IDENTITY(1,1) PRIMARY KEY,
    idRol         INT,
    idMenu        INT,
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_RolMenu_Rol  FOREIGN KEY (idRol)  REFERENCES Rol(idRol),
    CONSTRAINT FK_RolMenu_Menu FOREIGN KEY (idMenu) REFERENCES Menu(idMenu)
);
GO

-- ---------------------------------------------------------------------
-- Usuario: quien entra al sistema. Su rol decide que menus ve.
-- clave se guarda como hash SHA256 (nunca en texto plano).
-- ---------------------------------------------------------------------
CREATE TABLE Usuario (
    idUsuario     INT IDENTITY(1,1) PRIMARY KEY,
    nombre        VARCHAR(50),
    correo        VARCHAR(50),
    telefono      VARCHAR(50),
    idRol         INT,
    urlFoto       VARCHAR(500),
    nombreFoto    VARCHAR(100),
    clave         VARCHAR(100),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Usuario_Rol FOREIGN KEY (idRol) REFERENCES Rol(idRol)
);
GO

-- ---------------------------------------------------------------------
-- Categoria de productos (Computadoras, Laptops, Teclados...).
-- ---------------------------------------------------------------------
CREATE TABLE Categoria (
    idCategoria   INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(50),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE()
);
GO

-- ---------------------------------------------------------------------
-- Producto. urlImagen/nombreImagen: en el tutorial la imagen va a
-- Firebase; en nuestra version ira a wwwroot (regla del profesor).
-- ---------------------------------------------------------------------
CREATE TABLE Producto (
    idProducto    INT IDENTITY(1,1) PRIMARY KEY,
    codigoBarra   VARCHAR(50),
    marca         VARCHAR(50),
    descripcion   VARCHAR(100),
    idCategoria   INT,
    stock         INT,
    urlImagen     VARCHAR(500),
    nombreImagen  VARCHAR(100),
    precio        DECIMAL(10,2),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Producto_Categoria FOREIGN KEY (idCategoria) REFERENCES Categoria(idCategoria)
);
GO

-- ---------------------------------------------------------------------
-- NumeroCorrelativo: genera el numero de venta 000001, 000002...
-- ultimoNumero sube en cada venta; cantidadDigitos = ceros a la izquierda;
-- gestion = 'venta' (podria haber 'compra').
-- ---------------------------------------------------------------------
CREATE TABLE NumeroCorrelativo (
    idNumeroCorrelativo INT IDENTITY(1,1) PRIMARY KEY,
    ultimoNumero        INT,
    cantidadDigitos     INT,
    gestion             VARCHAR(100),
    fechaActualizacion  DATETIME
);
GO

-- ---------------------------------------------------------------------
-- TipoDocumentoVenta: Boleta / Factura (Peru).
-- Version Mexico: sera Ticket / Factura, default Ticket.
-- ---------------------------------------------------------------------
CREATE TABLE TipoDocumentoVenta (
    idTipoDocumentoVenta INT IDENTITY(1,1) PRIMARY KEY,
    descripcion          VARCHAR(50),
    esActivo             BIT,
    fechaRegistro        DATETIME DEFAULT GETDATE()
);
GO

-- ---------------------------------------------------------------------
-- Venta: cabecera. Quien vendio (idUsuario), tipo de documento,
-- cliente en texto libre (Version Mexico: sera FK a Clientes por RFC).
-- Ojo: la columna Total va con T mayuscula tal cual el tutorial.
-- ---------------------------------------------------------------------
CREATE TABLE Venta (
    idVenta              INT IDENTITY(1,1) PRIMARY KEY,
    numeroVenta          VARCHAR(6),
    idTipoDocumentoVenta INT,
    idUsuario            INT,
    documentoCliente     VARCHAR(10),
    nombreCliente        VARCHAR(20),
    subTotal             DECIMAL(10,2),
    impuestoTotal        DECIMAL(10,2),
    Total                DECIMAL(10,2),
    fechaRegistro        DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_Venta_TipoDocumentoVenta FOREIGN KEY (idTipoDocumentoVenta) REFERENCES TipoDocumentoVenta(idTipoDocumentoVenta),
    CONSTRAINT FK_Venta_Usuario            FOREIGN KEY (idUsuario)            REFERENCES Usuario(idUsuario)
);
GO

-- ---------------------------------------------------------------------
-- DetalleVenta: lineas de la venta. Guarda marca/descripcion/categoria
-- COPIADAS del producto a proposito: si manana renombran el producto,
-- el ticket de ayer debe seguir diciendo lo que se vendio ayer.
-- Version Mexico: aqui entraran descuento e impuesto por linea.
-- ---------------------------------------------------------------------
CREATE TABLE DetalleVenta (
    idDetalleVenta      INT IDENTITY(1,1) PRIMARY KEY,
    idVenta             INT,
    idProducto          INT,
    marcaProducto       VARCHAR(100),
    descripcionProducto VARCHAR(100),
    categoriaProducto   VARCHAR(100),
    cantidad            INT,
    precio              DECIMAL(10,2),
    total               DECIMAL(10,2),
    CONSTRAINT FK_DetalleVenta_Venta FOREIGN KEY (idVenta) REFERENCES Venta(idVenta)
);
GO

-- ---------------------------------------------------------------------
-- Negocio: datos de la tienda (una sola fila, idNegocio = 1).
-- Version Mexico: quitar numeroDocumento y porcentajeImpuesto;
-- agregar rfc, codigoPostal, regimenFiscal. Por ahora igual al tutorial.
-- ---------------------------------------------------------------------
CREATE TABLE Negocio (
    idNegocio          INT IDENTITY(1,1) PRIMARY KEY,
    urlLogo            VARCHAR(500),
    nombreLogo         VARCHAR(100),
    numeroDocumento    VARCHAR(50),
    nombre             VARCHAR(50),
    correo             VARCHAR(50),
    direccion          VARCHAR(50),
    telefono           VARCHAR(50),
    porcentajeImpuesto DECIMAL(10,2),
    simboloMoneda      VARCHAR(5)
);
GO

-- ---------------------------------------------------------------------
-- Configuracion: credenciales de servicios externos (correo, nube) en
-- filas recurso/propiedad/valor, para cambiarlas sin tocar codigo.
-- Sin llave primaria, tal cual el tutorial.
-- ---------------------------------------------------------------------
CREATE TABLE Configuracion (
    recurso   VARCHAR(50),
    propiedad VARCHAR(50),
    valor     VARCHAR(60)
);
GO

-- Verificacion: deben salir 12 tablas.
SELECT name AS tabla FROM sys.tables ORDER BY name;
SELECT COUNT(*) AS total_tablas FROM sys.tables;
GO

ALTER AUTHORIZATION ON DATABASE::DBVENTA TO sa;