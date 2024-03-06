using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security.Cryptography;
using Libplanet.Common;
using Planetarium.Cryptography.BLS12_381;

namespace VrfTestbed.VrfLib
{
    public class BlsPublicKey : IEquatable<BlsPublicKey>
    {
        private const int KeyByteSize = BLS.PUBLICKEY_SERIALIZE_SIZE;

        private readonly ImmutableArray<byte> _publicKey;

        public BlsPublicKey(IReadOnlyList<byte> publicKey)
        {
            if (publicKey is ImmutableArray<byte> i ? i.IsDefaultOrEmpty : !publicKey.Any())
            {
                throw new ArgumentNullException(
                    nameof(publicKey), "PublicKey is empty.");
            }

            if (publicKey.Count != KeyByteSize)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(publicKey),
                    $"The key must be {KeyByteSize} bytes."
                );
            }

            _publicKey = publicKey.ToImmutableArray();
            _ = CryptoConfig.ConsensusCryptoBackend.ValidateGetNativePublicKey(this);
        }

        public static bool operator ==(BlsPublicKey left, BlsPublicKey right) =>
            left.Equals(right);

        public static bool operator !=(BlsPublicKey left, BlsPublicKey right) =>
            !left.Equals(right);

        public bool Equals(BlsPublicKey? other) =>
            other is { } && _publicKey.SequenceEqual(other._publicKey);

        public override bool Equals(object? obj) => obj is BlsPublicKey other && Equals(other);

        public override int GetHashCode() => ByteUtil.CalculateHashCode(ToByteArray());

        [Pure]
        public bool VerifyProofOfPossession(BlsSignature proofOfPossession) =>
            CryptoConfig.ConsensusCryptoBackend.VerifyPoP(this, proofOfPossession);

        [Pure]
        public byte[] ToByteArray() => _publicKey.ToArray();

        [Pure]
        public ImmutableArray<byte> ByteArray => _publicKey;

        [Pure]
        public bool Verify(IReadOnlyList<byte> payload, BlsSignature signature)
        {
            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (signature == null)
            {
                throw new ArgumentNullException(nameof(signature));
            }


            if (payload is ImmutableArray<byte> i ? i.IsDefaultOrEmpty : !payload.Any())
            {
                throw new ArgumentException("Payload is empty.", nameof(payload));
            }

            HashDigest<SHA256> hashed = HashDigest<SHA256>.DeriveFrom(payload.ToImmutableArray());
            return CryptoConfig.ConsensusCryptoBackend.Verify(
                signature, new BlsPublicKey[] { this }, hashed);
        }

        public override string ToString() => ByteUtil.Hex(ToByteArray());
    }
}