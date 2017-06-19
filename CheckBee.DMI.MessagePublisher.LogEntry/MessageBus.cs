using CheckBee.Messages.Messages;

namespace CheckBee.DMI.MessagePublisher.LogEntry
{
    public class MessageBus
    {
        //private readonly ConnectionFactory _factory;
        private string _hostName;

        private readonly EnvironmentConfig _environmentConfig;

        public MessageBus(string hostName)
        {
            _hostName = hostName;
        }

        public MessageBus(EnvironmentConfig config)
        {
            _environmentConfig = config;
            //_hostName = GetHost(config);
        }

        public bool Send<T>(T message) where T : LogEntryMessage
        {

            //var endpoint = new MessageCommandEndpoint<M>(_hostName);
            var endpoint = new MessageCommandEndpoint<T>(_environmentConfig);
            endpoint.Send(message);
            return false;
        }


        private static string GetHost(EnvironmentConfig config)
        {
            switch (config)
            {
                case EnvironmentConfig.Local:
                    return "localost";
                case EnvironmentConfig.Dev:
                    return "msp03-aia01p";
                case EnvironmentConfig.Test:
                    return null;
                case EnvironmentConfig.Staging:
                    return null;
                default:
                    return null;
            }
        }

    }
}
