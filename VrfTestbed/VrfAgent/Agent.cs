using VrfTestbed.VrfLib;
using VrfTestbed.Consensus;

namespace VrfTestbed.VrfAgent
{
    public class Agent
    {
        protected BlsPrivateKey _blsPrivateKey;
        protected ProofSet _proofSet;
        protected long _height;
        protected int _round;
        protected HashSet<Agent> _peers;
        protected ValidatorSet _validatorSet;

        public Agent(BlsPrivateKey blsPrivateKey)
        {
            _blsPrivateKey = blsPrivateKey;
            _proofSet = new ProofSet(0, 0);
            _peers = new HashSet<Agent>();
            _validatorSet = new ValidatorSet();
        }

        public void AddPeer(Agent agent)
        {
            _peers.Add(agent);
        }

        public void RemovePeer(Agent agent)
        {
            _peers.Remove(agent);
        }

        public void UpdateValidator(Validator validator)
        {
            _validatorSet.Update(validator);
        }

        public void NewRound(long height, int round)
        {
            _height = height;
            _round = round;
            _proofSet = new ProofSet(height, round);
        }

        public Lot GenerateLot(long height, int round)
            => new LotMetadata(height, round).Sign(_blsPrivateKey);

        public void InsertLot()
        {
            BroadCastLot(GenerateLot(_height, _round));
        }

        public void PutLot(Lot lot)
        {
            if (_validatorSet.ContainsPublicKey(lot.BlsPublicKey))
            {
                _proofSet.Add(lot.Signature, lot.BlsPublicKey);
            }
        }

        public void BroadCastLot(Lot lot)
        {
            foreach (Agent peer in _peers)
            {
                SendLot(lot, peer);
            }
        }

        public static void SendLot(Lot lot, Agent agent)
        {
            agent.PutLot(lot);
        }

        public void VerifyProof()
        {
            _proofSet.Verify();
        }

        public int SeedInt() => _proofSet.SeedInt();

        public Validator GetProposer() 
            => Sortition.Execute(_validatorSet, _proofSet.Seed()).First();

        public BlsPublicKey BlsPublicKey => _blsPrivateKey.PublicKey;
    }
}
