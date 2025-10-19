using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Core.Messaging
{
    public sealed class LengthFieldFrameReader : IFrameReader
    {
        private readonly IMessageTransport _transport;
        private readonly int _headerSize;
        private readonly int _lengthFieldOffset;
        private readonly int _lengthFieldSize;
        private readonly bool _lengthIncludesHeader;
        private readonly bool _isBigEndian;
        private readonly int _maxFrameSize;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="transport"></param>
        /// <param name="headerSize">toplam başlık uzunluğu (ör. 4, 6, 8…)</param>
        /// <param name="lengthFieldOffset">başlık içinde uzunluk alanının başlangıcı (0-based)</param>
        /// <param name="lengthFieldSize">uzunluk alanı kaç byte (1/2/4)</param>
        /// <param name="lengthIncludesHeader">uzunluk alanı tüm frame’i içeriyor mu (true) yoksa sadece payload’ı mı (false)</param>
        /// <param name="isBigEndian">uzunluk alanı endian</param>
        /// <param name="maxFrameSize">güvenlik (garip büyük değerlerde kes)</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public LengthFieldFrameReader(
            IMessageTransport transport,
            int headerSize,
            int lengthFieldOffset,
            int lengthFieldSize,
            bool lengthIncludesHeader,
            bool isBigEndian = false,
            int maxFrameSize = 64 * 1024)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));
            _headerSize = headerSize > 0 ? headerSize : throw new ArgumentOutOfRangeException(nameof(headerSize));
            _lengthFieldOffset = lengthFieldOffset;
            _lengthFieldSize = lengthFieldSize;
            _lengthIncludesHeader = lengthIncludesHeader;
            _isBigEndian = isBigEndian;
            _maxFrameSize = maxFrameSize;

            if (_lengthFieldOffset < 0 || _lengthFieldOffset + _lengthFieldSize > _headerSize)
                throw new ArgumentException("Length field must be inside header.");
            if (_lengthFieldSize != 1 && _lengthFieldSize != 2 && _lengthFieldSize != 4)
                throw new ArgumentException("Length field size must be 1, 2, or 4.");
        }

        public byte[] ReadFrame(int timeoutMs) => ReadFrame(timeoutMs, CancellationToken.None);

        public byte[] ReadFrame(int timeoutMs, CancellationToken ct)
        {
            var header = ReadExact(_headerSize, timeoutMs, ct);

            int length = ParseLength(header, _lengthFieldOffset, _lengthFieldSize, _isBigEndian);
            int totalSize = _lengthIncludesHeader ? length : (_headerSize + length);

            if (totalSize < _headerSize || totalSize > _maxFrameSize)
                throw new InvalidOperationException($"LengthFieldFrameReader invalid frame size: {totalSize}");

            var frame = new byte[totalSize];
            Buffer.BlockCopy(header, 0, frame, 0, _headerSize);

            int remaining = totalSize - _headerSize;
            if (remaining > 0)
            {
                var rest = ReadExact(remaining, timeoutMs, ct);
                Buffer.BlockCopy(rest, 0, frame, _headerSize, remaining);
            }

            return frame;
        }

        private byte[] ReadExact(int count, int timeoutMs, CancellationToken ct)
        {
            var buf = new byte[count];
            var deadline = Environment.TickCount + timeoutMs;
            int read = 0;

            while (read < count)
            {
                ct.ThrowIfCancellationRequested();
                if (Environment.TickCount > deadline)
                    throw new TimeoutException($"LengthFieldFrameReader timeout. Need={count}, Got={read}");

                int n = _transport.Read(buf, read, count - read, 5);
                if (n > 0) read += n;
            }
            return buf;
        }

        private static int ParseLength(byte[] header, int offset, int size, bool bigEndian)
        {
            if (size == 1) return header[offset];

            if (size == 2)
            {
                var b0 = header[offset + (bigEndian ? 0 : 1)];
                var b1 = header[offset + (bigEndian ? 1 : 0)];
                return (b0 << 8) | b1;
            }

            // size == 4
            if (bigEndian)
                return (header[offset + 0] << 24) | (header[offset + 1] << 16) | (header[offset + 2] << 8) | header[offset + 3];

            return (header[offset + 3] << 24) | (header[offset + 2] << 16) | (header[offset + 1] << 8) | header[offset + 0];
        }
    }
}
