using VrfTestbed.Consensus;
using VrfTestbed.VrfCrypto;
using VrfTestbed.VrfLib;

namespace VrfTestbed.VrfAgent
{
    public class ByzantineAgent : Agent
    {
        private List<Lot> _receivedLots;

        public ByzantineAgent(PrivateKey blsPrivateKey)
            :base(blsPrivateKey) 
        {
            _receivedLots = new List<Lot>();
        }

        public new void NewRound(long height, int round)
        {
            _height = height;
            _round = round;
            _proofSets.TryAdd((height, round), new ProofSet(height, round));
        }


        public new void PushLot(Lot lot)
        {
            _receivedLots.Add(lot);
        }

        public PrivateKey PrivateKey => _privateKey;
    }
}
