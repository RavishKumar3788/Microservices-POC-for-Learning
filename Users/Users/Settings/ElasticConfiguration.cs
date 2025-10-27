namespace Users.Settings
{
    public class ElasticConfiguration
    {
        public required string Uri { get; set; }
        public required string IndexFormat { get; set; }
        public int NumberOfShards { get; set; }
        public int NumberOfReplicas { get; set; }
    }
}
