using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace MonaiUnity
{
    public class MonaiInferResponse
    {
        public bool completed = false;
        public Dictionary<string, int> label_names { get; set; }
        public MonaiInferResponseLatencies latencies { get; set; }
        public Stream label { get; set; }

        public static async Task<MonaiInferResponse> ParseInferResponse(MultipartMemoryStreamProvider stream, string outputPath)
        {
            // Contents[0] = params
            var responseParams = await stream.Contents[0].ReadAsStringAsync();
            MonaiInferResponse response = JsonConvert.DeserializeObject<MonaiInferResponse>(responseParams) ?? new MonaiInferResponse();

            // Contents[1] = file
            using (Stream output = File.OpenWrite(outputPath))
            using (Stream responseFileStream = stream.Contents[1].ReadAsStream())
            {
                responseFileStream.CopyTo(output);
            }
            response.label = File.OpenRead(outputPath);
            response.completed = true;

            return response;

        }

    }

    public class MonaiInferResponseLatencies
    {
        public float pre = -1;
        public float infer = -1;
        public float invert = -1;
        public float post = -1;
        public float write = -1;
        public float total = -1;

        public override string ToString()
        {
            string output = "";
            output += "Total response time: " + total + " sec\n";
            output += "Infer time: " + infer + " sec (" + (infer / total) * 100 + " %)\n";
            output += "Preprocessing time: " + pre + " sec (" + (pre / total) * 100 + " %)";
            return output;
        }
    }
}
