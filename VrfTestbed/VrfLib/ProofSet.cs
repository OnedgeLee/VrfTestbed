using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Numerics;
using VrfTestbed.Consensus;
using VrfTestbed.VrfCrypto;

namespace VrfTestbed.VrfLib
{
    public class ProofSet
    {
        private ImmutableArray<byte> _payload;
        private ConcurrentDictionary<PublicKey, (Proof, BigInteger)> _proofs;
        private (PublicKey, Proof)? _dominantProof;
        private BigInteger _totalPower;

        public ProofSet(IReadOnlyList<byte> payload)
        {
            _payload = payload.ToImmutableArray();
            _proofs = new ConcurrentDictionary<PublicKey, (Proof, BigInteger)>();
            _dominantProof = null;
            _totalPower = BigInteger.Zero;
        }

        public ProofSet(long height, int round)
            : this(new LotMetadata(height, round).ByteArray) { }

        public int Count => _proofs.Count;

        public void Add(PublicKey publicKey, Proof proof, BigInteger power)
        {
            if (proof.Verify(publicKey, _payload.ToArray()) && _proofs.TryAdd(publicKey, (proof, power)))
            {
                _dominantProof = null;
                _totalPower += power;
            }
        }

        public void Add(IEnumerable<(PublicKey, Proof, BigInteger)> proofs)
        {
            foreach (var proof in proofs)
            {
                Add(proof.Item1, proof.Item2, proof.Item3);
            }
        }

        public bool Verify()
        {
            ConcurrentBag<bool> results = new ConcurrentBag<bool>();
            Parallel.ForEach(
                _proofs,
                proof => results.Add(proof.Value.Item1.Verify(proof.Key, _payload.ToArray())));
            return results.All(result => result);
        }

        public (PublicKey, Proof) DominantProof
        {
            get
            {
                if (_proofs.IsEmpty)
                {
                    throw new Exception("Empty Proof");
                }

                if (_dominantProof is { } dominantProof)
                {
                    return dominantProof;
                }
                else
                {
                    _dominantProof = HighestProofs!.MaxBy(proof => proof.Item2);
                    return ((PublicKey, Proof))_dominantProof;
                }
            }
        }

        public int Seed()
            => DominantProof.Item2.Seed();

        private IEnumerable<(PublicKey, Proof)>? HighestProofs
            => _proofs
                .GroupBy(proof => proof.Value.Item1.Draw(1, proof.Value.Item2, _totalPower))
                .MaxBy(group => group.Key)?
                .Select(item => (item.Key, item.Value.Item1));
    }
}
