using Domain.Model.Dto;
using Domain.Model.Dto.Compra;
using Infrastructure.Kafka;
using Infrastructure.Repository.InterfacesRepository;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.Services.Interface;
using Infrastructure.Services.Validators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Order.Tests.Services
{
    [TestClass]
    public class CompraServiceTests
    {
        private Mock<ICompraRepository> _repositorio = null!;
        private Mock<IMasterData> _maestros = null!;
        private Mock<IKafkaProducer> _kafka = null!;
        private List<ProductoMovimientoStock> _publicados = null!;
        private CompraService _servicio = null!;

        [TestInitialize]
        public void Inicializar()
        {
            _repositorio = new Mock<ICompraRepository>();
            _maestros = new Mock<IMasterData>();
            _kafka = new Mock<IKafkaProducer>();
            _publicados = new List<ProductoMovimientoStock>();

            var usuarioActual = new Mock<ICurrentUserService>();
            usuarioActual.Setup(u => u.IdUsuario).Returns(3);

            _maestros.Setup(m => m.SucursalExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _maestros.Setup(m => m.ProveedorExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _maestros.Setup(m => m.TransportistaExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _maestros.Setup(m => m.ProductoExisteAsync(It.IsAny<int>())).ReturnsAsync(true);
            _repositorio.Setup(r => r.RegistrarCompraAsync(It.IsAny<Compras>())).ReturnsAsync(true);
            _kafka.Setup(k => k.PublishAsync(It.IsAny<string>(), It.IsAny<ProductoMovimientoStock>(), It.IsAny<string?>()))
                  .Callback<string, ProductoMovimientoStock, string?>((_, m, _) => _publicados.Add(m))
                  .Returns(Task.CompletedTask);

            _servicio = new CompraService(
                _repositorio.Object,
                new CompraValidator(),
                _maestros.Object,
                Mock.Of<IInventoryClient>(),
                Mock.Of<IAdminCompraApiClient>(),
                Mock.Of<IAuthApiClient>(),
                _kafka.Object,
                usuarioActual.Object,
                NullLogger<CompraService>.Instance);
        }

        private static Compras CompraValida() => new()
        {
            Id_Sucursal = 1,
            Id_Proveedor = 1,
            Id_Transportista = 1,
            Tipo_Documento = "Factura",
            Numero_Documento = "00001",
            Detalles = new List<DetalleCompras>
            {
                new() { Id_Producto = 4, Cantidad = 10, Precio_Compra = 1.50m, Precio_Venta = 2.25m, SubTotal = 15 }
            }
        };

        [TestMethod]
        public async Task RegistrarCompra_ProveedorInexistente_Rechaza()
        {
            _maestros.Setup(m => m.ProveedorExisteAsync(It.IsAny<int>())).ReturnsAsync(false);

            var resultado = await _servicio.RegistrarCompraAsync(CompraValida());

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("El proveedor no existe", resultado.Message);
            _repositorio.Verify(r => r.RegistrarCompraAsync(It.IsAny<Compras>()), Times.Never);
        }

        [TestMethod]
        public async Task RegistrarCompra_ProductoInexistente_Rechaza()
        {
            _maestros.Setup(m => m.ProductoExisteAsync(4)).ReturnsAsync(false);

            var resultado = await _servicio.RegistrarCompraAsync(CompraValida());

            Assert.IsFalse(resultado.IsSuccess);
            Assert.AreEqual("Producto 4 no existe", resultado.Message);
        }

        [TestMethod]
        public async Task RegistrarCompra_Correcta_PublicaEntradaConLosPrecios()
        {
            var resultado = await _servicio.RegistrarCompraAsync(CompraValida());

            Assert.IsTrue(resultado.IsSuccess);
            var movimiento = _publicados.Single();
            Assert.AreEqual("ENTRADA", movimiento.Tipo);
            Assert.AreEqual("COMPRA", movimiento.Referencia);
            Assert.AreEqual(10, movimiento.Cantidad);
            Assert.AreEqual(1.50m, movimiento.PrecioCompra);
            Assert.AreEqual(2.25m, movimiento.PrecioVenta);
            Assert.AreEqual(3, movimiento.IdUsuario);
        }
    }
}
