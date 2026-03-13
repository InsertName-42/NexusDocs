using Google;
using Google.Apis.Auth.AspNetCore3;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Markdig;
using System.Net;
using System.Net.Http.Headers;

namespace NexusDocs.Services;

public class GoogleSyncService
{
    private readonly IGoogleAuthProvider _auth;

    public GoogleSyncService(IGoogleAuthProvider auth)
    {
        _auth = auth;
    }

    public async Task<(string? content, string? newEtag)> SyncPageAsync(string docId, string? currentEtag, bool useMarkdown)
    {
        var credential = await _auth.GetCredentialAsync();
        var driveService = new DriveService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credential,
            ApplicationName = "NexusDocs"
        });

        // 1. Determine the export format based on user tags
        string mimeType = useMarkdown ? "text/markdown" : "text/html";

        var request = driveService.Files.Export(docId, mimeType);

        // 2. Add the ETag for conditional loading
        if (!string.IsNullOrEmpty(currentEtag))
        {
            request.ModifyRequest = (httpRequest) =>
            {
                // Google ETags are usually wrapped in quotes
                httpRequest.Headers.IfNoneMatch.Add(new EntityTagHeaderValue($"\"{currentEtag.Trim('"')}\""));
            };
        }

        try
        {
            using (var responseStream = await request.ExecuteAsStreamAsync())
            using (var reader = new StreamReader(responseStream))
            {
                string rawContent = await reader.ReadToEndAsync();

                // 3. Process content if it's Markdown
                string finalHtml = useMarkdown ? Markdown.ToHtml(rawContent) : rawContent;

                // 4. Capture the new ETag from the response
                // Note: The Google .NET client often exposes the ETag on the metadata object
                var metadata = await driveService.Files.Get(docId).ExecuteAsync();

                return (finalHtml, metadata.ETag);
            }
        }
        catch (GoogleApiException ex) when (ex.HttpStatusCode == HttpStatusCode.NotModified)
        {
            // The document hasn't changed! Return nulls to indicate no update needed.
            return (null, null);
        }
    }
}