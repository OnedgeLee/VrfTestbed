using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using Libplanet.Common;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Consensus
{
    public class Lot : ILotMetadata, IEquatable<Lot>, IBencodable
    {
        private static readonly Binary SignatureKey = new Binary(new byte[] { 0x53 }); // 'S'
        private static readonly Binary BlsPublicKeyKey = new Binary(new byte[] { 0x50 }); // 'P'

        private static readonly Codec _codec = new Codec();
        private readonly LotMetadata _metadata;

        public Lot(
            LotMetadata metadata,
            BlsPublicKey blsPublicKey,
            BlsSignature signature)
        {
            if (!blsPublicKey.Verify(_codec.Encode(metadata.Bencoded), signature))
            {
                throw new ArgumentException(
                    $"Given {nameof(signature)} is invalid.",
                    nameof(signature));
            }

            _metadata = metadata;
            BlsPublicKey = blsPublicKey;
            Signature = signature;
        }

        public Lot(Bencodex.Types.IValue bencoded)
            : this(bencoded is Bencodex.Types.Dictionary dict
                ? dict
                : throw new ArgumentException(
                    $"Given {nameof(bencoded)} must be of type " +
                    $"{typeof(Bencodex.Types.Dictionary)}: {bencoded.GetType()}",
                    nameof(bencoded)))
        {
        }

#pragma warning disable SA1118 // The parameter spans multiple lines
        private Lot(Bencodex.Types.Dictionary encoded)
            : this(
                new LotMetadata((IValue)encoded),
                new BlsPublicKey(((Binary)encoded[BlsPublicKeyKey]).ByteArray),
                new BlsSignature(((Binary)encoded[SignatureKey]).ByteArray))
        {
        }
#pragma warning restore SA1118

        public long Height => _metadata.Height;

        public int Round => _metadata.Round;

        public BlsPublicKey BlsPublicKey { get; }

        public BlsSignature Signature { get; }

        [JsonIgnore]
        public Bencodex.Types.IValue Bencoded
            => ((Bencodex.Types.Dictionary)_metadata.Bencoded)
                .Add(BlsPublicKeyKey, BlsPublicKey.ByteArray)
                .Add(SignatureKey, Signature.ByteArray);

        [Pure]
        public bool Verify() 
            => BlsPublicKey.Verify(
                _codec.Encode(_metadata.Bencoded).ToImmutableArray(), Signature);

        [Pure]
        public bool Equals(Lot? other)
        {
            return other is Lot lot &&
                _metadata.Equals(lot._metadata) &&
                Signature.Equals(lot.Signature);
        }

        [Pure]
        public override bool Equals(object? obj)
        {
            return obj is Lot other && Equals(other);
        }

        [Pure]
        public override int GetHashCode()
        {
            return HashCode.Combine(
                _metadata.GetHashCode(),
                ByteUtil.CalculateHashCode(Signature.ToByteArray()));
        }

        /// <inheritdoc/>
        [Pure]
        public override string ToString()
        {
            var dict = new Dictionary<string, object>
            {
                { "bls_public_key", BlsPublicKey.ToString() },
                { "height", Height },
                { "round", Round },
            };
            return JsonSerializer.Serialize(dict);
        }
    }
}