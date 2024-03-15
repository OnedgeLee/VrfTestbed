using System.Security.Cryptography;

namespace VrfTestbed.VrfCrypto
{
    public static class CryptoConfig
    {
        private static ICryptoBackend<SHA256>? _cryptoBackend;
        private static IConsensusCryptoBackend<SHA256>? _consensusCryptoBackend;

        public static ICryptoBackend<SHA256> CryptoBackend
        {
            get => _cryptoBackend ??= new DefaultCryptoBackend<SHA256>();
            set => _cryptoBackend = value;
        }

        public static IConsensusCryptoBackend<SHA256> ConsensusCryptoBackend
        {
            get => _consensusCryptoBackend ??= new DefaultConsensusCryptoBackend<SHA256>();
            set => _consensusCryptoBackend = value;
        }
    }
}
