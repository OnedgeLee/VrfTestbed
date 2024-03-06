using System.Numerics;
using VrfTestbed.Consensus;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Test
{
    public class VrfTest
    {
        [Fact]
        public void DeterministicSortition()
        {
            var privateKeys = new BlsPrivateKey[]
            { 
                new BlsPrivateKey(), 
                new BlsPrivateKey(), 
                new BlsPrivateKey()
            };

            ValidatorSet validatorSet = new ValidatorSet();
            
            foreach (var key in privateKeys)
            {
                validatorSet = validatorSet.Update(new Validator(key.PublicKey, BigInteger.One));
            }

            var payload = new LotMetadata(10, 10).ByteArray;
            var sigs = privateKeys.Select(key => key.Sign(payload));
            var proof = new VrfProof().Aggregate(sigs);

            var proposer = Sortition.Execute(validatorSet, proof.Seed());

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(proposer, Sortition.Execute(validatorSet, proof.Seed()));
            }
        }

        [Fact]
        public void DeterministicSeed()
        {
            var privateKeys = new BlsPrivateKey[]
            {
                new BlsPrivateKey(),
                new BlsPrivateKey(),
                new BlsPrivateKey()
            };

            ValidatorSet validatorSet = new ValidatorSet();

            foreach (var key in privateKeys)
            {
                validatorSet.Update(new Validator(key.PublicKey, BigInteger.One));
            }

            var payload = new LotMetadata(10, 10).ByteArray;
            var sigs = privateKeys.Select(key => key.Sign(payload));
            var proof = new VrfProof().Aggregate(sigs);

            var seed = proof.SeedInt();

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(seed, proof.SeedInt());
            }
        }
    }
}