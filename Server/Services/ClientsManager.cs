using Greet;
using Grpc.Core;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;

namespace Server.Services
{
    public class ClientsManager : IDisposable
    {

        public ConcurrentDictionary<ServerCallContext, IServerStreamWriter<HelloReply>> Clients { get; set; } = new ConcurrentDictionary<ServerCallContext, IServerStreamWriter<HelloReply>>();

        private IConnection connection { get; }
        private IModel channel { get; }

        public ClientsManager()
        {
            timer = new Timer(2500);
            timer.Elapsed += (sender, e) => HandleTimer();
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
        }

        public void StartListen()
        {
            if (!timer.Enabled)
                timer.Start();

            channel.QueueDeclare(queue: "testing", durable: false, exclusive: false, autoDelete: false, arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                foreach (var item in Clients.Where(w => !w.Key.CancellationToken.IsCancellationRequested))
                {
                    await item.Value.WriteAsync(new Greet.HelloReply()
                    {
                        Message = message
                    });
                }
            };
            channel.BasicConsume(queue: "testing", autoAck: true, consumer: consumer);
        }

        Timer timer { get; set; }
        void HandleTimer()
        {
            channel.QueueDeclare(queue: "testing", durable: false, exclusive: false, autoDelete: false, arguments: null);
            string message = JsonSerializer.Serialize(new { Date = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString(), Value =  DateTime.Now.Millisecond * 55.54M  });
            var body = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: "testing", basicProperties: null, body: body);
        }

        public void Dispose()
        {
            channel.Dispose();
            connection.Dispose();
        }
    }
}
