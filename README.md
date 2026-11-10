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

Swagger: http://localhost:8004/swagger (Docker) · https://localhost:7118/swagger (local)

## Integraciones

### HTTP (síncrono) — a través del Gateway
Order llama al Gateway (`Services:GatewayUrl`) **reenviando el token del usuario** (`TokenDelegatingHandler`):

- **Admin** → valida y obtiene producto, cliente, proveedor, sucursal y transportista (`/admin/{Recurso}/{id}`)
- **Auth** → obtiene los datos del usuario (`/auth/Usuario/{id}`)

### Kafka (asíncrono) — producer
Después de guardar una compra o venta, publica **un mensaje por cada detalle** en el tópico `movimientos-stock`.
La clave del mensaje es el `IdProducto`, así los movimientos de un mismo producto se procesan en orden.

```json
{ "IdProducto": 5, "Tipo": "ENTRADA", "Cantidad": 10, "Referencia": "COMPRA", "PrecioCompra": 1.20, "PrecioVenta": 1.80 }
```

| Operación | `Tipo` | `Referencia` |
|---|---|---|
| Compra | `ENTRADA` | `COMPRA` |
| Venta | `SALIDA` | `VENTA` |

El consumidor es el microservicio **Inventory**.

## Configuración

| Clave | Descripción |
|---|---|
| `ConnectionStrings:CadenaSQL` | Cadena de conexión a `DB_Order` |
| `Jwt:Authority` | URL de Auth, de donde se descarga la clave pública (local: `https://localhost:7186`) |
| `Jwt:RequireHttpsMetadata` | `false` solo dentro de Docker (http) |
| `Jwt:Issuer` / `Jwt:Audience` | Deben coincidir con los de Auth |
| `Services:GatewayUrl` | URL del Gateway (local: `https://localhost:7134`; Docker: `http://gateway:8080`) |
| `Kafka:BootstrapServers` | Broker de Kafka (local con Docker: `localhost:29092`; dentro de Docker: `kafka:9092`) |

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

## Docker

```bash
docker build -t supermercado-order .
```

Normalmente se levanta junto al resto con el `docker-compose.yml` del repo del Gateway.
