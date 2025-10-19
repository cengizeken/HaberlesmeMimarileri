using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaberlesmeMimarisi.Domain.Parsing
{
    /// <summary>
    /// Bir RX frame'ini domain RxMessage nesnesine dönüştürür.
    /// </summary>
    public interface IRxMessageParser
    {
        /// <summary>
        /// buffer[offset .. offset+count) aralığındaki veriyi parse eder.
        /// Başarısızlıkta ArgumentException / InvalidOperationException fırlatabilir.
        /// </summary>
        HaberlesmeMimarisi.Domain.Messages.RxMessage Parse(byte[] buffer, int offset, int count);
    }
}
