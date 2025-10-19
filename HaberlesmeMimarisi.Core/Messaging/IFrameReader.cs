using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Core.Messaging
{
    /// <summary>
    /// Chunk'lı okumadan bağımsız olarak, taşıyıcıdan tek bir "frame" döndürür.
    /// Timeout'ta TimeoutException fırlatır.
    /// </summary>
    public interface IFrameReader
    {
        byte[] ReadFrame(int timeoutMs);
        byte[] ReadFrame(int timeoutMs, CancellationToken cancellationToken);
    }
}
