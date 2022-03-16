using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace WoWsShipBuilder.Core.Services;

public class DesktopDataService : IDataService
{
    private readonly IFileSystem fileSystem;

    public DesktopDataService(IFileSystem fileSystem)
    {
        this.fileSystem = fileSystem;
    }

    public async Task StoreStringAsync(string content, string path)
    {
        CreateDirectory(path);
        await fileSystem.File.WriteAllTextAsync(path, content, Encoding.UTF8);
    }

    public async Task StoreAsync(object content, string path)
    {
        CreateDirectory(path);
        string fileContents = JsonConvert.SerializeObject(content);
        await fileSystem.File.WriteAllTextAsync(path, fileContents, Encoding.UTF8);
    }

    public async Task StoreAsync(Stream stream, string path)
    {
        CreateDirectory(path);
        await using var fileStream = fileSystem.File.OpenWrite(path);
        await stream.CopyToAsync(fileStream);
    }

    public void Store(object content, string path)
    {
        CreateDirectory(path);
        string fileContents = JsonConvert.SerializeObject(content);
        fileSystem.File.WriteAllText(path, fileContents, Encoding.UTF8);
    }

    public void Store(Stream stream, string path)
    {
        CreateDirectory(path);
        using var fileStream = fileSystem.File.OpenWrite(path);
        stream.CopyTo(fileStream);
    }

    public async Task<string?> LoadStringAsync(string path)
    {
        return await fileSystem.File.ReadAllTextAsync(path, Encoding.UTF8);
    }

    public async Task<T?> LoadAsync<T>(string path)
    {
        string contents = await fileSystem.File.ReadAllTextAsync(path, Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(contents);
    }

    public T? Load<T>(string path)
    {
        string contents = fileSystem.File.ReadAllText(path, Encoding.UTF8);
        return JsonConvert.DeserializeObject<T>(contents);
    }

    public string CombinePaths(params string[] paths)
    {
        return fileSystem.Path.Combine(paths);
    }

    private void CreateDirectory(string path)
    {
        string directoryName = fileSystem.Path.GetDirectoryName(path);
        fileSystem.Directory.CreateDirectory(directoryName);
    }
}
