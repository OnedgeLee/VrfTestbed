using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using VrfTestbed.VrfCrypto;

namespace VrfTestbed.Consensus
{
    public class Lot : ILotMetadata, IEquatable<Lot>, IBencodable
    {
        private static readonly Binary ProofKey = new Binary(new byte[] { 0x70 }); // 'p'
        private static readonly Binary PublicKeyKey = new Binary(new byte[] { 0x50 }); // 'P'

        private static readonly Codec _codec = new Codec();
        private readonly LotMetadata _metadata;

        public Lot(
            LotMetadata metadata,
            PublicKey publicKey,
            Proof proof)
        {
            if (!publicKey.VerifyProof(_codec.Encode(metadata.Bencoded), proof))
            {
                throw new ArgumentException(
                    $"Given {nameof(proof)} is invalid.",
                    nameof(proof));
            }

            _metadata = metadata;
            PublicKey = publicKey;
            Proof = proof;
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
                new PublicKey(((Binary)encoded[PublicKeyKey]).ByteArray),
                new Proof(((Binary)encoded[ProofKey]).ByteArray))
        {
        }
#pragma warning restore SA1118

        public long Height => _metadata.Height;

        public int Round => _metadata.Round;

        public PublicKey PublicKey { get; }

        public Proof Proof { get; }

        [JsonIgnore]
        public Bencodex.Types.IValue Bencoded
            => ((Bencodex.Types.Dictionary)_metadata.Bencoded)
                .Add(PublicKeyKey, PublicKey.Format(true))
                .Add(ProofKey, Proof.ByteArray);

        [Pure]
        public bool Verify() 
            => PublicKey.VerifyProof(_codec.Encode(_metadata.Bencoded), Proof);

        [Pure]
        public bool Equals(Lot? other)
        {
            return other is Lot lot &&
                _metadata.Equals(lot._metadata) &&
                Proof.Equals(lot.Proof);
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
                Proof.GetHashCode());
        }

        /// <inheritdoc/>
        [Pure]
        public override string ToString()
        {
            var dict = new Dictionary<string, object>
            {
                { "public_key", PublicKey.ToString() },
                { "height", Height },
                { "round", Round },
            };
            return JsonSerializer.Serialize(dict);
        }
    }
}