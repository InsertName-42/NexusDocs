using Google.Apis.Drive.v3;
using Google.Apis.Services;
using System.Net;

namespace NexusDocs.Services
{
    public class GoogleSyncService
    {
        private readonly IConfiguration _config;

        public GoogleSyncService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<(string? content, string? etag)> SyncPageAsync(string fileId, string? currentVersion, bool isMarkdown)
        {
            fileId = fileId.Trim();
            var apiKey = _config["Google:ApiKey"];

            var service = new DriveService(new BaseClientService.Initializer()
            {
                ApiKey = apiKey,
                ApplicationName = "NexusDocs"
            });

            try
            {
                var getRequest = service.Files.Get(fileId);
                getRequest.Fields = "version";
                var fileMetadata = await getRequest.ExecuteAsync();

                string newVersion = fileMetadata.Version?.ToString() ?? "0";

                if (newVersion == currentVersion)
                {
                    return (null, currentVersion);
                }

                string mimeType = isMarkdown ? "text/plain" : "text/html";

                var exportRequest = service.Files.Export(fileId, mimeType);
                using (var stream = new MemoryStream())
                {
                    await exportRequest.DownloadAsync(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        string content = await reader.ReadToEndAsync();
                        return (content, newVersion);
                    }
                }
            }
            catch (Google.GoogleApiException ex)
            {
                throw new Exception(ex.Error?.Errors?.FirstOrDefault()?.Message ?? ex.Message);
            }
        }
    }
}