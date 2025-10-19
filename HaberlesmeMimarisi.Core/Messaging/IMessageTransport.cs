using System;

namespace HaberlesmeMimarisi.Core.Messaging
{
    public interface IMessageTransport : IDisposable
    {
        void Open();
        void Close();
        int Write(byte[] buffer, int offset, int count);
        int Read(byte[] buffer, int offset, int count, int timeoutMs);
        bool IsOpen { get; }
    }
}
