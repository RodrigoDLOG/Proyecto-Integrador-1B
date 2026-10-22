-- 05_version_mexico.sql
-- Cambios del "Sistema de Venta Version Mexico" (diapositivas 18 a 29 del profesor).
-- Orden: 01_crear_tablas, 02_datos_iniciales, 03_catalogos_sat, 04_catalogo_claveprodserv y este.
-- Se puede ejecutar varias veces: cada cambio revisa primero si ya esta hecho.
-- Las claves del SAT se guardan tal cual (ej. regimenFiscalReceptor = '616'), como pide la
-- diapositiva 21 para Clientes; asi el XML de la factura se arma sin buscar nada.
USE DBVENTA;
GO

--________________________________ c_Medida_Local (diapositiva 23) ________________________________
-- Unidad de medida propia de la empresa; la del SAT va aparte en c_ClaveUnidad_SAT
IF OBJECT_ID('c_Medida_Local') IS NULL
CREATE TABLE c_Medida_Local (
    idMedidaLocal INT IDENTITY(1,1) PRIMARY KEY,
    descripcion   VARCHAR(50),
    esActivo      BIT,
    fechaRegistro DATETIME DEFAULT GETDATE()
);
GO
IF NOT EXISTS (SELECT 1 FROM c_Medida_Local)
INSERT INTO c_Medida_Local(descripcion, esActivo) VALUES
('Pieza', 1), ('Caja', 1), ('Paquete', 1), ('Kit', 1), ('Servicio', 1);
GO

--________________________________ Negocio (diapositiva 18) ________________________________
-- Agregar RFC, Codigo Postal y Regimen Fiscal
IF COL_LENGTH('Negocio', 'rfc') IS NULL
    ALTER TABLE Negocio ADD rfc VARCHAR(13);
IF COL_LENGTH('Negocio', 'codigoPostal') IS NULL
    ALTER TABLE Negocio ADD codigoPostal VARCHAR(5);
IF COL_LENGTH('Negocio', 'regimenFiscal') IS NULL
    ALTER TABLE Negocio ADD regimenFiscal VARCHAR(3)
        CONSTRAINT FK_Negocio_RegimenFiscal REFERENCES c_RegimenFiscal_SAT(c_RegimenFiscal);
GO
-- 601 = General de Ley Personas Morales (RFC de 12 caracteres)
UPDATE Negocio SET
    rfc           = COALESCE(rfc, NULLIF(numeroDocumento, ''), 'MTI260101AB1'),
    codigoPostal  = COALESCE(codigoPostal, '64000'),
    regimenFiscal = COALESCE(regimenFiscal, '601')
WHERE idNegocio = 1;
GO

--________________________________ Cliente (diapositivas 20 y 21) ________________________________
IF OBJECT_ID('Cliente') IS NULL
CREATE TABLE Cliente (
    idCliente               INT IDENTITY(1,1) PRIMARY KEY,
    nombre                  VARCHAR(80),
    correo                  VARCHAR(50),
    rfc                     VARCHAR(13),
    domicilioFiscalReceptor VARCHAR(5),
    regimenFiscalReceptor   VARCHAR(3),
    esActivo                BIT,
    fechaRegistro           DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Cliente_Rfc UNIQUE (rfc),
    CONSTRAINT FK_Cliente_RegimenFiscal FOREIGN KEY (regimenFiscalReceptor) REFERENCES c_RegimenFiscal_SAT(c_RegimenFiscal)
);
GO
-- Diapositiva 27: sin cliente definido es "Venta a Publico En General" (RFC generico del SAT, regimen 616)
IF NOT EXISTS (SELECT 1 FROM Cliente WHERE rfc = 'XAXX010101000')
INSERT INTO Cliente(nombre, correo, rfc, domicilioFiscalReceptor, regimenFiscalReceptor, esActivo)
VALUES (N'Venta a Público En General', '', 'XAXX010101000', '64000', '616', 1);
GO

--________________________________ Producto (diapositivas 22 a 25) ________________________________
-- precio = Precio Unitario SIN IVA. descuento y tasaImpuesto son porcentajes (15.00, 16.00)
IF COL_LENGTH('Producto', 'idMedidaLocal') IS NULL
    ALTER TABLE Producto ADD idMedidaLocal INT
        CONSTRAINT FK_Producto_MedidaLocal REFERENCES c_Medida_Local(idMedidaLocal);
IF COL_LENGTH('Producto', 'claveUnidad') IS NULL
    ALTER TABLE Producto ADD claveUnidad VARCHAR(3)
        CONSTRAINT FK_Producto_ClaveUnidad REFERENCES c_ClaveUnidad_SAT(c_ClaveUnidad);
IF COL_LENGTH('Producto', 'claveProdServ') IS NULL
    ALTER TABLE Producto ADD claveProdServ VARCHAR(8)
        CONSTRAINT FK_Producto_ClaveProdServ REFERENCES c_ClaveProdServ_SAT(c_ClaveProdServ);
IF COL_LENGTH('Producto', 'descuento') IS NULL
    ALTER TABLE Producto ADD descuento DECIMAL(5,2);
IF COL_LENGTH('Producto', 'objetoImp') IS NULL
    ALTER TABLE Producto ADD objetoImp VARCHAR(2)
        CONSTRAINT FK_Producto_ObjetoImp REFERENCES c_ObjetoImp_SAT(c_ObjetoImpuesto);
IF COL_LENGTH('Producto', 'claveImpuesto') IS NULL
    ALTER TABLE Producto ADD claveImpuesto VARCHAR(3)
        CONSTRAINT FK_Producto_Impuesto REFERENCES c_Impuesto_SAT(c_Impuesto);
IF COL_LENGTH('Producto', 'tasaImpuesto') IS NULL
    ALTER TABLE Producto ADD tasaImpuesto DECIMAL(5,2);
GO
-- Los 10 productos de ejemplo: Pieza / H87, IVA 16 %, objeto de impuesto 02
UPDATE Producto SET
    idMedidaLocal = (SELECT idMedidaLocal FROM c_Medida_Local WHERE descripcion = 'Pieza'),
    claveUnidad   = 'H87',
    claveProdServ = CASE
                        WHEN descripcion LIKE 'Computadora%' THEN '43211507'
                        WHEN descripcion LIKE 'Laptop%'      THEN '43211503'
                        WHEN descripcion LIKE 'Teclado%'     THEN '43211706'
                        WHEN descripcion LIKE 'Monitor%'     THEN '43211902'
                        WHEN descripcion LIKE 'Microfono%'   THEN '52161520'
                        ELSE '01010101'
                    END,
    descuento     = 0,
    objetoImp     = '02',
    claveImpuesto = '002',
    tasaImpuesto  = 16
WHERE claveProdServ IS NULL;
GO
-- El ejemplo de la diapositiva 27 (monitor de 32" con 15 % de descuento)
UPDATE Producto SET descuento = 15 WHERE codigoBarra = '707070' AND descuento = 0;
GO

--________________________________ Venta (diapositivas 26 y 27) ________________________________
-- El RFC ocupa 13 caracteres y el nombre del cliente hasta 80 (antes 10 y 20)
ALTER TABLE Venta ALTER COLUMN documentoCliente VARCHAR(13);
ALTER TABLE Venta ALTER COLUMN nombreCliente VARCHAR(80);
IF COL_LENGTH('Venta', 'idCliente') IS NULL
    ALTER TABLE Venta ADD idCliente INT CONSTRAINT FK_Venta_Cliente REFERENCES Cliente(idCliente);
IF COL_LENGTH('Venta', 'descuentoTotal') IS NULL
    ALTER TABLE Venta ADD descuentoTotal DECIMAL(10,2);
GO

--________________________________ DetalleVenta (diapositivas 26 y 27) ________________________________
-- Por producto: precio unitario, descuento, subtotal, impuesto y total.
--   importe   = cantidad * precio                 (5000 * 1 = 5000)
--   descuento = importe * porcentajeDescuento/100  (5000 * 15/100 = 750)
--   subTotal  = importe - descuento               (5000 - 750 = 4250)
--   impuesto  = subTotal * tasaImpuesto/100       (4250 * 16/100 = 680)
--   total     = subTotal + impuesto               (4250 + 680 = 4930)
-- Las claves SAT se copian del producto al vender, como ya se copian marca y descripcion:
-- si despues se edita el producto, la venta (y su factura) no cambian.
IF COL_LENGTH('DetalleVenta', 'porcentajeDescuento') IS NULL
    ALTER TABLE DetalleVenta ADD porcentajeDescuento DECIMAL(5,2);
IF COL_LENGTH('DetalleVenta', 'descuento') IS NULL
    ALTER TABLE DetalleVenta ADD descuento DECIMAL(10,2);
IF COL_LENGTH('DetalleVenta', 'subTotal') IS NULL
    ALTER TABLE DetalleVenta ADD subTotal DECIMAL(10,2);
IF COL_LENGTH('DetalleVenta', 'tasaImpuesto') IS NULL
    ALTER TABLE DetalleVenta ADD tasaImpuesto DECIMAL(5,2);
IF COL_LENGTH('DetalleVenta', 'impuesto') IS NULL
    ALTER TABLE DetalleVenta ADD impuesto DECIMAL(10,2);
IF COL_LENGTH('DetalleVenta', 'claveProdServ') IS NULL
    ALTER TABLE DetalleVenta ADD claveProdServ VARCHAR(8);
IF COL_LENGTH('DetalleVenta', 'claveUnidad') IS NULL
    ALTER TABLE DetalleVenta ADD claveUnidad VARCHAR(3);
IF COL_LENGTH('DetalleVenta', 'unidad') IS NULL
    ALTER TABLE DetalleVenta ADD unidad VARCHAR(50);
IF COL_LENGTH('DetalleVenta', 'objetoImp') IS NULL
    ALTER TABLE DetalleVenta ADD objetoImp VARCHAR(2);
IF COL_LENGTH('DetalleVenta', 'claveImpuesto') IS NULL
    ALTER TABLE DetalleVenta ADD claveImpuesto VARCHAR(3);
GO

-- Ventas hechas antes de la Version Mexico: ahi el precio ya traia el IVA incluido.
-- Se completan sus columnas nuevas con ese mismo criterio para que Historial y Reporte las muestren.
UPDATE d SET
    porcentajeDescuento = 0,
    descuento           = 0,
    tasaImpuesto        = 16,
    subTotal            = ROUND(d.total / 1.16, 2),
    impuesto            = d.total - ROUND(d.total / 1.16, 2),
    claveProdServ       = p.claveProdServ,
    claveUnidad         = p.claveUnidad,
    unidad              = m.descripcion,
    objetoImp           = p.objetoImp,
    claveImpuesto       = p.claveImpuesto
FROM DetalleVenta d
LEFT JOIN Producto p ON p.idProducto = d.idProducto
LEFT JOIN c_Medida_Local m ON m.idMedidaLocal = p.idMedidaLocal
WHERE d.subTotal IS NULL;
UPDATE Venta SET descuentoTotal = 0 WHERE descuentoTotal IS NULL;
GO

--________________________________ Factura (diapositivas 28 y 29) ________________________________
-- Una factura por venta. Guarda lo que se pide en "Solicitar Factura" y lo que regresa el timbrado.
-- Si el timbrado falla, el registro queda sin uuid y con mensajeError, y se puede reintentar.
IF OBJECT_ID('Factura') IS NULL
CREATE TABLE Factura (
    idFactura             INT IDENTITY(1,1) PRIMARY KEY,
    idVenta               INT NOT NULL,
    usoCFDI               VARCHAR(4),
    regimenFiscalReceptor VARCHAR(3),
    formaPago             VARCHAR(2),
    tipoComprobante       VARCHAR(1),
    metodoPago            VARCHAR(3),
    codigoPostal          VARCHAR(5),
    uuid                  VARCHAR(36),
    folio                 VARCHAR(40),
    fechaTimbrado         DATETIME,
    xmlTimbrado           NVARCHAR(MAX),
    pdfTimbrado           VARBINARY(MAX),
    mensajeError          VARCHAR(500),
    fechaRegistro         DATETIME DEFAULT GETDATE(),
    CONSTRAINT UQ_Factura_Venta UNIQUE (idVenta),
    CONSTRAINT FK_Factura_Venta           FOREIGN KEY (idVenta)               REFERENCES Venta(idVenta),
    CONSTRAINT FK_Factura_UsoCFDI         FOREIGN KEY (usoCFDI)               REFERENCES c_UsoCFDI(c_UsoCFDI),
    CONSTRAINT FK_Factura_RegimenFiscal   FOREIGN KEY (regimenFiscalReceptor) REFERENCES c_RegimenFiscal_SAT(c_RegimenFiscal),
    CONSTRAINT FK_Factura_FormaPago       FOREIGN KEY (formaPago)             REFERENCES c_FormaPago_SAT(c_FormaPago),
    CONSTRAINT FK_Factura_TipoComprobante FOREIGN KEY (tipoComprobante)       REFERENCES c_TipoDeComprobante_SAT(c_TipoDeComprobante),
    CONSTRAINT FK_Factura_MetodoPago      FOREIGN KEY (metodoPago)            REFERENCES c_MetodoPago_SAT(c_MetodoPago)
);
GO

--________________________________ Menu Clientes (diapositiva 20) ________________________________
IF NOT EXISTS (SELECT 1 FROM Menu WHERE controlador = 'Cliente')
INSERT INTO Menu(descripcion, idMenuPadre, controlador, paginaAccion, esActivo)
SELECT 'Clientes', idMenu, 'Cliente', 'Index', 1
FROM Menu WHERE descripcion = 'Administracion' AND controlador IS NULL;
GO
-- Los tres roles: el empleado da de alta al cliente en el mostrador para poder facturarle
INSERT INTO RolMenu(idRol, idMenu, esActivo)
SELECT r.idRol, m.idMenu, 1
FROM Rol r CROSS JOIN Menu m
WHERE m.controlador = 'Cliente'
  AND NOT EXISTS (SELECT 1 FROM RolMenu rm WHERE rm.idRol = r.idRol AND rm.idMenu = m.idMenu);
GO
