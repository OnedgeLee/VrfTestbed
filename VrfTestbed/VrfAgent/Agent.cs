using VrfTestbed.VrfLib;
using VrfTestbed.Consensus;
using System.Collections.Concurrent;

namespace VrfTestbed.VrfAgent
{
    public class Agent
    {
        protected BlsPrivateKey _blsPrivateKey;
        protected ConcurrentDictionary<(long, int), ProofSet> _proofSets;
        protected long _height;
        protected int _round;
        protected HashSet<Agent> _peers;
        protected ValidatorSet _validatorSet;

        public Agent(BlsPrivateKey blsPrivateKey)
        {
            _blsPrivateKey = blsPrivateKey;
            _proofSets = new ConcurrentDictionary<(long, int), ProofSet>();
            _peers = new HashSet<Agent>();
            _validatorSet = new ValidatorSet();
        }

        public ProofSet CurrentProofSet => _proofSets.GetOrAdd((_height, _round), new ProofSet(_height, _round));

        public HashSet<Agent> Peers => _peers;

        public long Height => _height; 

        public int Round => _round;

        public ValidatorSet ValidatorSet => _validatorSet;

        public ConcurrentDictionary<(long, int), ProofSet> ProofSets => _proofSets;

        public bool AddProofSet(long height, int round)
            => _proofSets.TryAdd((height, round), new ProofSet(height, round));

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

        public void UpdateValidatorSet(ValidatorSet validatorSet)
        {
            _validatorSet = validatorSet;
        }

        public void NewRound(long height, int round)
        {
            _height = height;
            _round = round;
            _proofSets.TryAdd((height, round), new ProofSet(height, round));

            InsertLot();
        }

        public Lot GenerateLot(long height, int round)
            => new LotMetadata(height, round).Sign(_blsPrivateKey);

        public void InsertLot()
        {
            BroadCastLot(GenerateLot(_height, _round));
        }

        public void PutLot(Lot lot)
        {
            if (_validatorSet.GetValidator(lot.BlsPublicKey) is Validator validator)
            {
                ProofSet proofSet = _proofSets.GetOrAdd((lot.Height, lot.Round), new ProofSet(lot.Height, lot.Round));
                proofSet.Add(lot.BlsPublicKey, lot.Signature, validator.Power);
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
            CurrentProofSet.Verify();
        }

        public int Seed() => CurrentProofSet.Seed();

        public Validator GetProposer()
            => _validatorSet.GetValidator(CurrentProofSet.DominantProof.Item1)
                ?? throw new Exception("There are no dominant proof");

        public BlsPublicKey BlsPublicKey => _blsPrivateKey.PublicKey;
    }
}
