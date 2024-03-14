using Dasync.Collections;
using System.Collections.Concurrent;
using System.Numerics;
using VrfTestbed.Consensus;

namespace VrfTestbed.VrfLib
{
    public class ProofSet
    {
        private IReadOnlyList<byte> _payload;
        private ConcurrentDictionary<BlsPublicKey, (Proof, BigInteger)> _proofs;
        private (BlsPublicKey, Proof)? _dominantProof;
        private BigInteger _totalPower;
        private bool _verified;

        public ProofSet(IReadOnlyList<byte> payload)
        {
            _payload = payload;
            _proofs = new ConcurrentDictionary<BlsPublicKey, (Proof, BigInteger)>();
            _dominantProof = null;
            _totalPower = BigInteger.Zero;
        }

        public ProofSet(long height, int round)
            : this(new LotMetadata(height, round).ByteArray) { }

        public int Count => _proofs.Count;

        public void Add(BlsPublicKey publicKey, Proof proof, BigInteger power)
        {
            if (proof.Verify(publicKey, _payload) && _proofs.TryAdd(publicKey, (proof, power)))
            {
                _dominantProof = null;
                _totalPower += power;
            }
        }

        public void Add(BlsPublicKey publicKey, BlsSignature signature, BigInteger power)
            => Add(publicKey, new Proof(signature), power);


        public void Add(IEnumerable<(BlsPublicKey, Proof, BigInteger)> proofs)
        {
            foreach (var proof in proofs)
            {
                Add(proof.Item1, proof.Item2, proof.Item3);
            }
        }

        public void Add(IEnumerable<(BlsPublicKey, BlsSignature, BigInteger)> proofs)
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
                proof => results.Add(proof.Value.Item1.Verify(proof.Key, _payload)));
            return results.All(result => result);
        }

        public (BlsPublicKey, Proof) DominantProof
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
                    return ((BlsPublicKey, Proof))_dominantProof;
                }
            }
        }

        public int Seed()
            => DominantProof.Item2.Seed();

        private IEnumerable<(BlsPublicKey, Proof)>? HighestProofs
            => _proofs
                .GroupBy(proof => proof.Value.Item1.Draw(1, proof.Value.Item2, _totalPower))
                .MaxBy(group => group.Key)?
                .Select(item => (item.Key, item.Value.Item1));
    }
}
