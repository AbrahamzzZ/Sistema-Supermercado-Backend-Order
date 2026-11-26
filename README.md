# Sistema Supermercado — Order

Microservicio de **compras y ventas**. Valida los datos con Admin y el stock con Inventory, registra la operación
y publica los movimientos de stock en **Kafka** (tópico `movimientos-stock`) para que Inventory los procese.
También entrega las estadísticas que Admin usa para los reportes.

**Base de datos:** `DB_Order` — `COMPRA`, `DETALLE_COMPRA`, `VENTA`, `DETALLE_VENTA`.

📘 Documentación:
- [Cómo levantar todo el sistema](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway#readme) (README del Gateway)
- [Manual técnico](https://github.com/AbrahamzzZ/Sistema-Supermercado-Backend-Gateway/blob/main/docs/Manual%20Tecnico.docx): capítulo 8 (Order) y 10 (Comunicación entre microservicios)

## Ejecutar

Normalmente se levanta con el `docker-compose.yml` del repo del Gateway (Swagger: http://localhost:8004/swagger).

**Para depurar (modo híbrido):** el resto del sistema sigue en Docker y este servicio corre desde Visual Studio.

```bash
# en el repo del Gateway
docker compose up -d                  # todo el sistema
docker compose stop order-api       # libera el servicio que vas a depurar
```

Luego F5 en Visual Studio (o `dotnet run --project Order --launch-profile https`) → https://localhost:7118/swagger.
En el entorno Development, `appsettings.Development.json` lo conecta a lo que corre en Docker: SQL Server
`localhost,14330` (las mismas bases), Auth `http://localhost:8002` (validación de tokens), Gateway
`http://localhost:8000` y Kafka `localhost:29092`.

Pruébalo desde su Swagger con un token de `POST http://localhost:8000/auth/Usuario/login` (botón *Authorize*):
el frontend sigue pasando por el Gateway de Docker, que apunta al contenedor detenido. Al terminar:
`docker compose up -d --build order-api`.

Para usar tu propio SQL Server en vez del de Docker, define `ConnectionStrings:CadenaSQL` con `dotnet user-secrets`
(tiene prioridad sobre `appsettings.Development.json`).

## Endpoints principales

| Ruta (vía Gateway `/order/...`) | Descripción |
|---|---|
| `GET /api/Compra/numero-documento` · `POST /api/Compra` | Número de documento y registro de compras |
| `GET /api/Compra/{numeroDocumento}` · `/detalles/{id}` | Consulta de una compra |
| `GET /api/Venta/numero-documento` · `POST /api/Venta` | Número de documento y registro de ventas |
| `GET /api/Venta/{numeroDocumento}` · `/detalles/{id}` | Consulta de una venta |
| `GET /api/Estadistica/...` | Totales por ID para los reportes de Admin |
