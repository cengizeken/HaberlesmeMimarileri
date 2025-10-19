namespace HaberlesmeMimarisi.App.Discovery
{
    public interface ICardIdDiscovery
    {
        byte DiscoverCardId(int timeoutMs = 200);
    }
}
