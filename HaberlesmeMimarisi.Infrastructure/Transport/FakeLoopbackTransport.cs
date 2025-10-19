using System;
using System.Threading;
using HaberlesmeMimarisi.Core.Messaging;

namespace HaberlesmeMimarisi.Infrastructure.Transport
{
    public sealed class FakeLoopbackTransport : IMessageTransport
    {
        private bool _open;
        private readonly byte _cardId;
        private byte[] _pending;
        public bool IsOpen => _open;
        // YEN?: random kontrol ve RNG
        private readonly bool _randomizeData;
        private readonly Random _rng;
        // YEN?: opsiyonel random parametreleri
        public FakeLoopbackTransport(byte cardId = 0x3A, bool randomizeData = false, int? seed = null)
        {
            _cardId = cardId;
            _randomizeData = randomizeData;
            _rng = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public FakeLoopbackTransport(byte cardId = 0x3A) { _cardId = cardId; }

        public void Open() => _open = true;
        public void Close() => _open = false;
        public int Write(byte[] buffer, int offset, int count)
        {
            if (count >= 4)
            {
                var txId = buffer[offset + 0];
                var card = buffer[offset + 1];
                var lo   = buffer[offset + 2];
                var hi   = buffer[offset + 3];

                if (txId == 0xFF && card == 0xCC && lo == 0x55 && hi == 0x55)
                {
                    _pending = new byte[] { 0xFF, _cardId, 0x55, 0x55 };
                }
                else
                {
                    if (txId == 0x23) _pending = new byte[] { 0x24, _cardId, 0x55, 0x55 };
                    else if (txId == 0x76) _pending = new byte[] { 0x77, _cardId, 0x55, 0x55 };
                    else if (txId == 0xBB) _pending = new byte[] { 0xBC, _cardId, 0x55, 0x55 };
                    else if (txId == 0x33) _pending = new byte[] { 0x34, _cardId, lo, hi };
                    else if (txId == 0x86) _pending = new byte[] { 0x87, _cardId, lo, hi };
                    else if (txId == 0x1B) _pending = new byte[] { 0x1C, _cardId, lo, hi };
                    else if (txId == 0x41) _pending = new byte[] { 0xCB, _cardId, lo, hi };
                    else if (txId == 0xBA) _pending = new byte[] { 0xFA, _cardId, lo, hi };
                    else _pending = new byte[] { (byte)(txId + 1), _cardId, 0x55, 0x55 };
                }
            }
            return count;
        }

        public int ReadV1(byte[] buffer, int offset, int count, int timeoutMs)
        {
            if (_pending == null) { Thread.Sleep(1); return 0; }
            int n = Math.Min(count, _pending.Length);
            Array.Copy(_pending, 0, buffer, offset, n);
            _pending = null;
            return n;
        }
        public int Read(byte[] buffer, int offset, int count, int timeoutMs)
        {
            if (_pending == null) { Thread.Sleep(1); return 0; }

            // YEN?: Anlaml? RX'lerde 2-byte data'y? random üret
            if (_randomizeData)
            {
                byte rxId = _pending[0];
                // YGÖ9 (u16 range) + YGÖ10 (low/high de?erlendirme)
                if (rxId == 0x34 || rxId == 0x87 || rxId == 0x1C || rxId == 0xCB || rxId == 0xFA)
                {
                    ushort value = (ushort)_rng.Next(0, 65536); // 0..65535
                    _pending[2] = (byte)(value & 0xFF);
                    _pending[3] = (byte)(value >> 8);
                }
            }

            int n = Math.Min(count, _pending.Length);
            Array.Copy(_pending, 0, buffer, offset, n);
            _pending = null;
            return n;
        }

        public void Dispose() => Close();
    }
}
