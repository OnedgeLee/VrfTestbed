using VrfTestbed.Consensus;
using VrfTestbed.VrfLib;

namespace VrfTestbed.VrfAgent
{
    public class ProofSet
    {
        private readonly object _lock;

        private VrfProof _proof;

        private List<BlsPublicKey> _publicKeys;

        private IReadOnlyList<byte> _payload;

        public ProofSet(IReadOnlyList<byte> payload) 
        {
            _lock = new object();
            lock (_lock)
            {
                _proof = new VrfProof();
                _publicKeys = new List<BlsPublicKey>();
                _payload = payload;
            }
        }

        public ProofSet(long height, int round)
            : this(new LotMetadata(height, round).ToByteArray())
        {
        }

        public void Add(BlsSignature signature, BlsPublicKey publicKey)
        {
            lock (_lock)
            {
                _proof = _proof.Aggregate(signature);
                _publicKeys.Add(publicKey);
            }
        }

        public bool Verify()
        {
            lock (_lock)
            {
                return _proof.Verify(_publicKeys.ToArray(), _payload);
            }
        }

        public byte[] Seed()
        {
            lock (_lock)
            {
                return _proof.Seed();
            }
        }

        public int SeedInt()
        {
            lock (_lock)
            {
                return _proof.SeedInt();
            }
        }
    }
}
