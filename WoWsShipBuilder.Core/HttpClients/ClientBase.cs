using System;
using System.IO;
using System.IO.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WoWsShipBuilder.Core.DataProvider;

// ReSharper disable VirtualMemberNeverOverridden.Global
namespace WoWsShipBuilder.Core.HttpClients
{
    public abstract class ClientBase
    {
        protected ClientBase(IFileSystem fileSystem, AppDataHelper appDataHelper)
        {
            FileSystem = fileSystem;
            AppDataHelper = appDataHelper;
        }

        protected IFileSystem FileSystem { get; }

        protected AppDataHelper AppDataHelper { get; }

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
