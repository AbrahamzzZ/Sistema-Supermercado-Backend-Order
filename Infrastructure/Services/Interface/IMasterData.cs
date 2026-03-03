namespace Infrastructure.Services.Interface
{
    public interface IMasterData
    {
        Task<bool> UsuarioExisteAsync(int idUsuario);
        Task<bool> SucursalExisteAsync(int idSucursal);
        Task<bool> ProveedorExisteAsync(int idProveedor);
        Task<bool> TransportistaExisteAsync(int idTransportista);
        Task<bool> ProductoExisteAsync(int idProducto);
        Task<bool> ClienteExisteAsync(int idProducto);
    }
}
