namespace HaberlesmeMimarisi.Core.Utils
{
    public static class Bytes
    {
        public static string Hex(byte b) => $"0x{b:X2}";
        public static string Hex(ushort w) => $"0x{w:X4}";
    }
}
