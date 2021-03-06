﻿using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Worker
{
    class Worker
    {
        static void Main(string[] args)
        {
            IConnectionFactory factory = new ConnectionFactory();
            
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "task_queue", 
                                     durable: true, 
                                     exclusive: false, 
                                     autoDelete: false, 
                                     arguments: null);

                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, eventArgs) => {
                    var body = eventArgs.Body;
                    var message = Encoding.UTF8.GetString(body.ToArray());
                    Console.Write(" [x] Received '{0}'", message);

                    int dots = message.Split(".").Length - 1;

                    // 有幾個 . 就等幾秒
                    Thread.Sleep(dots * 1000);
                    Console.WriteLine(" => Finish!");

                    // 手動傳送 Ack
                    channel.BasicAck(deliveryTag: eventArgs.DeliveryTag,
                                     multiple: false);
                };

                // 將 autoAck 改為 false
                // 先前的兩個案例都是 true
                channel.BasicConsume(queue: "task_queue", 
                                     autoAck: false,
                                     consumer: consumer);
                

                Console.WriteLine(" Press [enter] to exit.");
                Console.ReadLine();
            }
        }
    }
}
