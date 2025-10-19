using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Core.Messaging
{
    public sealed class FixedLengthFrameReader : IFrameReader
    {
        private readonly IMessageTransport _transport;
        private readonly int _frameLength;

        public FixedLengthFrameReader(IMessageTransport transport, int frameLength)
        {
            if (frameLength <= 0) throw new ArgumentOutOfRangeException(nameof(frameLength));
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _frameLength = frameLength;
        }

        public byte[] ReadFrame(int timeoutMs) => ReadFrame(timeoutMs, CancellationToken.None);

        public byte[] ReadFrame(int timeoutMs, CancellationToken ct)
        {
            var buf = new byte[_frameLength];
            var deadline = Environment.TickCount + timeoutMs;
            int read = 0;

            while (read < _frameLength)
            {
                ct.ThrowIfCancellationRequested();
                if (Environment.TickCount > deadline)
                    throw new TimeoutException($"FixedLengthFrameReader timeout. Need={_frameLength}, Got={read}");

                int n = _transport.Read(buf, read, _frameLength - read, 5); // kısa poll
                if (n > 0) read += n;
            }
            return buf;
        }
    }
}
