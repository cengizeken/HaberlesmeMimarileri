using HaberlesmeMimarisi.Domain.Messages;

namespace HaberlesmeMimarisi.App.Messaging
{
    public interface IMessageClient
    {
        RxMessage Request(TxMessage tx, int timeoutMs = 200);
    }
}
