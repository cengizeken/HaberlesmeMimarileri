using System;
using HaberlesmeMimarisi.Domain.Messages;
using HaberlesmeMimarisi.Core.Messaging;

namespace HaberlesmeMimarisi.App.Discovery
{
    public sealed class CardIdDiscovery : ICardIdDiscovery
    {
        private readonly IMessageTransport _transport;
        public CardIdDiscovery(IMessageTransport transport) { _transport = transport; }

        public byte DiscoverCardId(int timeoutMs = 200)
        {
            // 0xFF-0xCC-0x55-0x55 -> 0xFF-card-0x55-0x55
            var tx = new TxMessage(0xFF, 0xCC, 0x5555);
            var txBytes = tx.ToBytes();
            _transport.Write(txBytes, 0, txBytes.Length);

            var buf = new byte[4];
            var read = _transport.Read(buf, 0, 4, timeoutMs);
            if (read != 4) throw new TimeoutException("CardID read timeout");

            var rx = RxMessage.FromBytes(buf);
            if (rx.RxMessageId != 0xFF || buf[2] != 0x55 || buf[3] != 0x55)
                throw new InvalidOperationException("Unexpected CardID response");

            return rx.CardId;
        }
    }
}
