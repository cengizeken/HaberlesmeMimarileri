using System;
using HaberlesmeMimarisi.Core.Messaging;
using HaberlesmeMimarisi.Domain.Messages;

namespace HaberlesmeMimarisi.App.Messaging
{
    public sealed class MessageClient : IMessageClient
    {
        private readonly IMessageTransport _transport;
        public MessageClient(IMessageTransport transport) { _transport = transport; }

        public RxMessage Request(TxMessage tx, int timeoutMs = 200)
        {
            var data = tx.ToBytes();
            _transport.Write(data, 0, data.Length);

            var buf = new byte[4];
            var read = _transport.Read(buf, 0, 4, timeoutMs);
            if (read != 4) throw new TimeoutException("RX timeout");
            return RxMessage.FromBytes(buf, 0);
        }
    }
}