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

Solo este servicio:

```bash
dotnet run --project Order --launch-profile https     # https://localhost:7118/swagger
```

Necesita SQL Server, **Kafka**, **Auth**, **Admin**, **Inventory** y el **Gateway**. La infraestructura se puede
levantar sola desde el repo del Gateway:

```bash
docker compose up -d sqlserver kafka kafka-init kafka-ui seq
```

## Endpoints principales

| Ruta (vía Gateway `/order/...`) | Descripción |
|---|---|
| `GET /api/Compra/numero-documento` · `POST /api/Compra` | Número de documento y registro de compras |
| `GET /api/Compra/{numeroDocumento}` · `/detalles/{id}` | Consulta de una compra |
| `GET /api/Venta/numero-documento` · `POST /api/Venta` | Número de documento y registro de ventas |
| `GET /api/Venta/{numeroDocumento}` · `/detalles/{id}` | Consulta de una venta |
| `GET /api/Estadistica/...` | Totales por ID para los reportes de Admin |
