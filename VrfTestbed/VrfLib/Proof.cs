using Libplanet.Common;
using System.Collections.Immutable;
using System.Numerics;
using System.Security.Cryptography;


namespace VrfTestbed.VrfLib
{
    public class Proof
    {
        private readonly BlsSignature _signature;
        private ImmutableArray<byte>? _hash;

        public Proof(BlsSignature signature) 
        {
            _signature = signature;
        }

        public BlsSignature Signature => _signature;

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
                    _hash = HashDigest<SHA512>.DeriveFrom(_signature.ByteArray).ByteArray;
                    return (ImmutableArray<byte>)_hash;
                }
            }
        }

        public bool Verify(BlsPublicKey publicKey, IReadOnlyList<byte> payload)
            => _signature.Verify(new BlsPublicKey[] { publicKey }, payload);

        public int Seed()
        {
            byte[] seed = Hash.ToArray().Take(4).ToArray();
            return BitConverter.IsLittleEndian
                ? BitConverter.ToInt32(seed.Reverse().ToArray(), 0)
                : BitConverter.ToInt32(seed, 0);
        }

        public BigInteger Select(int expectedSize, BigInteger power, BigInteger totalPower)
        {
            double targetProb = (double)new BigInteger(Hash.ToArray(), isUnsigned: true, isBigEndian: false)
                / (double)new BigInteger(Enumerable.Repeat(byte.MaxValue, 64).ToArray(), isUnsigned: true, isBigEndian: false);

            return BinomialQuantileFunction(targetProb, (double)expectedSize / (double)totalPower, power);
        }

        private BigInteger BinomialQuantileFunction(double targetProb, double prob, BigInteger nSample)
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
                * Math.Pow(1d - prob, (nSample - nPositive));
        }

        private static double Combination(double n, double r)
        {
            double nCr = 1;
            for (double i = n; i > (n - r); i--)
            {
                 nCr *= i;
            }

            for (double i = 1; i <= r; i++)
            {
                nCr /= i;
            }
            return nCr;
        }
    }
}
