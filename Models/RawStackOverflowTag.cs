using System.Diagnostics.Eventing.Reader;

namespace ZadanieRekrutacyjne.Models
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Collective
    {
        public List<string> tags { get; set; }
        public List<ExternalLink> external_links { get; set; }
        public string description { get; set; }
        public string link { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class ExternalLink
    {
        public string type { get; set; }
        public string link { get; set; }
    }

    public class Item
    {
        public bool has_synonyms { get; set; }
        public bool is_moderator_only { get; set; }
        public bool is_required { get; set; }
        public int count { get; set; }
        public string name { get; set; }
        public List<Collective> collectives { get; set; }
    }

    public class Root
    {
        public List<Item> items { get; set; }
        public bool has_more { get; set; }
        public int quota_max { get; set; }
        public int quota_remaining { get; set; }
    }


}
