using Domain.Model.Dto;
using Domain.Model.Dto.Venta;
using Infrastructure.Kafka;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.Interface;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Utilities.Shared;

namespace Order.Tests.Services
{
    [TestClass]
    public class VentaServiceTests
    {
        private Mock<IVentaRepository> _repositorio = null!;
        private Mock<IMasterData> _maestros = null!;
        private Mock<IInventoryClient> _inventario = null!;
        private Mock<IKafkaProducer> _kafka = null!;
        private Mock<ICurrentUserService> _usuarioActual = null!;
        private List<ProductoMovimientoStock> _publicados = null!;
        private VentaService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<IVentaRepository>();
            _maestros = new Mock<IMasterData>();
            _inventario = new Mock<IInventoryClient>();
            _kafka = new Mock<IKafkaProducer>();
            _usuarioActual = new Mock<ICurrentUserService>();
            _publicados = new List<ProductoMovimientoStock>();

            // Por defecto: usuario 7 en el token, todos los maestros existen, stock de sobra y el SP registra.
            _usuarioActual.Setup(u => u.IdUsuario).Returns(7);
            _maestros.Setup(m => m.SucursalExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _maestros.Setup(m => m.ClienteExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _maestros.Setup(m => m.ProductoExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _inventario.Setup(i => i.ObtenerStockAsync(It.IsAny<int>())).ReturnsAsync(100);
            _repositorio.Setup(r => r.RegistrarVentaAsync(It.IsAny<Ventas>())).ReturnsAsync(true);
            _kafka.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<ProductoMovimientoStock>(), It.IsAny<string?>()))
                  .Callback<string, ProductoMovimientoStock, string?>((_, m, _) => _publicados.Add(m))
                  .Returns(Task.CompletedTask);

            _servicio = new VentaService(
                _repositorio.Object,
                new VentaValidator(),
                _maestros.Object,
                _inventario.Object,
                Mock.Of<IAdminVentaApiClient>(),
                Mock.Of<IAuthApiClient>(),
                _kafka.Object,
                _usuarioActual.Object,
                NullLogger<VentaService>.Instance);
        }

        private static Ventas VentaValida(params DetalleVentas[] detalles) => new()
        {
            Id_Usuario = 999, // debe ignorarse: el usuario sale del token
            Id_Sucursal = 1,
            Id_Cliente = 1,
            Tipo_Documento = "Factura",
            Numero_Documento = "00001",
            Monto_Pago = 50,
            Detalles = detalles.Length > 0
                ? detalles.ToList()
                : new List<DetalleVentas> { new() { Id_Producto = 1, Cantidad = 2, Precio_Venta = 5, SubTotal = 10 } }
        };

        [TestMethod]
        public async Task RegistrarVenta_SinUsuarioEnToken_RechazaYNoRegistra()
        {
            _usuarioActual.Setup(u => u.IdUsuario).Returns((int?)null);

            var resultado = await _servicio.RegistrarVentaAsync(VentaValida());

            Assert.IsFalse(resultado.IsSuccess);
            _repositorio.Verify(r => r.RegistrarVentaAsync(It.IsAny<Ventas>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarVenta_UsaElUsuarioDelTokenYNoElDelBody()
        {
            var venta = VentaValida();

            await _servicio.RegistrarVentaAsync(venta);

            _repositorio.Verify(r => r.RegistrarVentaAsync(It.Is<Ventas>(v => v.Id_Usuario == 7)), Times.Once);
        }

        [TestMethod]
        public async Task RegistrarVenta_ConDatosInvalidos_RechazaSinConsultarOtrosServicios()
        {
            var venta = VentaValida();
            venta.Monto_Pago = 0;
            venta.Detalles = new List<DetalleVentas>();

            var resultado = await _servicio.RegistrarVentaAsync(venta);

            Assert.IsFalse(resultado.IsSuccess);
            _maestros.Verify(m => m.SucursalExisteAsync(It.IsAny<int>()), Times.Never);
            _repositorio.Verify(r => r.RegistrarVentaAsync(It.IsAny<Ventas>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarVenta_ClienteInexistente_Rechaza()
        {
            _maestros.Setup(m => m.ClienteExisteAsync(It.IsAny<int>())).ReturnsAsync(false);

            var resultado = await _servicio.RegistrarVentaAsync(VentaValida());

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("El Cliente no existe", resultado.Message);
            _repositorio.Verify(r => r.RegistrarVentaAsync(It.IsAny<Ventas>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarVenta_StockInsuficienteSumandoLineasDelMismoProducto_Rechaza()
        {
            // 3 + 3 = 6 unidades del producto 1, pero solo hay 5.
            _inventario.Setup(i => i.ObtenerStockAsync(1)).ReturnsAsync(5);
            var venta = VentaValida(
                new DetalleVentas { Id_Producto = 1, Cantidad = 3 },
                new DetalleVentas { Id_Producto = 1, Cantidad = 3 });

            var resultado = await _servicio.RegistrarVentaAsync(venta);

            Assert.IsFalse(resultado.IsSuccess);
            StringAssert.Contains(resultado.Message, "Stock insuficiente para el producto 1");
            _repositorio.Verify(r => r.RegistrarVentaAsync(It.IsAny<Ventas>()), Times.Never);
            Assert.AreEqual(0, _publicados.Count);
        }

        [TestMethod]
        public async Task RegistrarVenta_InventoryNoResponde_RechazaSinRegistrar()
        {
            _inventario.Setup(i => i.ObtenerStockAsync(It.IsAny<int>())).ThrowsAsync(new HttpRequestException("caído"));

            var resultado = await _servicio.RegistrarVentaAsync(VentaValida());

            Assert.IsFalse(resultado.IsSuccess);
            StringAssert.Contains(resultado.Message, "No se pudo verificar el stock");
            _repositorio.Verify(r => r.RegistrarVentaAsync(It.IsAny<Ventas>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarVenta_Correcta_PublicaUnaSalidaPorDetalle()
        {
            var venta = VentaValida(
                new DetalleVentas { Id_Producto = 1, Cantidad = 2, Precio_Venta = 5 },
                new DetalleVentas { Id_Producto = 2, Cantidad = 1, Precio_Venta = 8 });

            var resultado = await _servicio.RegistrarVentaAsync(venta);

            Assert.IsTrue(resultado.IsSuccess);
            Assert.AreEqual(Mensajes.MESSAGE_REGISTER, resultado.Message);
            Assert.AreEqual(2, _publicados.Count);
            Assert.IsTrue(_publicados.All(m => m.Tipo == "SALIDA" && m.Referencia == "VENTA" && m.IdUsuario == 7));
            Assert.AreEqual(2, _publicados.Single(m => m.IdProducto == 1).Cantidad);
        }

        [TestMethod]
        public async Task RegistrarVenta_SiElSpNoRegistra_NoPublicaEnKafka()
        {
            _repositorio.Setup(r => r.RegistrarVentaAsync(It.IsAny<Ventas>())).ReturnsAsync(false);

            var resultado = await _servicio.RegistrarVentaAsync(VentaValida());

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual(0, _publicados.Count);
        }

        [TestMethod]
        public async Task RegistrarVenta_KafkaFalla_LaVentaQuedaRegistradaConAviso()
        {
            _kafka.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<ProductoMovimientoStock>(), It.IsAny<string?>()))
                  .ThrowsAsync(new Exception("broker caído"));

            var resultado = await _servicio.RegistrarVentaAsync(VentaValida());

            Assert.IsTrue(resultado.IsSuccess);
            StringAssert.Contains(resultado.Message, "no se pudo notificar a Inventario");
        }
    }
}
