using System.IO;
using System.IO.Abstractions;
using System.Runtime.Versioning;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WoWsShipBuilder.Infrastructure.ApplicationData;

namespace WoWsShipBuilder.Desktop.Infrastructure.Data;

[UnsupportedOSPlatform("browser")]
public class DesktopDataService : IDataService
{
    private readonly IFileSystem fileSystem;

    public DesktopDataService(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public async Task StoreStringAsync(string content, string path)
    {
        this.CreateDirectory(path);
        await this.fileSystem.File.WriteAllTextAsync(path, content, Encoding.UTF8);
    }

    public async Task StoreAsync<T>(T content, string path)
    {
        this.CreateDirectory(path);
        string fileContents = JsonSerializer.Serialize(content, AppConstants.JsonSerializerOptions);
        await this.fileSystem.File.WriteAllTextAsync(path, fileContents, Encoding.UTF8);
    }

    public async Task StoreAsync(Stream stream, string path)
    {
        this.CreateDirectory(path);
        await using var fileStream = this.fileSystem.File.Open(path, FileMode.Create);
        await stream.CopyToAsync(fileStream);
    }

    public void Store<T>(T content, string path)
    {
        this.CreateDirectory(path);
        string fileContents = JsonSerializer.Serialize(content, AppConstants.JsonSerializerOptions);
        this.fileSystem.File.WriteAllText(path, fileContents, Encoding.UTF8);
    }

    public void Store(Stream stream, string path)
    {
        this.CreateDirectory(path);
        using var fileStream = this.fileSystem.File.OpenWrite(path);
        stream.CopyTo(fileStream);
    }

    public async Task<string?> LoadStringAsync(string path)
    {
        return await this.fileSystem.File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    public async Task<T?> LoadAsync<T>(string path)
    {
        string contents = await this.fileSystem.File.ReadAllTextAsync(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(contents, AppConstants.JsonSerializerOptions);
    }

    public T? Load<T>(string path)
    {
        string contents = this.fileSystem.File.ReadAllText(path, Encoding.UTF8);
        return JsonSerializer.Deserialize<T>(contents, AppConstants.JsonSerializerOptions);
    }

    public string CombinePaths(params string[] paths)
    {
        return this.fileSystem.Path.Combine(paths);
    }

    private void CreateDirectory(string path)
    {
        string directoryName = this.fileSystem.Path.GetDirectoryName(path)!;
        this.fileSystem.Directory.CreateDirectory(directoryName);
    }
}
