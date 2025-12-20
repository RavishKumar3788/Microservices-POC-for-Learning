namespace Users.Settings
{
    public class RedisConfiguration
    {
        public string Configuration { get; set; } = string.Empty;
        public bool AbortOnConnectFail { get; set; } = true;
    }
}
