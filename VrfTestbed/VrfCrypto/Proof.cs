using Libplanet.Common;
using System.Collections.Immutable;
using System.Numerics;
using System.Security.Cryptography;


namespace VrfTestbed.VrfCrypto
{
    public class Proof : IEquatable<Proof>, IComparable<Proof>, IComparable
    {
        private readonly ImmutableArray<byte> _piBytes;
        private ImmutableArray<byte>? _hash;

        public Proof(IReadOnlyList<byte> piBytes)
        {
            _piBytes = piBytes.ToImmutableArray();
        }

        public ImmutableArray<byte> ByteArray => _piBytes;

        public byte[] ToByteArray() => ByteArray.ToArray();

        public ImmutableArray<byte> Hash
        {
            get
            {
                if (_hash is { } hash)
                {
                    return hash;
                }
                else
                {
                    _hash = HashDigest<SHA512>.DeriveFrom(ByteArray).ByteArray;
                    return (ImmutableArray<byte>)_hash;
                }
            }
        }

        public BigInteger HashInt => HashToInt(Hash);

        public bool Verify(PublicKey publicKey, byte[] payload)
            => publicKey.VerifyProof(payload, this);

        public int Seed()
        {
            byte[] seed = Hash.ToArray().Take(4).ToArray();
            return BitConverter.IsLittleEndian
                ? BitConverter.ToInt32(seed.Reverse().ToArray(), 0)
                : BitConverter.ToInt32(seed, 0);
        }

        public BigInteger Draw(int expectedSize, BigInteger power, BigInteger totalPower)
        {
            double targetProb = (double)HashInt / (double)HashToInt(Enumerable.Repeat(byte.MaxValue, 64).ToImmutableArray());

            return BinomialQuantileFunction(targetProb, expectedSize / (double)totalPower, power);
        }

        public static BigInteger BinomialQuantileFunction(double targetProb, double prob, BigInteger nSample)
        {
            // Cumulative binomial distribution
            double cumulativePositiveProb = 0;
            for (BigInteger nPositive = 0; nPositive < nSample; nPositive++)
            {
                // Binomial distribution
                cumulativePositiveProb += BinomialProb((double)nSample, (double)nPositive, prob);

                if (targetProb <= cumulativePositiveProb)
                {
                    return nPositive;
                }
            }

            return nSample;
        }

        private static double BinomialProb(double nSample, double nPositive, double prob)
        {
            return Combination(nSample, nPositive)
                * Math.Pow(prob, nPositive)
                * Math.Pow(1d - prob, nSample - nPositive);
        }

        private static double Combination(double n, double r)
        {
            double nCr = 1;
            for (double i = n; i > n - r; i--)
            {
                nCr *= i;
            }

            for (double i = 1; i <= r; i++)
            {
                nCr /= i;
            }

            return nCr;
        }

        public static BigInteger HashToInt(ImmutableArray<byte> hash)
            => new BigInteger(hash.ToArray(), isUnsigned: true, isBigEndian: false);

        public bool Equals(Proof? other)
            => ByteArray.Equals(other?.ByteArray);

        public override bool Equals(object? obj)
            => Equals(obj as Proof);

        public int CompareTo(Proof? other)
            => other is Proof otherProof
                ? (HashInt - otherProof.HashInt).Sign
                : throw new ArgumentException($"Argument {nameof(other)} is null");

        public int CompareTo(object? obj)
            => obj is Proof otherProof
                ? CompareTo(otherProof)
                : throw new ArgumentException($"Argument {nameof(obj)} is not an ${nameof(Proof)}.", nameof(obj));

        public override int GetHashCode()
            => ByteUtil.CalculateHashCode(ToByteArray());
    }
}
