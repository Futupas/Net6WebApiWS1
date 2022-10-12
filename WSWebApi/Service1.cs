namespace WSWebApi
{
    public class Service1
    {
        protected readonly IConfiguration config;
        protected readonly ILogger logger;
        public Service1(
            IConfiguration config,
            ILoggerFactory loggerFactory)
        {
            this.config = config;
            this.logger = loggerFactory.CreateLogger<Service1>();
        }

        public async Task DoSomething()
        {
            logger.LogInformation($"Hello world. MyKey = {config["MyKey"]}");
        }
    }
}
