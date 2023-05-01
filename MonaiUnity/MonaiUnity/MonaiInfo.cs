using Newtonsoft.Json;

namespace MonaiUnity
{
    public class MonaiInfo
    {
        public string name { get; set; }
        public string description { get; set; }
        public string version { get; set; }
        public List<string> labels { get; set; }

        public Dictionary<string, MonaiModel> models = new Dictionary<string, MonaiModel>();
    }

    public class MonaiModel
    {
        public string type { get; set; }

        public string description { get; set; }

        public int dimension { get; set; }

        // TODO: finish label retreival
        // public Dictionary<string, int> labels { get; set; }

        public override string ToString()
        {
            string output = this.type;
            output += " (" + this.dimension + ") ";
            output += "\"" + (this.description ?? "") + "\"";
            return output;
        }
    }

    public class MonaiModelConfig
    {
        public bool cache_transform = false;
        public bool cache_transforms_in_memory = false;
        public int cache_transforms_ttl = 0;
        // public string[] device = { "cpu" };
    }
}
