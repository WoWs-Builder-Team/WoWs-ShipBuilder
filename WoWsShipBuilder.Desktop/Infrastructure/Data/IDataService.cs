using System.IO;
using System.Threading.Tasks;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

public interface IDataService
{
    Task StoreStringAsync(string content, string path);

    Task StoreAsync(object content, string path);

    Task StoreAsync(Stream stream, string path);

    void Store(object content, string path);

    void Store(Stream stream, string path);

    Task<string?> LoadStringAsync(string path);

    Task<T?> LoadAsync<T>(string path);

    T? Load<T>(string path);

    string CombinePaths(params string[] paths);
}
