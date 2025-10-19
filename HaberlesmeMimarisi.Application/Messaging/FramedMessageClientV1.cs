using HaberlesmeMimarisi.Core.Messaging;
using HaberlesmeMimarisi.Domain.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.App.Messaging
{
    /// <summary>
    /// FrameReader ile tek frame okur ve RxMessage'a çevirir.
    /// </summary>
    public sealed class FramedMessageClientV1
    {
        private readonly IMessageTransport _transport;
        private readonly IFrameReader _frameReader;

        public FramedMessageClientV1(IMessageTransport transport, IFrameReader frameReader)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _frameReader = frameReader ?? throw new ArgumentNullException(nameof(frameReader));
        }

        public RxMessage Request(TxMessage tx, int timeoutMs = 200)
        {
            var data = tx.ToBytes();
            _transport.Write(data, 0, data.Length);

            var frame = _frameReader.ReadFrame(timeoutMs);
            // Domain parser: şu an 4-byte bekleyen FromBytes. Eğer frame'iniz daha uzunsa,
            // offset vererek ilk 4 byte'tan RxMessage'ı çıkarabilirsiniz:
            return RxMessage.FromBytes(frame, 0);
        }

        public RxMessage Request(TxMessage tx, int timeoutMs, CancellationToken ct)
        {
            var data = tx.ToBytes();
            _transport.Write(data, 0, data.Length);

            var frame = _frameReader.ReadFrame(timeoutMs, ct);
            return RxMessage.FromBytes(frame, 0);
        }
    }
}
