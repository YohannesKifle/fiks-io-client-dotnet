using System;
using KS.Fiks.IO.Client.Encryption;
using KS.Fiks.IO.Client.FileIO;
using KS.Fiks.IO.Client.Models;
using KS.Fiks.IO.Client.Send;
using KS.Fiks.IO.Client.Utility;
using RabbitMQ.Client;

namespace KS.Fiks.IO.Client.Amqp
{
    internal class AmqpReceiveConsumer : DefaultBasicConsumer, IAmqpReceiveConsumer
    {
        private readonly IPayloadDecrypter _decrypter;

        private readonly IFileWriter _fileWriter;

        private readonly ISendHandler _sendHandler;

        public AmqpReceiveConsumer(IModel model, IFileWriter fileWriter, IPayloadDecrypter decrypter, ISendHandler sendHandler)
            : base(model)
        {
            _fileWriter = fileWriter;
            _decrypter = decrypter;
            _sendHandler = sendHandler;
        }

        public event EventHandler<MessageReceivedArgs> Received;

        public override void HandleBasicDeliver(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IBasicProperties properties,
            byte[] body)
        {
            base.HandleBasicDeliver(consumerTag, deliveryTag, redelivered, exchange, routingKey, properties, body);
            var receivedMessage = ParseMessage(routingKey, properties, body);

            if (Received != null)
            {
                Received.Invoke(
                    this,
                    new MessageReceivedArgs(receivedMessage, new ReplySender(_sendHandler, receivedMessage)));
                Model.BasicAck(deliveryTag, false);
            }
        }

        private ReceivedMessage ParseMessage(string routingKey, IBasicProperties properties, byte[] body)
        {
            var metadata = ReceivedMessageParser.Parse(routingKey, properties);
            return new ReceivedMessage(metadata, body, _decrypter, _fileWriter);
        }
    }
}