using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MonaiUnity
{
    public class MonaiLabel
    {
        public string URL = "http://localhost:8000";
        public string token = "";
        public string SaveDir = GetTemporaryDirectory();
        public bool hasDatastore = false;
        public Dictionary<string, MonaiInferResponse> infers = new Dictionary<string, MonaiInferResponse>();
        public MonaiInfo info { get; set; }
        public MonaiDatastore datastore { get; set; }
        public string[] objectKeys { get; set; }

        public MonaiLabel(string url, string token, string saveDir)
        {
            this.URL = url;
            this.token = token;
            this.SaveDir = saveDir;
        }

        public MonaiLabel(string url, string token)
        {
            this.URL = url;
            this.token = token;
            this.SaveDir = GetTemporaryDirectory();
        }

        public MonaiLabel(string url)
        {
            this.URL = url;
            this.token = "";
            this.SaveDir = GetTemporaryDirectory();
        }

        public MonaiLabel()
        {
            this.URL = "http://localhost:8000";
            this.token = "";
            this.SaveDir = GetTemporaryDirectory();

        }

        public async Task<MonaiInfo> GetInfo()
        {
            using (var client = new HttpClient())
            {
                string uri = this.URL + "/info/";
                string responseBody = await client.GetStringAsync(uri);
                this.info = JsonConvert.DeserializeObject<MonaiInfo>(responseBody) ?? new MonaiInfo();
                return this.info;
            }
        }

        public async Task<MonaiDatastore> GetDatastore(string output = "all")
        {
            using(var client = new HttpClient())
            {
                string uri = this.URL + "/datastore/?output=" + output;
                string responseBody = await client.GetStringAsync(uri);
                this.datastore = JsonConvert.DeserializeObject<MonaiDatastore>(responseBody) ?? new MonaiDatastore();
                this.hasDatastore = true;

                // get all object ids
                List<string> keys = new List<string>();

                foreach (KeyValuePair<string, MonaiDatastoreObject> obj in datastore.objects)
                {
                    keys.Add(obj.Key);
                }

                this.objectKeys = keys.ToArray();
                return this.datastore;
            }
        }

        public async Task<MonaiDatastore> PutDatastore(string ImgId, string AbsolutePath)
        {
            using(var client = new HttpClient())
            {
                // TODO: Only support .nii.gz files atm
                var responseBody = "";
                using (var request = new HttpRequestMessage(new HttpMethod("PUT"), this.URL + "/datastore/image?image=" + ImgId))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");

                    var multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(new StringContent("{}"), "params");

                    var fileContent = new ByteArrayContent(File.ReadAllBytes(AbsolutePath));
                    fileContent.Headers.Add("Content-Type", "application/x-gzip");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(AbsolutePath));

                    request.Content = multipartContent;

                    var httpResponse = await client.SendAsync(request);
                    responseBody = await httpResponse.Content.ReadAsStringAsync();
                }

                MonaiDatastorePutResponse response = JsonConvert.DeserializeObject<MonaiDatastorePutResponse>(responseBody) ?? new MonaiDatastorePutResponse();
                await GetDatastore();

                return this.datastore;
            }
        }

        private class MonaiDatastorePutResponse
        {
            public string image = "";
        }

        public async Task<MonaiInferResponse> InferRemote(string model, string image, string outputFile)
        {
            using (var client = new HttpClient())
            {
                MonaiInferResponse response = new MonaiInferResponse();
                string uri = this.URL + "/infer/" + model + "?image=" + image + "&output=all";
                Console.WriteLine("(POST) " + uri);

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), uri))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");

                    var multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(new StringContent("{}"), "params");
                    multipartContent.Add(new StringContent(""), "file");
                    multipartContent.Add(new StringContent(""), "label");

                    request.Content = multipartContent;
                    var httpResponse = await client.SendAsync(request);
                    Console.WriteLine(httpResponse.StatusCode);

                    // If ReadAsMultipartAsync not found, install...
                    // https://www.nuget.org/packages/microsoft.aspnet.webapi.client
                    // `dotnet add package Microsoft.AspNet.WebApi.Client --version 5.2.9`
                    var multipartResponse = await httpResponse.Content.ReadAsMultipartAsync();

                    // Ref -> MonaiInferResponse.cs
                    response = await MonaiInferResponse.ParseInferResponse(multipartResponse, outputFile);
                }

                // update datastore
                await GetDatastore();

                infers.Add(image, response);
                return response;
            }
        }

        public async Task<MonaiInferResponse> InferLocal(string model, string volumePath, string outputFile, string labelPath = "")
        {
            using (var client = new HttpClient())
            {
                MonaiInferResponse response = new MonaiInferResponse();
                string uri = this.URL + "/infer/" + model + "?output=all";
                Console.WriteLine("(POST) " + uri);

                using (var request = new HttpRequestMessage(new HttpMethod("POST"), uri))
                {
                    request.Headers.TryAddWithoutValidation("accept", "application/json");

                    MultipartFormDataContent multipartContent = new MultipartFormDataContent();
                    multipartContent.Add(new StringContent("{}"), "params");

                    var volumeContent = new ByteArrayContent(File.ReadAllBytes(volumePath));
                    volumeContent.Headers.Add("Content-Type", "application/x-gzip");
                    multipartContent.Add(volumeContent, "file", Path.GetFileName(volumePath));

                    if (labelPath != "")
                    {
                        var labelContent = new ByteArrayContent(File.ReadAllBytes(labelPath));
                        labelContent.Headers.ContentType = new MediaTypeHeaderValue("application/x-gzip");
                        multipartContent.Add(labelContent, "label", Path.GetFileName(labelPath));
                    }

                    request.Content = multipartContent;
                    var httpResponse = await client.SendAsync(request);
                    Console.WriteLine(httpResponse.StatusCode);

                    var multipartResponse = await httpResponse.Content.ReadAsMultipartAsync();
                    response = await MonaiInferResponse.ParseInferResponse(multipartResponse, outputFile);
                }

                // update datastore
                infers.Add(labelPath, response);
                await GetDatastore();
                return response;
            }
        }

        public static string GetTemporaryDirectory()
        {
            string tempDirectory = Path.GetTempPath();
            Directory.CreateDirectory(tempDirectory);
            return tempDirectory;
        }
    }
}
