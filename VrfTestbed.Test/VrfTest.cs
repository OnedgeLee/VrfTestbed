using System.Numerics;
using VrfTestbed.Consensus;
using VrfTestbed.VrfCrypto;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Test
{
    public class VrfTest
    {
        public ProofSet ConstructProofSet(long height, int round, IEnumerable<PrivateKey> privateKeys)
        {
            var payload = new LotMetadata(height, round).ByteArray;
            var sigs = privateKeys.Select(key => key.Sign(payload));
            var proofSet = new ProofSet(payload);
            foreach (var pk in privateKeys)
            {
                proofSet.Add(pk.PublicKey, pk.Prove(payload), BigInteger.One);
            }

            return proofSet;
        }


        [Fact]
        public void DeterministicSortition()
        {
            var privateKeys = new PrivateKey[]
            { 
                new PrivateKey(), 
                new PrivateKey(), 
                new PrivateKey()
            };

            ValidatorSet validatorSet = new ValidatorSet();
            
            foreach (var key in privateKeys)
            {
                validatorSet = validatorSet.Update(new Validator(key.PublicKey, BigInteger.One));
            }

            var proofSet = ConstructProofSet(10, 10, privateKeys);
            var proposer = proofSet.DominantProof.Item1;

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(proposer, ConstructProofSet(10, 10, privateKeys).DominantProof.Item1);
            }
        }

        [Fact]
        public void DeterministicSeed()
        {
            var privateKeys = new PrivateKey[]
            {
                new PrivateKey(),
                new PrivateKey(),
                new PrivateKey()
            };

            ValidatorSet validatorSet = new ValidatorSet();

            foreach (var key in privateKeys)
            {
                validatorSet.Update(new Validator(key.PublicKey, BigInteger.One));
            }

            var proofSet = ConstructProofSet(10, 10, privateKeys);
            var seed = proofSet.Seed();

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(seed, ConstructProofSet(10, 10, privateKeys).DominantProof.Item2.Seed());
            }
        }
    }
}