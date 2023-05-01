using Newtonsoft.Json;

namespace MonaiUnity
{
    public class Monai
    {


        static async Task Main()
        {
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                string saveFilesLocally = @"C:\Users\kyle\Documents\GitHub\VolumetricSegmentation-Unity\test\output\";
                MonaiLabel label = new MonaiLabel("http://localhost:8000", "", saveFilesLocally);
                Console.WriteLine("Using temporary directory - ");
                Console.WriteLine(label.SaveDir);
                MonaiInfo info = await label.GetInfo();

                // Print Model Info
                Console.WriteLine("Saving response outputs to: " + label.SaveDir);  // Print tmp directory location
                Console.WriteLine(info.name + "(v" + info.version + ")");
                Console.WriteLine(info.description);
                Console.WriteLine();

                foreach (KeyValuePair<string, MonaiModel> model in info.models)
                {
                    MonaiModel monaiModel = model.Value;
                    Console.WriteLine(model.ToString());
                }

                // Print Datastore Info
                MonaiDatastore datastore = await label.GetDatastore();
                if (!label.hasDatastore)
                {
                    Console.Error.WriteLine("Error retrieving datastore.");
                    return;
                }

                Console.WriteLine(datastore.ToString());

                //// --- INFER REMOTE ---
                //// 

                //// USAGE - label.inferRemote(model, imageId, outputFile)
                //MonaiInferResponse response = await label.InferRemote("deepedit", "spleen_3", "spleen_3-label.nii.gz");
                //Console.WriteLine("Infer Completed");
                //Console.WriteLine(response.latencies.ToString());

                // --- INFER LOCAL ---
                // Outputs resulting nii.gz file to [label.SaveDir]/[outputFile]
                string pathToLocalFile = @"C:\Users\kyle\Documents\GitHub\VolumetricSegmentation-Unity\test\spleen_12.nii.gz";

                // USAGE - label.inferRemote(model, localInputFile, localOutputFile)
                MonaiInferResponse response = await label.InferLocal("deepedit", pathToLocalFile, "local_labels.nii.gz");
                Console.WriteLine("Infer Completed");
                Console.WriteLine(response.latencies.ToString());
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }

    }
}
