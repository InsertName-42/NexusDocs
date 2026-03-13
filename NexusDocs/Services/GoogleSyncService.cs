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
                //1. Request the 'version' field specifically
                var getRequest = service.Files.Get(fileId);
                getRequest.Fields = "version";
                var fileMetadata = await getRequest.ExecuteAsync();

                string newVersion = fileMetadata.Version?.ToString() ?? "0";

                //2. Comparison Logic
                if (newVersion == currentVersion)
                {
                    return (null, currentVersion);
                }

                //3. Export if versions differ
                var exportRequest = service.Files.Export(fileId, "text/html");
                using (var stream = new MemoryStream())
                {
                    await exportRequest.DownloadAsync(stream);
                    stream.Position = 0;
                    using (var reader = new StreamReader(stream))
                    {
                        string htmlContent = await reader.ReadToEndAsync();

                        System.Diagnostics.Debug.WriteLine("***************************************************");
                        System.Diagnostics.Debug.WriteLine($">>> VERSION CHANGE DETECTED: {currentVersion} -> {newVersion} <<<");
                        System.Diagnostics.Debug.WriteLine("***************************************************");

                        return (htmlContent, newVersion);
                    }
                }
            }
            catch (Google.GoogleApiException ex)
            {
                string errorMessage = ex.Error?.Errors?.FirstOrDefault()?.Message ?? ex.Message;

                System.Diagnostics.Debug.WriteLine("\n***************************************************");
                System.Diagnostics.Debug.WriteLine("!!! GOOGLE SYNC ERROR !!!");
                System.Diagnostics.Debug.WriteLine($"REASON: {errorMessage}");
                System.Diagnostics.Debug.WriteLine("***************************************************\n");

                throw new Exception(errorMessage);
            }
        }
    }
}