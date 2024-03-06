using VrfTestbed.VrfLib;
using VrfTestbed.Consensus;

namespace VrfTestbed.VrfAgent
{
    public class ByzantineAgent : Agent
    {
        private List<Lot> _receivedLots;

        public ByzantineAgent(BlsPrivateKey blsPrivateKey)
            :base(blsPrivateKey) 
        {
            _receivedLots = new List<Lot>();
        }

        public new void PutLot(Lot lot)
        {
            _receivedLots.Add(lot);
        }

        public BlsPrivateKey PrivateKey => _blsPrivateKey;
    }
}
