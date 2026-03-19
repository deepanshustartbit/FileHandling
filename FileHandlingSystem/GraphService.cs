using Azure.Identity;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileHandlingSystem
{
    public class GraphService
    {
        private readonly GraphServiceClient _client;

        public GraphService(string clientId)
        {
            var credential = new InteractiveBrowserCredential(
                new InteractiveBrowserCredentialOptions
                {
                    ClientId = clientId,
                    TenantId = "common"
                });

            _client = new GraphServiceClient(credential);
        }

        public async Task<List<string>> GetFilesAsync()
        {
            var files = new List<string>();

            var drive = await _client.Me.Drive.GetAsync();

            var items = await _client.Drives[drive.Id]
                .Items["root"]
                .Children
                .GetAsync();

            foreach (var item in items.Value)
            {
                if (item.File != null)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }
    }
}
