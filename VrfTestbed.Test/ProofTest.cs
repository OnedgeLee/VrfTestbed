using System.Numerics;
using VrfTestbed.VrfLib;

namespace VrfTestbed.Test
{
    public class ProofTest
    {
        [Fact]
        public void BinomialQuantileFunction()
        {
            // Drawn Score, Sampled draw, Selection prob, Power
            Assert.Equal(1, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(90)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(10)));

            Assert.Equal(0, Proof.BinomialQuantileFunction(0.404, 0.01, new BigInteger(90)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(10)));

            Assert.Equal(2, Proof.BinomialQuantileFunction(0.8, 0.01, new BigInteger(90)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.8, 0.01, new BigInteger(10)));

            Assert.Equal(2, Proof.BinomialQuantileFunction(0.8, 0.01, new BigInteger(90)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.9, 0.01, new BigInteger(10)));

            Assert.Equal(2, Proof.BinomialQuantileFunction(0.8, 0.01, new BigInteger(90)));
            Assert.Equal(1, Proof.BinomialQuantileFunction(0.905, 0.1, new BigInteger(1)));

            Assert.Equal(2, Proof.BinomialQuantileFunction(0.8, 0.01, new BigInteger(90)));
            Assert.Equal(2, Proof.BinomialQuantileFunction(0.999, 0.01, new BigInteger(10)));

            Assert.Equal(1, Proof.BinomialQuantileFunction(0.5, 0.001, new BigInteger(900)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.001, new BigInteger(100)));

            Assert.Equal(1, Proof.BinomialQuantileFunction(0.5, 0.0001, new BigInteger(9000)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.0001, new BigInteger(1000)));

            Assert.Equal(1, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(70)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(50)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(30)));
            Assert.Equal(0, Proof.BinomialQuantileFunction(0.5, 0.01, new BigInteger(20)));

            Assert.Equal(4, Proof.BinomialQuantileFunction(0.99, 0.01, new BigInteger(90)));

        }
    }
}
