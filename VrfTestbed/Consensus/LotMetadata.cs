using System.Collections.Immutable;
using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using VrfTestbed.VrfCrypto;

namespace VrfTestbed.Consensus
{
    public class LotMetadata : ILotMetadata, IEquatable<LotMetadata>, IBencodable
    {
        private static readonly Binary HeightKey =
            new Binary(new byte[] { 0x48 }); // 'H'

        private static readonly Binary RoundKey =
            new Binary(new byte[] { 0x52 }); // 'R'

        private static readonly Codec _codec = new Codec();

        public LotMetadata(
            long height,
            int round)
        {
            if (height < 0)
            {
                throw new ArgumentException(
                    $"Given {nameof(height)} cannot be negative: {height}");
            }
            else if (round < 0)
            {
                throw new ArgumentException(
                    $"Given {nameof(round)} cannot be negative: {round}");
            }

            Height = height;
            Round = round;
        }

        public LotMetadata(Bencodex.Types.IValue bencoded)
            : this(bencoded is Bencodex.Types.Dictionary dict
                ? dict
                : throw new ArgumentException(
                    $"Given {nameof(bencoded)} must be of type " +
                    $"{typeof(Bencodex.Types.Dictionary)}: {bencoded.GetType()}",
                    nameof(bencoded)))
        {
        }

        private LotMetadata(Bencodex.Types.Dictionary bencoded)
            : this((Integer)bencoded[HeightKey], (Integer)bencoded[RoundKey])
        {
        }

        public long Height { get; }

        public int Round { get; }

        [JsonIgnore]
        public Bencodex.Types.IValue Bencoded
        {
            get
            {
                Dictionary bencoded = Bencodex.Types.Dictionary.Empty
                    .Add(HeightKey, Height)
                    .Add(RoundKey, Round);

                return bencoded;
            }
        }

        public byte[] ToByteArray() => new Codec().Encode(Bencoded);

        public ImmutableArray<byte> ByteArray => ToByteArray().ToImmutableArray();

        public Lot Sign(PrivateKey signer)
            => new Lot(this, signer.PublicKey, signer.Prove(_codec.Encode(Bencoded)));

        /// <inheritdoc/>
        public bool Equals(LotMetadata? other)
        {
            return other is LotMetadata metadata &&
                Height == metadata.Height &&
                Round == metadata.Round;
        }

        public override bool Equals(object? obj) =>
            obj is LotMetadata other && Equals(other);

        public override int GetHashCode()
        {
            return HashCode.Combine(
                Height,
                Round);
        }
    }
}