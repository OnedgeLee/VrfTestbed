using VrfTestbed.VrfLib;
using VrfTestbed.Consensus;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace VrfTestbed.VrfAgent
{
    public class Agent : IDisposable
    {
        protected BlsPrivateKey _blsPrivateKey;
        protected ConcurrentDictionary<(long, int), ProofSet> _proofSets;
        protected long _height;
        protected int _round;
        protected HashSet<Agent> _peers;
        protected ValidatorSet _validatorSet;
        private readonly Channel<Lot> _lotRequests;
        private readonly CancellationTokenSource _cancellationTokenSource;


        public Agent(BlsPrivateKey blsPrivateKey)
        {
            _blsPrivateKey = blsPrivateKey;
            _proofSets = new ConcurrentDictionary<(long, int), ProofSet>();
            _peers = new HashSet<Agent>();
            _validatorSet = new ValidatorSet();
            _lotRequests = Channel.CreateUnbounded<Lot>();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = LotConsumerTask(_cancellationTokenSource.Token);
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
            _ = Task.Run(() => BroadCastLot(GenerateLot(_height, _round)));
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
            agent.PushLot(lot);
        }

        public void PushLot(Lot lot)
        {
            _ = _lotRequests.Writer.WriteAsync(lot);
        }

        internal async Task LotConsumerTask(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    await ConsumeLot(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        private async Task ConsumeLot(CancellationToken cancellationToken)
        {
            Lot lot = await _lotRequests.Reader.ReadAsync(cancellationToken);
            if (_validatorSet.GetValidator(lot.BlsPublicKey) is Validator validator)
            {
                ProofSet proofSet = _proofSets.GetOrAdd((lot.Height, lot.Round), new ProofSet(lot.Height, lot.Round));
                _ = Task.Run(() => proofSet.Add(lot.BlsPublicKey, lot.Signature, validator.Power));
            }
        }

        public void VerifyProof()
        {
            CurrentProofSet.Verify();
        }

        public int Seed() => CurrentProofSet.Seed();

        public Validator GetProposer()
            => _validatorSet.GetValidator(CurrentProofSet.DominantProof.Item1)
                ?? throw new Exception("There are no dominant proof");

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _lotRequests.Writer.TryComplete();
        }

        public BlsPublicKey BlsPublicKey => _blsPrivateKey.PublicKey;
    }
}
