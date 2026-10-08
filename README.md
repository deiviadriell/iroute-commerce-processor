# IRoute Commerce

Carga, validación y cuarentena de comercios desde archivos `commerce_DDMMYYYY.csv`.

| Capa | Tecnología |
|------|------------|
| Front End | Angular 20 (standalone, signals, lazy routes) + Bootstrap 5 |
| Back End | .NET 10 Web API, arquitectura hexagonal |
| Base de datos | SQL Server (stored procedures + table-valued parameter) |

## Acceso

> [!IMPORTANT]
> **Credenciales para ingresar al sistema**
>
> | Usuario | Contraseña |
> |:-------:|:----------:|
> | **`admin`** | **`Admin123*`** |
>
> Front End: http://localhost:4200 · Swagger: http://localhost:5103/swagger

## Estructura

```
database/                     Scripts SQL en orden de ejecución + instalar.bat / install.ps1
backend/
  src/IRoute.Commerce.Domain
    Entities/                         Commerce, QuarantinedCommerce, ProcessDateSummary, User
  src/IRoute.Commerce.Application
    Abstractions/                     Puertos: ICommerceRepository, IUserRepository, ICommerceFileReader,
                                      IPasswordHasher, ITokenService
    Commerces/                        CommerceService + UploadResult / ProcessResult
    Auth/                             AuthService + LoginResult
  src/IRoute.Commerce.Infrastructure  Adaptadores
    Persistence/                      CommerceRepository, UserRepository (ADO.NET + stored procedures)
    Files/                            CsvCommerceFileReader
    Security/                         PasswordHasher (PBKDF2), TokenService (JWT)
  src/IRoute.Commerce.Api
    Controllers/                      CommerceController, AuthController
    Program.cs                        Registro de dependencias, JWT, CORS, Swagger
  tests/IRoute.Commerce.Tests/Unit    xUnit + NSubstitute
frontend/
  src/app/core                        Modelos, servicio HTTP, utilidades
  src/app/features                    upload · process · quarantine
samples/                            Archivos de prueba por caso (ver samples/README.md)
```

## Requisitos

- .NET SDK 10
- Node 20+ y Angular CLI 20
- SQL Server local (por defecto `.\SQLEXPRESS` con autenticación de Windows)
- `sqlcmd` (herramientas de línea de comandos de SQL Server)

## Orden para levantar el proyecto

| Paso | Qué | Cuándo |
|------|-----|--------|
| 1 | Base de datos | Solo la primera vez |
| 2 | API | Cada vez |
| 3 | Front End | Cada vez |

La API no crea la base de datos: sin el paso 1 el login y todas las pantallas fallan.

## 1. Base de datos

Doble clic en **`database/instalar.bat`**.

1. Pregunta el servidor: presionar **Enter** para usar `.\SQLEXPRESS` o escribir otro (por ejemplo `localhost` o `.\MSSQLSERVER01`).
2. Ejecuta los scripts en orden y termina con *Base de datos IRouteCommerce instalada correctamente.*

Se puede ejecutar varias veces: no borra datos ni duplica objetos.

Desde una terminal también se puede ejecutar:

```powershell
.\database\install.ps1 -Server ".\SQLEXPRESS"
```

| Script | Contenido |
|--------|-----------|
| 01_create_database.sql | Base `IRouteCommerce` |
| 02_create_tables.sql | Tablas `commerce`, `commerce_quarantine` y tipo `commerce_type` |
| 03_alter_commerce_quarantine_add_motivo.sql | `ALTER TABLE` que agrega la columna `motivo` |
| 04_sp_create_commerce.sql | Inserta los registros del archivo |
| 05_sp_process_commerce.sql | Valida por `pc_processdate`, mueve a cuarentena y retorna la cantidad |
| 06_sp_list_commerce_quarantine.sql | Lista la cuarentena (filtro opcional por fecha) |
| 07_sp_list_commerce_process_dates.sql | Fechas cargadas en `commerce` |
| 08_create_users.sql | Tabla `users` y `sp_get_user_by_username` |
| 09_seed_users.sql | Usuario de prueba |

## 2. API

La cadena de conexión está en `ConnectionStrings:Commerce` de `backend/src/IRoute.Commerce.Api/appsettings.json`.

```powershell
cd backend
dotnet test
dotnet run --project src/IRoute.Commerce.Api --launch-profile http
```

Swagger: http://localhost:5103/swagger (botón *Authorize* para enviar el token)

Todos los endpoints de `/api/commerce` requieren `Authorization: Bearer <token>`.

Credenciales: ver [Acceso](#acceso).

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/auth/login` | `{ username, password }`. Retorna el JWT. 401 si las credenciales son incorrectas |
| POST | `/api/commerce/upload` | `multipart/form-data` con `file`. 400 si el archivo está vacío, el nombre no cumple `commerce_DDMMYYYY.csv` o el formato es inválido. 201 con la cantidad insertada |
| POST | `/api/commerce/process` | `{ "processDate": "2026-10-07" }`. Retorna `{ processDate, quarantined }` |
| GET | `/api/commerce/quarantine?processDate=` | Lista `commerce_quarantine` |
| GET | `/api/commerce/process-dates` | Fechas disponibles en `commerce` |

## 3. Front End

```powershell
cd frontend
npm install
npx ng serve
```

http://localhost:4200 — la URL del API está en `src/environments/environment.ts`.

0. **Inicio de sesión**: guard de rutas, interceptor que agrega el token y redirige al login ante un 401.
1. **Carga de archivo**: selección del archivo, validación del nombre, vista previa paginada marcando los registros que irían a cuarentena y envío al API.
2. **Procesar**: selección de la fecha `pc_processdate` y ejecución de la validación.
3. **Registros con errores**: listado de la cuarentena con filtro por fecha y búsqueda.

## Formato del CSV

Cabecera obligatoria (delimitador `,` `;` `|` o tabulación, detectado automáticamente):

```
pc_processdate,pc_codcomred,pc_nomcomred,pc_tipdoc,pc_numdoc,pc_ciudad,pc_direccion,pc_telefono,pc_email
```

`pc_processdate` acepta `yyyy-MM-dd`, `dd/MM/yyyy`, `dd-MM-yyyy`, `ddMMyyyy`, `yyyyMMdd`.

## Reglas de cuarentena (`sp_process_commerce`)

| Regla | Motivo |
|-------|--------|
| `pc_nomcomred` vacío | El nombre del comercio (nomcomred) se encuentra vacío |
| `pc_numdoc` vacío | El número de documento (numdoc) se encuentra vacío |
| `pc_numdoc` con letras | El número de documento (numdoc) contiene letras |
| `pc_numdoc` con caracteres especiales | El número de documento (numdoc) contiene caracteres especiales |

Si un registro incumple varias reglas, los motivos se concatenan con `; `. El movimiento se hace en una sola sentencia `DELETE ... OUTPUT INTO` dentro de una transacción.

## Decisiones técnicas

- **Hexagonal**: `Domain` no depende de nada; `Application` define las interfaces que necesita en `Abstractions` y los servicios trabajan solo contra ellas; `Infrastructure` las implementa (SQL Server, CSV, JWT); `Api` expone los controllers y conecta todo en `Program.cs`.
- **Resultados**: los servicios devuelven clases `Result` (`Ok` / `Fail`) en lugar de lanzar excepciones para errores de negocio; el controller decide el código HTTP.
- **Eficiencia**: el archivo se lee en streaming y se envía a `sp_create_commerce` en una sola llamada mediante un table-valued parameter (`SqlDataRecord` sin materializar un `DataTable`).
- **Seguridad**: contraseñas con PBKDF2-SHA256 (100.000 iteraciones) y JWT firmado con HMAC-SHA256.
- **Errores**: `IExceptionHandler` global que responde 500 con `ProblemDetails` sin exponer detalles internos.
