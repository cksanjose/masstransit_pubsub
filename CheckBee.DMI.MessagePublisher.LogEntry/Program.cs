using CheckBee.Messages;
using CheckBee.Messages.Messages;
using System;

namespace CheckBee.DMI.MessagePublisher.LogEntry
{
    class Program
    {
        static void Main(string[] args)
        {
            //************************************************
            // Main functions like the DMI Core Process
            //************************************************

            // Connect to the message 'bus'
            //var bus = new MessageBus("localHost");
            var bus = new MessageBus(EnvironmentConfig.Local);


            //*********************************
            // Enter Transaction sending loop.
            // This is Core kicking out endless
            // transactions
            //*********************************
            for (var x = 0; x < 100; x++)
            {
                //***************************************
                // Send the scan check request message
                //***************************************
                var payload = new LogEntryPayload
                {
                    Id = x,
                    MessageBody = "This is a message from RabbitMQ test app",
                    RoutingKey = "",
                    LogType = 1

                };
                bus.Send(new LogEntryMessage
                {
                    ConversationId = Guid.NewGuid().ToString(),
                    MessageId = Guid.NewGuid().ToString(),
                    Message = payload,
                    DestinationAddress = "rabbitmq://localhost", //"rabbitmq://localhost/LogEntryQueue",
                    Headers = { },
                    Host = new HostInfo
                    {
                        Assembly = "CheckBeePublisher",
                        AssemblyVersion = "1.0",
                        FrameworkVersion = "4.0.30319.42000",
                        MachineName = "CFS-LAPTOP-140",
                        MassTransitVersion = "3.5.7.1082",
                        OperatingSystemVersion = "Microsoft Windows NT 6.2.9200.0",
                        ProcessId = 9108,
                        ProcessName = "CheckBeePublisher"
                    },
                    SourceAddress = "",
                    MessageType = new[]
                    {
                        "urn:message:CheckBee.Messages.Messages:LogEntryMessage"
                    }
                });

                // Fake process speed of 1 tx / second
                //System.Threading.Thread.Sleep(1000);
            }
        }
    }
}