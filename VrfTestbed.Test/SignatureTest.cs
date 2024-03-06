using VrfTestbed.Consensus;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Test
{
    public class SignatureTest
    {
        [Fact]
        public void DeterministicSignature()
        {
            BlsPrivateKey privKey = new BlsPrivateKey();
            LotMetadata lotMetadata = new LotMetadata(0, 0);

            BlsSignature sig = privKey.Sign(lotMetadata.ByteArray);

            for (int i = 0; i < 100; i++)
            {
                Assert.Equal(sig, privKey.Sign(lotMetadata.ByteArray));
            }
        }

        [Fact]
        public void DuplicatedAggregation()
        {
            BlsPrivateKey privKey = new BlsPrivateKey();
            LotMetadata lotMetadata = new LotMetadata(0, 0);

            BlsSignature sig = privKey.Sign(lotMetadata.ByteArray);

            BlsSignature DupAggrSig = sig.Aggregate(sig);
            Assert.NotEqual(sig, DupAggrSig);
            Assert.True(DupAggrSig.Verify(new BlsPublicKey[] { privKey.PublicKey, privKey.PublicKey }, lotMetadata.ByteArray));
            Assert.False(DupAggrSig.Verify(new BlsPublicKey[] { privKey.PublicKey }, lotMetadata.ByteArray));
        }

        [Fact]
        public void CommutativeAggregation()
        {
            BlsPrivateKey privKey1 = new BlsPrivateKey();
            BlsPrivateKey privKey2 = new BlsPrivateKey();
            BlsPrivateKey privKey3 = new BlsPrivateKey();
            LotMetadata lotMetadata = new LotMetadata(0, 0);

            BlsSignature sig1 = privKey1.Sign(lotMetadata.ByteArray);
            BlsSignature sig2 = privKey2.Sign(lotMetadata.ByteArray);

            BlsSignature aggr1 = sig1.Aggregate(sig2);
            BlsSignature aggr2 = sig2.Aggregate(sig1);

            Assert.Equal(aggr1, aggr2);

            Assert.True(aggr1.Verify(new BlsPublicKey[] { privKey1.PublicKey, privKey2.PublicKey }, lotMetadata.ByteArray));
            Assert.True(aggr1.Verify(new BlsPublicKey[] { privKey2.PublicKey, privKey1.PublicKey }, lotMetadata.ByteArray));
            Assert.True(aggr2.Verify(new BlsPublicKey[] { privKey1.PublicKey, privKey2.PublicKey }, lotMetadata.ByteArray));
            Assert.True(aggr2.Verify(new BlsPublicKey[] { privKey2.PublicKey, privKey1.PublicKey }, lotMetadata.ByteArray));

            Assert.False(aggr1.Verify(new BlsPublicKey[] { privKey1.PublicKey, privKey3.PublicKey }, lotMetadata.ByteArray));
            Assert.False(aggr1.Verify(new BlsPublicKey[] { privKey1.PublicKey }, lotMetadata.ByteArray));
        }

        [Fact]
        public void AssociativeAggregation()
        {
            BlsPrivateKey privKey1 = new BlsPrivateKey();
            BlsPrivateKey privKey2 = new BlsPrivateKey();
            BlsPrivateKey privKey3 = new BlsPrivateKey();
            LotMetadata lotMetadata = new LotMetadata(0, 0);

            BlsSignature sig1 = privKey1.Sign(lotMetadata.ByteArray);
            BlsSignature sig2 = privKey2.Sign(lotMetadata.ByteArray);
            BlsSignature sig3 = privKey3.Sign(lotMetadata.ByteArray);

            BlsSignature aggr1 = sig1.Aggregate(sig2).Aggregate(sig3);
            BlsSignature aggr2 = sig2.Aggregate(sig3).Aggregate(sig1);

            Assert.Equal(aggr1, aggr2);

            Assert.True(aggr1.Verify(new BlsPublicKey[] { privKey1.PublicKey, privKey2.PublicKey, privKey3.PublicKey }, lotMetadata.ByteArray));
        }
    }
}