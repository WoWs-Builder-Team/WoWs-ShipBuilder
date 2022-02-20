using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;
using WoWsShipBuilder.Core.Services;

// ReSharper disable VirtualMemberNeverOverridden.Global
namespace WoWsShipBuilder.Core.HttpClients
{
    public abstract class ClientBase
    {
        protected ClientBase(IFileSystem fileSystem, IAppDataService appDataService)
        {
            FileSystem = fileSystem;
            AppDataService = appDataService;
        }

        protected IFileSystem FileSystem { get; }

        protected IAppDataService AppDataService { get; }

        protected abstract HttpClient Client { get; }

        internal virtual async Task DownloadFileAsync(Uri uri, string fileName)
        {
            await using Stream stream = await Client.GetStreamAsync(uri);
            IFileInfo fileInfo = FileSystem.FileInfo.FromFileName(fileName);
            FileSystem.Directory.CreateDirectory(fileInfo.DirectoryName);
            await using Stream fileStream = fileInfo.Open(FileMode.Create);
            await stream.CopyToAsync(fileStream);
        }

        internal virtual async Task<T?> GetJsonAsync<T>(string url, JsonSerializer? customSerializer = null)
        {
            await using Stream stream = await Client.GetStreamAsync(url);
            return GetJson<T>(stream, customSerializer);
        }

        internal T? GetJson<T>(Stream stream, JsonSerializer? customSerializer = null)
        {
            using var streamReader = new StreamReader(stream);
            using var jsonReader = new JsonTextReader(streamReader);
            JsonSerializer serializer = customSerializer ?? new JsonSerializer();
            return serializer.Deserialize<T>(jsonReader);
        }
    }
}
