using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MonaiUnity
{
    public class MonaiDatastore
    {
        public string name = "Name";
        public string description = "Description";
        public string images_dir = "";
        public string labels_dir = "";
        public Dictionary<string, MonaiDatastoreObject> objects { get; set; }

        public override string ToString()
        {
            string output = name + " (" + description + ")";

            if (objects != null)
            {
                Console.WriteLine("Printing Available Objects (volumes + labels)");
                foreach (KeyValuePair<string, MonaiDatastoreObject> obj in objects)
                {
                    MonaiDatastoreObject monaiObj = obj.Value;
                    Console.WriteLine(monaiObj.image?.info.name);
                }
            }
            return output;
        }
    }

    public class MonaiDatastoreObject
    {
        public MonaiDatastoreImage? image { get; set; }
        public MonaiDatastoreLabel? label { get; set; }
    }

    public class MonaiDatastoreImage
    {
        public string ext = ".nii.gz";
        public MonaiDatastoreInfo info { get; set; }

    }

    public class MonaiDatastoreInfo
    {
        public string name = string.Empty;
        public int ts { get; set; }
    }

    public class MonaiDatastoreLabel
    {
        // TODO: fill this out
    }
}
