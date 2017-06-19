using CheckBee.Messages.Messages;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CheckBee.DMI.MessagePublisher.LogEntry
{
    public class MessageCommandEndpoint<T> where T : LogEntryMessage
    {
        private readonly ConnectionFactory _factory;
        private readonly string _messageExchange;
        private readonly string _routingKey;
        private readonly EndpointConfiguration _endpointConfiguration;

        // A PUBLISHING endpoint that could be a command for a specific service OR just an event publish
        public MessageCommandEndpoint(string hostName)
        {
            _factory = new ConnectionFactory()
            {
                HostName = hostName
            };
            _messageExchange = GetExchangeFor(typeof(T));
            _routingKey = GetRoutingKeyFor(typeof(T));


        }

        public MessageCommandEndpoint(EnvironmentConfig environmentConfig)
        {
            _endpointConfiguration = GetEndpointConfigurationFor<T>(environmentConfig);

            _factory = new ConnectionFactory()
            {
                HostName = _endpointConfiguration.HostName
            };

            switch (environmentConfig)
            {
                case EnvironmentConfig.Local:
                    _factory.UserName = "guest";
                    _factory.Password = "guest";
                    break;
                case EnvironmentConfig.Dev:
                    //_factory.UserName = "jmoser";
                    //_factory.Password = "Panz3r41$!";
                    _factory.UserName = "checkbee_dev";
                    _factory.Password = "123456";
                    _factory.VirtualHost = "Transactions";
                    break;
                default:
                    break;
            }
        }




        public virtual void Send(T message)
        {

            using (var connection = _factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                var props = channel.CreateBasicProperties();
                props.CorrelationId = Guid.NewGuid().ToString();

                //stamp the message with our queue name
                //message.RoutingKey = _routingKey;
                //message.Message.RoutingKey = _endpointConfiguration.RoutingKey;

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                //channel.BasicPublish(exchange: _messageExchange, routingKey: _routingKey, basicProperties: null,
                //    body: body);

                //configure content-type for Mass Transit/Json so Mass Transit-based subscriber can process the message
                IBasicProperties basicProperties = new BasicProperties
                {
                    MessageId = message.MessageId,
                    ContentType = "application/vnd.masstransit+json",
                    DeliveryMode = 2
                };

                channel.BasicPublish(exchange: _endpointConfiguration.Exchange, routingKey: _endpointConfiguration.RoutingKey, basicProperties: basicProperties,
                    body: body);
                //GetExchangeFor(message.GetType())

                switch (typeof(T).ToString())
                {
                    case "CheckBee.Messages.Messages.LogEntryMessage":
                        Console.WriteLine(" [x] Log entry sent to processor");
                        break;

                    default:
                        Console.WriteLine("Unknown message type...");
                        break;

                }
            }
        }

        private static string GetRoutingKeyFor(Type messageType)
        {
            // this would be in some better structured dictionary to facility dynamic addition at start up.
            switch (messageType.ToString())
            {
                case "dmi.messages.ScanCheck":
                    return "transaction.init";
                default:
                    return null;

            }
        }

        private static string GetExchangeFor(Type messageType)
        {
            switch (messageType.ToString())
            {
                case "dmi.messages.ScanCheck":
                    return "DMI.Transaction";
                default:
                    return null;

            }
        }


        private static EndpointConfiguration GetEndpointConfigurationFor<T>(EnvironmentConfig environmentConfig)
        {
            var ret = EndpointConfigurations()
                .SingleOrDefault(ep =>
                    ep.Environment == environmentConfig &&
                    ep.MessageType == typeof(T)
                );
            return ret;
        }


        public class EndpointConfiguration
        {

            public EnvironmentConfig Environment { get; set; }
            public string HostName { get; set; }
            public Type MessageType { get; set; }

            public string Exchange { get; set; }

            public string RoutingKey { get; set; }
        }


        private static IEnumerable<EndpointConfiguration> EndpointConfigurations()
        {
            List<EndpointConfiguration> configs = new List<EndpointConfiguration>
            {
                new EndpointConfiguration()
                {
                    Environment = EnvironmentConfig.Local,
                    HostName = "localhost",
                    MessageType = typeof(LogEntryMessage),
                    Exchange = "CheckBee.Messages.Messages:LogEntryMessage",
                    RoutingKey = ""
                }

            };

            return configs.AsEnumerable();

        }
    }
}
