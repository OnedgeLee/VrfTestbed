using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Libplanet.Common;
using Planetarium.Cryptography.BLS12_381;

namespace VrfTestbed.VrfLib
{
    public class BlsPrivateKey : IEquatable<BlsPrivateKey>
    {
        private const int KeyByteSize = BLS.SECRETKEY_SERIALIZE_SIZE;
        private readonly ImmutableArray<byte> _privateKey;
        private BlsPublicKey? _publicKey;
        private BlsSignature? _proofOfPossession;

        public BlsPrivateKey()
        {
            _privateKey = CryptoConfig.ConsensusCryptoBackend.GeneratePrivateKey()
                .ToImmutableArray();
            _proofOfPossession = CryptoConfig.ConsensusCryptoBackend.GetProofOfPossession(this);
        }

        public BlsPrivateKey(IReadOnlyList<byte> privateKey)
        {
            if (privateKey is ImmutableArray<byte> i ? i.IsDefaultOrEmpty : !privateKey.Any())
            {
                throw new ArgumentNullException(
                    nameof(privateKey), "PublicKey is empty.");
            }

            if (privateKey.Count != KeyByteSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(privateKey),
                    $"The key must be {KeyByteSize} bytes."
                );
            }

            if (privateKey.SequenceEqual(new byte[KeyByteSize]))
            {
                throw new ArgumentException(
                    "The zero private key should not be used.",
                    nameof(privateKey));
            }

            _privateKey = privateKey.ToImmutableArray();
            _ = CryptoConfig.ConsensusCryptoBackend.ValidateGetNativePrivateKey(this);
            _proofOfPossession = CryptoConfig.ConsensusCryptoBackend.GetProofOfPossession(this);
        }

        public BlsPrivateKey(string hex)
            : this(GenerateBytesFromHexString(hex))
        {
        }

        public BlsPublicKey PublicKey
        {
            get
            {
                if (_publicKey is null)
                {
                    _publicKey = CryptoConfig.ConsensusCryptoBackend.GetPublicKey(this);
                }

                return _publicKey;
            }
        }

        public BlsSignature ProofOfPossession
        {
            get
            {
                if (_proofOfPossession is null)
                {
                    _proofOfPossession =
                        CryptoConfig.ConsensusCryptoBackend.GetProofOfPossession(this);
                }

                return _proofOfPossession;
            }
        }

        [Pure]
        public ImmutableArray<byte> ByteArray => _privateKey;

        public static bool operator ==(BlsPrivateKey left, BlsPrivateKey right) =>
            left.Equals(right);

        public static bool operator !=(BlsPrivateKey left, BlsPrivateKey right) =>
            !left.Equals(right);

        [Pure]
        public static BlsPrivateKey FromString(string hex) =>
            new BlsPrivateKey(GenerateBytesFromHexString(hex));

        public bool Equals(BlsPrivateKey? other) =>
            other is { } && _privateKey.SequenceEqual(other._privateKey);

        public override bool Equals(object? obj) => obj is BlsPrivateKey other && Equals(other);

        public override int GetHashCode() => ByteUtil.CalculateHashCode(ToByteArray());

        public BlsSignature Sign(byte[] payload)
        {
            HashDigest<SHA256> hashed = HashDigest<SHA256>.DeriveFrom(payload);
            return CryptoConfig.ConsensusCryptoBackend.Sign(hashed, this);
        }

        public BlsSignature Sign(ImmutableArray<byte> message) => Sign(message.ToArray());


        [Pure]
        public byte[] ToByteArray() => _privateKey.ToArray();

        private static byte[] GenerateBytesFromHexString(string hex)
        {
            byte[] bytes = ByteUtil.ParseHex(hex);
            if (bytes.Length != KeyByteSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hex),
                    $"Expected {KeyByteSize * 2} hexadecimal digits."
                );
            }

            return bytes;
        }
    }
}