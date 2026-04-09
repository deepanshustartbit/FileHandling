using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace FileHandlingSystem
{
    public class GraphService
    {
        private readonly GraphServiceClient _client;
    
        public GraphService(string clientId,string tenentid, string clientSecret)
        {
         
            var credential = new ClientSecretCredential(
                tenentid,
                clientId,
                clientSecret
            );
            _client = new GraphServiceClient(credential);
        }

        public async Task<List<string>> GetFilesAsync()
        {
            var files = new List<string>();
            //  string siteId = "startbitsolutions.sharepoint.com:/sites/TeamStartbit2";
            var site = await _client.Sites["startbitsolutions.sharepoint.com:/sites/--------"].GetAsync();
            string siteId = site.Id; // from above
            var drives = await _client.Sites[siteId].Drives.GetAsync();
            var drive = drives.Value.FirstOrDefault();

            // Get root files
            var items = await _client.Drives[drive.Id]
                     .Items["root"]
                     .Children
                     .GetAsync();
          
            foreach (var item in items.Value)
            {
                Console.WriteLine("Name: " + item.Name);
                if (item.Folder != null && item.Name == "General")
                {
                    Console.WriteLine(" Entering General folder...");

                    var folderItems = await _client.Drives[drive.Id]
                        .Items[item.Id]
                        .Children
                        .GetAsync();

                    foreach (var file in folderItems.Value)
                    {
                        Console.WriteLine("File inside General: " + file.Name);
                    }
                }
                if (item.File != null)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }
        public List<string> GetLocalFiles(string folderPath)
        {
            var files = new List<string>();

            var filePaths = Directory.GetFiles(folderPath);

            foreach (var file in filePaths)
            {
                string fileName = Path.GetFileName(file);
                files.Add(fileName);

                Console.WriteLine("File: " + fileName);
            }

            return files;
        }
        public async Task UploadAllFilesToGeneral(string localFolderPath)
        {
            try
            {
                // Step 1: Get site
                var site = await _client.Sites["startbitsolutions.sharepoint.com:/sites/-------"].GetAsync();
                string siteId = site.Id;
                // Step 2: Get document library (drive)
                var drives = await _client.Sites[siteId].Drives.GetAsync();
                var drive = drives.Value.FirstOrDefault();

                if (drive == null)
                {
                    Console.WriteLine(" No drive found");
                    return;
                }

                // Step 3: Get root folders
                var rootItems = await _client.Drives[drive.Id]
                    .Items["root"]
                    .Children
                    .GetAsync();

                // Step 4: Find "General" folder
                var generalFolder = rootItems.Value.FirstOrDefault(x => x.Name == "General" && x.Folder != null);

                if (generalFolder == null)
                {
                    Console.WriteLine(" General folder not found");
                    return;
                }

                // Step 5: Get local files
                var files = Directory.GetFiles(localFolderPath);

                foreach (var filePath in files)
                {
                    try
                    {
                        string fileName = Path.GetFileName(filePath);

                        Console.WriteLine($"Uploading: {fileName}");

                        using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            // Step 6: Upload file
                            var uploadedItem = await _client.Drives[drive.Id]
                                .Items[generalFolder.Id]
                                .ItemWithPath(fileName)
                                .Content
                                .PutAsync(stream);

                            Console.WriteLine(" Uploaded: " + uploadedItem.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" Error uploading file: " + ex.Message);
                    }
                }

                Console.WriteLine(" All files uploaded successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(" Main Error: " + ex.Message);
            }
        }
    }
}
