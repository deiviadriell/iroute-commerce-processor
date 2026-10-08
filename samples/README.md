# Archivos de prueba

Cada carpeta es un caso. Cargar el archivo desde la pantalla **Carga de archivo** y, si se acepta, procesar la fecha en **Procesar**.

## Archivos que se aceptan

| Carpeta | Archivo | Al cargar | Al procesar |
|---------|---------|-----------|-------------|
| 01-correcto | commerce_08102026.csv | 8 registros, fecha 08/10/2026 | 08/10/2026 → 0 en cuarentena |
| 02-con-errores-cuarentena | commerce_07102026.csv | 10 registros, fecha 07/10/2026 | 07/10/2026 → 6 en cuarentena |
| 03-separador-punto-y-coma | commerce_09102026.csv | 4 registros, separador `;` y fecha `dd/MM/yyyy` | 09/10/2026 → 2 en cuarentena |
| 04-varias-fechas-proceso | commerce_10102026.csv | 5 registros en 3 fechas | 10/10 → 1 · 11/10 → 1 · 12/10 → 0 |

Detalle de la cuarentena:

| Archivo | Código | pc_numdoc | Motivo |
|---------|--------|-----------|--------|
| 02 | CR2003 | 1791122334001 | Nombre vacío |
| 02 | CR2004 | 17A4567890 | Documento con letras |
| 02 | CR2005 | (vacío) | Documento vacío |
| 02 | CR2006 | 1712-345678 | Documento con caracteres especiales |
| 02 | CR2008 | AB#12 | Nombre vacío (solo espacios) + letras + caracteres especiales |
| 02 | CR2009 | 17 9001 2345 | Documento con caracteres especiales (espacios) |
| 03 | CR3003 | 12A | Documento con letras |
| 03 | CR3004 | 0102030405001 | Nombre vacío |
| 04 | CR4002 | 1711122233 | Nombre vacío (10/10/2026) |
| 04 | CR4004 | 09-1234567 | Documento con caracteres especiales (11/10/2026) |

## Archivos que se rechazan (400)

| Carpeta | Archivo | Mensaje |
|---------|---------|---------|
| 05-archivo-vacio | commerce_13102026.csv | El archivo se encuentra vacío. |
| 06-solo-cabecera | commerce_14102026.csv | El archivo se encuentra vacío. |
| 07-nombre-invalido | comercios_octubre.csv | El archivo debe llamarse commerce_DDMMYYYY.csv con una fecha válida. |
| 08-fecha-invalida-en-nombre | commerce_31022026.csv | El archivo debe llamarse commerce_DDMMYYYY.csv con una fecha válida. (31 de febrero) |
| 09-faltan-columnas | commerce_15102026.csv | Faltan columnas en la cabecera: pc_tipdoc, pc_numdoc, pc_direccion, pc_telefono, pc_email. |
| 10-fecha-proceso-invalida | commerce_16102026.csv | Líneas 3, 4 y 5: pc_processdate inválida. |
| 11-campo-demasiado-largo | commerce_17102026.csv | Línea 3: pc_numdoc supera 30 caracteres. |

Los casos 05, 06, 07 y 08 también los detecta la pantalla antes de enviar (el botón **Enviar** queda deshabilitado).

Cargar el mismo archivo dos veces duplica los registros. Para empezar desde cero:

```sql
USE IRouteCommerce;
TRUNCATE TABLE dbo.commerce;
TRUNCATE TABLE dbo.commerce_quarantine;
```
