using System.Numerics;
using System.Text.Json.Serialization;
using Bencodex;
using Bencodex.Types;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Consensus
{
    public class Validator : IEquatable<Validator>, IBencodable
    {
        private static readonly Binary PublicKeyKey = new Binary(new byte[] { 0x50 }); // 'P'
        private static readonly Binary PowerKey = new Binary(new byte[] { 0x70 });     // 'p'

        public Validator(
            BlsPublicKey publicKey,
            BigInteger power)
        {
            if (power < BigInteger.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(power),
                    $"Given {nameof(power)} cannot be negative: {power}");
            }

            PublicKey = publicKey;
            Power = power;
        }

        public Validator(Bencodex.Types.IValue bencoded)
            : this(bencoded is Bencodex.Types.Dictionary dict
                ? dict
                : throw new ArgumentException(
                    $"Given {nameof(bencoded)} must be of type " +
                    $"{typeof(Bencodex.Types.Dictionary)}: {bencoded.GetType()}",
                    nameof(bencoded)))
        {
        }

        private Validator(Bencodex.Types.Dictionary bencoded)
            : this(
                new BlsPublicKey(((Binary)bencoded[PublicKeyKey]).ByteArray),
                (Integer)bencoded[PowerKey])
        {
        }

        public BlsPublicKey PublicKey { get; }

        public BigInteger Power { get; }

        [JsonIgnore]
        public Bencodex.Types.IValue Bencoded => Dictionary.Empty
            .Add(PublicKeyKey, PublicKey.ByteArray)
            .Add(PowerKey, Power);

        public static bool operator ==(Validator obj, Validator other)
        {
            return obj.Equals(other);
        }

        public static bool operator !=(Validator obj, Validator other)
        {
            return !(obj == other);
        }

        public override bool Equals(object? obj)
        {
            if (obj is Validator other)
            {
                return Equals(other);
            }

            return false;
        }

        public bool Equals(Validator? other)
        {
            return PublicKey.Equals(other?.PublicKey) && Power.Equals(other?.Power);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(PublicKey, Power);
        }

        public override string ToString() => $"{PublicKey}:{Power}";
    }
}