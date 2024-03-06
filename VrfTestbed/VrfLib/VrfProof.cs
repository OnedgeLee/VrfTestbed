using Libplanet.Common;
using System.Security.Cryptography;

namespace VrfTestbed.VrfLib
{
    public class VrfProof
    {
        private BlsSignature? _blsSignature;

        public VrfProof()
        {
            _blsSignature = null;
        }

        public VrfProof(BlsSignature blsSignature)
        {
            _blsSignature = blsSignature;
        }

        public VrfProof Aggregate(BlsSignature blsSignature)
            => new VrfProof(_blsSignature = _blsSignature is { } sig ? sig.Aggregate(blsSignature) : blsSignature);

        public VrfProof Aggregate(IEnumerable<BlsSignature> blsSignatures)
            => blsSignatures.Aggregate(this, (aggr, next) => aggr.Aggregate(next));

        public bool Verify(BlsPublicKey[] publicKeys, IReadOnlyList<byte> payload)
            => _blsSignature?.Verify(publicKeys, payload) ?? throw new Exception("Empty Proof");

        public byte[] Seed()
            => HashDigest<SHA256>.DeriveFrom(
                _blsSignature?.ToByteArray() ?? throw new Exception("Empty Proof")).ToByteArray();

        public int SeedInt()
        {
            byte[] seed = Seed().Take(4).ToArray();
            return BitConverter.IsLittleEndian
                ? BitConverter.ToInt32(seed.Reverse().ToArray(), 0)
                : BitConverter.ToInt32(seed, 0);
        }
    }
}
