using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Security.Cryptography;
using Libplanet.Common;
using Planetarium.Cryptography.BLS12_381;

namespace VrfTestbed.VrfLib
{
    public sealed class BlsSignature : IEquatable<BlsSignature>
    {
        private const int KeyByteSize = BLS.SIGNATURE_SERIALIZE_SIZE;
        private readonly ImmutableArray<byte> _signature;

        public BlsSignature(IReadOnlyList<byte> signature)
        {
            if (signature is ImmutableArray<byte> i ? i.IsDefaultOrEmpty : !signature.Any())
            {
                throw new ArgumentNullException(
                    nameof(signature), "Signature is empty.");
            }

            if (signature.Count != KeyByteSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(signature),
                    $"The key must be {KeyByteSize} bytes."
                );
            }

            _signature = signature.ToImmutableArray();
            _ = CryptoConfig.ConsensusCryptoBackend.ValidateGetNativeSignature(ToByteArray());
        }

        [Pure]
        public ImmutableArray<byte> ByteArray => _signature;


        [Pure]
        public byte[] ToByteArray() => _signature.ToArray();

        public static bool operator ==(BlsSignature left, BlsSignature right) =>
            left.Equals(right);

        public static bool operator !=(BlsSignature left, BlsSignature right) =>
            !left.Equals(right);

        [Pure]
        public static BlsSignature FromString(string hex)
            => new BlsSignature(GenerateBytesFromHexString(hex));

        public bool Verify(BlsPublicKey[] publicKeys, IReadOnlyList<byte> payload)
        {
            HashDigest<SHA256> hashed = payload switch
            {
                byte[] ma => HashDigest<SHA256>.DeriveFrom(ma),
                ImmutableArray<byte> im => HashDigest<SHA256>.DeriveFrom(im),
                _ => HashDigest<SHA256>.DeriveFrom(payload.ToArray()),
            };

            return CryptoConfig.ConsensusCryptoBackend.Verify(
                this, publicKeys, hashed);
        }

        public bool Equals(BlsSignature? other) =>
            other is { } && _signature.SequenceEqual(other._signature);

        public override bool Equals(object? obj) => obj is BlsPrivateKey other && Equals(other);

        public override int GetHashCode() => ByteUtil.CalculateHashCode(ToByteArray());


        public BlsSignature Aggregate(BlsSignature signature) 
            => CryptoConfig.ConsensusCryptoBackend.AggregateSignature(this, signature);

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