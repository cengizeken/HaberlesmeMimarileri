using HaberlesmeMimarisi.Core.Messaging;
using HaberlesmeMimarisi.Domain.Messages;
using HaberlesmeMimarisi.Domain.Parsing;
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
    /// FrameReader ile frame okur, IRxMessageParser ile parse eder.
    /// </summary>
    public sealed class FramedMessageClient
    {
        private readonly IMessageTransport _transport;
        private readonly IFrameReader _frameReader;
        private readonly IRxMessageParser _parser;

        public FramedMessageClient(IMessageTransport transport, IFrameReader frameReader, IRxMessageParser parser)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _frameReader = frameReader ?? throw new ArgumentNullException(nameof(frameReader));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
        }

        public RxMessage Request(TxMessage tx, int timeoutMs = 200)
        {
            var data = tx.ToBytes();
            _transport.Write(data, 0, data.Length);

            var frame = _frameReader.ReadFrame(timeoutMs);
            return _parser.Parse(frame, 0, frame.Length);
        }

        public RxMessage Request(TxMessage tx, int timeoutMs, CancellationToken ct)
        {
            var data = tx.ToBytes();
            _transport.Write(data, 0, data.Length);

            var frame = _frameReader.ReadFrame(timeoutMs, ct);
            return _parser.Parse(frame, 0, frame.Length);
        }
    }
}
