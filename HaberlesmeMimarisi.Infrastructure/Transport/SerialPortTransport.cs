using System;
using System.IO.Ports;
using System.Threading;
using HaberlesmeMimarisi.Core.Messaging;

namespace HaberlesmeMimarisi.Infrastructure.Transport
{
    public sealed class SerialPortTransport : IMessageTransport
    {
        private readonly SerialPort _port;
        public bool IsOpen => _port?.IsOpen ?? false;

        public SerialPortTransport(string portName, int baudRate = 115200, Parity parity = Parity.None, int dataBits = 8, StopBits stopBits = StopBits.One, int readTimeoutMs = 200)
        {
            _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadTimeout = readTimeoutMs,
                WriteTimeout = 200
            };
        }

        public void Open() { if (!_port.IsOpen) _port.Open(); }
        public void Close() { if (_port.IsOpen) _port.Close(); }
        public int Write(byte[] buffer, int offset, int count) { _port.Write(buffer, offset, count); return count; }

        public int Read(byte[] buffer, int offset, int count, int timeoutMs)
        {
            var deadline = Environment.TickCount + timeoutMs;
            int read = 0;
            while (read < count)
            {
                if (Environment.TickCount > deadline) break;
                if (_port.BytesToRead > 0)
                {
                    read += _port.Read(buffer, offset + read, count - read);
                }
                else Thread.Sleep(1);
            }
            return read;
        }

        public void Dispose() => Close();
    }
}
