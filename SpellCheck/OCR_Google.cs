using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Google.Cloud.Vision.V1;

namespace SpellCheck
{
    class OCR_Google
    {
        
        public static void ProcessDirectory(string targetDirectory)
        {
            var credential = GoogleCredential.FromFile(@"D:\Google_API_key_TFM.json");
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string path in fileEntries)
                GetDataAsync(path, credential).Wait();
        }
        private static async Task GetDataAsync(string path, GoogleCredential credential)
        {
            try
            {
                int pos = path.LastIndexOf("\\");
                string strFileName = path.Substring(pos + 1, path.Length - pos - 1);
                StorageClient storageClient = await StorageClient.CreateAsync(credential);
                storageClient.Service.HttpClient.Timeout = new TimeSpan(0, 10, 0);
                var bucket = await storageClient.GetBucketAsync("bucket ocr");
                FileStream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);

                using (MemoryStream memStream = new MemoryStream())
                {
                    await fs.CopyToAsync(memStream);
                    Google.Apis.Storage.v1.Data.Object googleDataObject;
                    googleDataObject = await storageClient.UploadObjectAsync(bucket.Name, "sinProcesar/" + strFileName, "application/pdf", memStream);
                    
                }
                var asyncRequest = new AsyncAnnotateFileRequest
                {
                    InputConfig = new InputConfig
                    {
                        GcsSource = new GcsSource
                        {
                            Uri = $"gs://{bucket.Name}/sinProcesar/{strFileName}"
                        },
                        MimeType = "application/pdf"
                    },
                    OutputConfig = new OutputConfig
                    {
                        BatchSize = 2,
                        GcsDestination = new GcsDestination
                        {
                            Uri = $"gs://{bucket.Name}/procesados/{strFileName.Split('.')[0]}"
                        }
                    }
                };
                asyncRequest.Features.Add(new Feature
                {
                    Type = Feature.Types.Type.DocumentTextDetection
                });

                List<AsyncAnnotateFileRequest> requests = new List<AsyncAnnotateFileRequest>();
                requests.Add(asyncRequest);
                var client = new ImageAnnotatorClientBuilder
                {
                    CredentialsPath = @"D:\Google_API_key_TFM.json"
                }.Build();
                var operation = client.AsyncBatchAnnotateFiles(requests);
                operation.PollUntilCompleted();
            }
            catch (Exception e) { }
        }

        private static async Task downloadFilesGoogle()
        {
            var credential = GoogleCredential.FromFile(@"D:\Google_API_key_TFM.json");
            StorageClient storageClient = await StorageClient.CreateAsync(credential);
            storageClient.Service.HttpClient.Timeout = new TimeSpan(0, 10, 0);
            var bucket = await storageClient.GetBucketAsync("bucket-ocr-tfm");
            var blobList = storageClient.ListObjects(bucket.Name, "procesados/");
            string strPath = @"D:\json\";
            foreach (var outputOcr in blobList.Where(x => x.Name.Contains(".json")))
            {
                using (var stream = File.OpenWrite(strPath + outputOcr.Name.Split('/')[1]))
                {
                    storageClient.DownloadObject(outputOcr, stream);
                }
            }
        }       
    }
}
