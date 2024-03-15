using VrfTestbed.Consensus;
using VrfTestbed.VrfCrypto;
using VrfTestbed.VrfLib;

namespace VrfTestbed.VrfAgent
{
    public class DummyAgent : Agent
    {
        public DummyAgent(PrivateKey blsPrivateKey)
            :base(blsPrivateKey) 
        {
        }

        public new void NewRound(long height, int round)
        {
            _height = height;
            _round = round;
            _proofSets.TryAdd((height, round), new ProofSet(height, round));
        }


        public new void PushLot(Lot lot)
        {
        }

        public PrivateKey PrivateKey => _privateKey;
    }
}
