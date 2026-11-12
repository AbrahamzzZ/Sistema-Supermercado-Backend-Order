# Sistema Supermercado — Order

Microservicio de **compras y ventas**. Al registrar una operación publica los movimientos de stock en **Kafka**
para que Inventory los procese de forma asíncrona.

> 📘 Para levantar **todo el sistema** (Docker, Kafka, Gateway, etc.) ver el
> [README principal en el repo del Gateway](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme).

## Responsabilidad

- Registro y consulta de **compras** (a proveedores) y **ventas** (a clientes) con sus detalles
- Generación del número de documento
- Registro y consulta de logs

**Base de datos:** `DB_Order` — tablas `COMPRA`, `DETALLE_COMPRA`, `VENTA`, `DETALLE_VENTA`, `LOG`.

## Estructura

```
Order/           API (controllers, Program.cs, appsettings)
Domain/          Entidades, DTOs (incluye DTOs de Admin/Auth), DbContext y Migrations
Infrastructure/  Repositorios, servicios, clientes HTTP, Handler del token y Kafka (producer)
Utilities/       ApiResponse, mensajes y paginación
```

## Endpoints

Vía Gateway: `http://localhost:8000/order/...` → aquí `/api/...` · **Todos requieren token.**

| Método | Ruta | Descripción |
|---|---|---|
| GET | `/api/Compra/numero-documento` | Genera el siguiente número de documento |
| GET | `/api/Compra/{numeroDocumento}` | Obtiene una compra (con datos de proveedor, sucursal, transportista y usuario) |
| GET | `/api/Compra/detalles/{idCompra}` | Detalles de una compra |
| POST | `/api/Compra` | Registra una compra |
| GET | `/api/Venta/numero-documento` | Genera el siguiente número de documento |
| GET | `/api/Venta/{numeroDocumento}` | Obtiene una venta (con datos de cliente, sucursal y usuario) |
| GET | `/api/Venta/detalles/{idVenta}` | Detalles de una venta |
| POST | `/api/Venta` | Registra una venta |
| GET | `/api/Log/paginacion?pageNumber=1&pageSize=10` | Logs paginados |
| GET | `/api/Log/{id}` | Obtiene un log |

**Estadísticas** (las consume Admin para los reportes de `/admin/Negocio/...`; el frontend no las llama
directamente). Devuelven solo **IDs y totales**: los nombres los completa Admin con sus propias tablas.

| Método | Ruta | Devuelve (`data`) |
|---|---|---|
| GET | `/api/Estadistica/productos-comprados` | Top 5 `{ id: idProducto, total: unidades }` |
| GET | `/api/Estadistica/productos-vendidos` | Top 5 `{ id: idProducto, total: unidades }` |
| GET | `/api/Estadistica/top-clientes` | Top 5 `{ id: idCliente, total: ventas }` |
| GET | `/api/Estadistica/top-proveedores` | Top 5 `{ id: idProveedor, total: compras }` |
| GET | `/api/Estadistica/viajes-transportista` | Top 5 `{ id: idTransportista, total: compras }` |
| GET | `/api/Estadistica/ventas-empleado` | Top 5 `{ id: idUsuario, total: ventas }` |
| GET | `/api/Estadistica/tendencia-vendidos` | `{ idProducto, fecha, cantidad }` por producto y día (hora de Ecuador) |
| GET | `/api/Estadistica/tendencia-comprados` | `{ idProducto, fecha, cantidad }` por producto y día (hora de Ecuador) |

Sin compras o ventas responden `200` con `data: []`.

Swagger: http://localhost:8004/swagger (Docker) · https://localhost:7118/swagger (local)

## Integraciones

### HTTP (síncrono) — a través del Gateway
Order llama al Gateway (`Services:GatewayUrl`) **reenviando el token del usuario** (`TokenDelegatingHandler`):

- **Admin** → valida y obtiene producto, cliente, proveedor, sucursal y transportista (`/admin/{Recurso}/{id}`)
- **Auth** → obtiene los datos del usuario (`/auth/Usuario/{id}`)

- **Inventory** → antes de registrar una venta consulta el stock de cada producto (`/inventory/ProductoStock/{id}`)

### Kafka (asíncrono) — producer
Después de guardar una compra o venta, publica **un mensaje por cada detalle** en el tópico `movimientos-stock`.
La clave del mensaje es el `IdProducto`, así los movimientos de un mismo producto se procesan en orden.

```json
{ "IdProducto": 5, "Tipo": "ENTRADA", "Cantidad": 10, "Referencia": "COMPRA",
  "PrecioCompra": 1.20, "PrecioVenta": 1.80, "IdUsuario": 1 }
```

| Campo / header | Descripción |
|---|---|
| `IdUsuario` | Usuario que hizo la compra/venta (tomado del token). Inventory lo usa para auditar el movimiento. |
| `PrecioCompra` | Solo en compras. En ventas va `null` para no sobrescribir el precio en Inventory. |
| Header `X-Correlation-Id` | Correlation Id de la petición original, para seguir la operación hasta Inventory en Seq. |

| Operación | `Tipo` | `Referencia` |
|---|---|---|
| Compra | `ENTRADA` | `COMPRA` |
| Venta | `SALIDA` | `VENTA` |

El consumidor es el microservicio **Inventory**. Si la publicación falla, la compra/venta **ya quedó registrada**:
se registra un log de nivel Error y la respuesta avisa que el stock no se actualizó.

## Reglas al registrar compras y ventas

- **Usuario:** `Id_Usuario` se toma **siempre del token JWT**; el valor que venga en el body se ignora.
- **Datos maestros:** se valida en Admin que existan sucursal, proveedor/cliente, transportista y productos.
- **Stock (solo ventas):** antes de registrar se consulta a Inventory el stock de cada producto (si un producto se
  repite en el detalle, se suman las cantidades). Si no alcanza → `400` *"Stock insuficiente para el producto X.
  Disponible: N"*. Si Inventory no responde, la venta **no** se registra.
- **Número de documento duplicado** → `400` con mensaje claro (antes el SP ocultaba el error).

## Logs (Serilog + Seq)

Los logs se envían a la consola y a **Seq** (`Seq:ServerUrl`; en Docker: http://localhost:5341 para verlos).
Cada log incluye `Servicio=Order`, `CorrelationId` y, en las peticiones, `IdUsuario`.

| Nivel | Cuándo |
|---|---|
| `Information` | Operación correcta (p. ej. *"Compra 00012 registrada por el usuario 1"*) |
| `Warning` | Regla de negocio incumplida (validación, stock insuficiente, documento duplicado, maestro inexistente) |
| `Error` | Fallo técnico: excepción no controlada, SQL Server, Kafka, Admin/Inventory sin responder |

- **Correlation Id:** header `X-Correlation-Id`. Si no llega (el Gateway lo envía), se genera uno. Se devuelve en
  la respuesta, se reenvía a Admin/Auth/Inventory y viaja en los mensajes de Kafka. En Seq, filtra por
  `CorrelationId = '...'` para ver toda la operación en todos los servicios.
- **Errores no controlados:** `GlobalExceptionHandler` los registra como Error y responde `500` con
  `{ "isSuccess": false, "message": "Ocurrió un error interno. Código de seguimiento: <correlationId>" }`.
- La tabla `LOG` y sus endpoints (`/api/Log`) se mantienen por compatibilidad.

## Fechas

Todas las fechas se guardan en **UTC** (`DATETIME2` con `SYSUTCDATETIME()`). Los SP que devuelven fechas
formateadas (`dd/mm/yyyy`) las convierten a hora de Ecuador (`SA Pacific Standard Time`).

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Order` |
| `Jwt:Authority` | URL de Auth, de donde se descarga la clave pública (local: `https://localhost:7186`) |
| `Jwt:RequireHttpsMetadata` | `false` solo dentro de Docker (http) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los de Auth |
| `Services:GatewayUrl` | URL del Gateway (local: `https://localhost:7134`; Docker: `http://gateway:8080`) |
| `Kafka:BootstrapServers` | Broker de Kafka (local con Docker: `localhost:29092`; dentro de Docker: `kafka:9092`) |
| `Seq:ServerUrl` | Servidor de logs Seq (local: `http://localhost:5341`; dentro de Docker: `http://seq:80`) |
| `Serilog:MinimumLevel` | Nivel mínimo de logs y overrides por namespace |

## Ejecutar solo este servicio

```bash
dotnet run --project Order --launch-profile https
```

Requiere **Auth**, **Admin** y el **Gateway** corriendo, además de **Kafka**. Puedes levantar solo la infraestructura
desde el repo del Gateway:

```bash
docker compose up -d sqlserver kafka kafka-init kafka-ui
```

## Migraciones (EF Core Code First)

Las migraciones se aplican solas al iniciar (`Database.Migrate()` en `Program.cs`). Para crear una nueva:

```bash
dotnet ef migrations add NombreMigracion --project Domain --startup-project Order
```

| Migración | Contenido |
|---|---|
| `Initial` | Tablas `COMPRA`, `DETALLE_COMPRA`, `VENTA`, `DETALLE_VENTA`, `LOG` |
| `AuditoriaYProcedimientos` | Fechas a `DATETIME2` UTC + tipos tabla y procedimientos almacenados |
| `Estadisticas` | Procedimientos de estadísticas para los reportes de Admin (no cambia tablas) |

### Procedimientos almacenados

Los SP y tipos tabla son archivos `.sql` en `Domain/Migrations/Scripts/`, embebidos en `Domain.dll`
(`<EmbeddedResource>` en `Domain.csproj`). La migración los ejecuta con `migrationBuilder.EjecutarScript("V001_Compra.sql")`
(`ScriptRunner.cs` separa los lotes por líneas `GO`, igual que SSMS).

| Script | Objetos |
|---|---|
| `V001_TiposTabla.sql` | `TI_Detalle_Compra`, `TI_Detalle_Venta` |
| `V001_Compra.sql` | `PA_OBTENER_COMPRA`, `PA_OBTENER_DETALLES_COMPRA`, `PA_REGISTRAR_COMPRA` |
| `V001_Venta.sql` | `PA_OBTENER_VENTA`, `PA_OBTENER_DETALLES_VENTA`, `PA_REGISTRAR_VENTA` |
| `V001_Log.sql` | `PA_LISTA_LOG_PAGINACION`, `PA_OBTENER_LOG`, `PA_REGISTRAR_LOG` |
| `V002_Estadisticas.sql` | `PA_ESTADISTICA_PRODUCTOS_COMPRADOS`, `_PRODUCTOS_VENDIDOS`, `_TOP_CLIENTES`, `_TOP_PROVEEDORES`, `_VIAJES_TRANSPORTISTA`, `_VENTAS_EMPLEADO`, `_TENDENCIA_VENDIDOS`, `_TENDENCIA_COMPRADOS` |

**Para cambiar un SP en el futuro** (nunca edites un script que ya se aplicó, porque las bases existentes no lo
volverían a ejecutar):

1. Crea un archivo nuevo versionado, p. ej. `Domain/Migrations/Scripts/V002_Venta.sql`, con `CREATE OR ALTER PROCEDURE ...`.
2. Crea una migración vacía: `dotnet ef migrations add CambioSpVenta --project Domain --startup-project Order`.
3. En su `Up()` llama `migrationBuilder.EjecutarScript("V002_Venta.sql");` y en `Down()` vuelve a ejecutar la
   versión anterior (`V001_Venta.sql`).

## Docker

```bash
docker build -t supermercado-order .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
