using System.Numerics;
using VrfTestbed.Consensus;

namespace VrfTestbed.VrfLib
{
    public static class Sortition
    {
        private static ulong SplitMix64(ulong seed)
        {
            ulong rand = seed + 0x9e3779b97f4a7c15;
            rand = (rand ^ (rand >> 30)) * 0xbf58476d1ce4e5b9;
            rand = (rand ^ (rand >> 27)) * 0x94d049bb133111eb;
            return rand ^ (rand >> 31);
        }

        private static BigInteger RandomThreshold(ulong seed, BigInteger maxThreshold)
        {
            BigInteger rand = new BigInteger(SplitMix64(seed));
            BigInteger threshold = BigInteger.Divide(
                BigInteger.Multiply(rand, maxThreshold),
                new BigInteger(ulong.MaxValue) + BigInteger.One);
            return threshold;
        }

        public static Validator[] Execute(
            ValidatorSet validatorSet,
            byte[] seedBytes,
            int sampleSize = 1)
        {
            ulong seed = BitConverter.ToUInt64(seedBytes, 0);
            BigInteger[] thresholds = new BigInteger[sampleSize];
            for (int i = 0; i < sampleSize; i++)
            {
                thresholds[i] = RandomThreshold(seed, validatorSet.TotalPower);
            }

            Array.Sort(thresholds);
            Validator[] samples = new Validator[sampleSize];
            BigInteger cumulative = BigInteger.Zero;
            int undrawn = 0;

            foreach (Validator validator in validatorSet.Validators)
            {
                while (thresholds[undrawn] < cumulative + validator.Power)
                {
                    samples[undrawn] = validator;
                    undrawn += 1;
                    if (undrawn == sampleSize)
                    {
                        return samples;
                    }
                }

                cumulative += validator.Power;
            }

            throw new InsufficientDrawSortitionException(sampleSize, sampleSize - undrawn);
        }
    }
}
