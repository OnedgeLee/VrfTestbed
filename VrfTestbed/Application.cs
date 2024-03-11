﻿using System;
using System.Numerics;
using VrfTestbed.Consensus;
using VrfTestbed.VrfAgent;
using VrfTestbed.VrfLib;

namespace VrfTestbed
{
    public class Application
    {
        private List<Agent> _agents;
        private ValidatorSet _validatorSet;

        public Application()
        {
            _agents = new List<Agent>();
            _validatorSet = new ValidatorSet();
        }

        public List<Agent> Agents => _agents;

        public void Help()
        {
            Console.WriteLine(
@"VrfTestbed Commands
add agent: Add an agent
add powered agent: Add an agent and update validator set
add agents #: Add number of agents
add powered agents #: Add number of agents and update validator set
add byzantine: Add an byzantine agent
add byzantines #: Add number of byzantine agents
list agents: List whole agent (include byzantine)
remove agent #: Remove an agent with index number
remove powered agent #: Remove an agent with index number and update validator set

update peers: Update peers
list peers #: List peers of agent

update validator set # #: Update validator set
apply validator set: Apply validator set to agents
list validator set # : List validator set of an agent

list proof set #: List ProofSets of an agent
list seeds all: List seeds of all agents
list proposers all: List proposers of all agents

new round # #: Start new round with given height, round");
        }

        public void AddAgent()
        {
            try
            {
                Agent agent = new Agent(new BlsPrivateKey());
                Console.WriteLine($"Added new agent : {agent.BlsPublicKey}");
                _agents.Add(agent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void AddByzantine()
        {
            try
            {
                Agent agent = new ByzantineAgent(new BlsPrivateKey());
                Console.WriteLine($"Added new byzantine : {agent.BlsPublicKey}");
                _agents.Add(agent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void ListAgents()
        {
            try
            {
                Console.WriteLine($"Format : index : public key : if byzantine : height : round");
                foreach (var item in _agents.Select((value, index) => (value, index)))
                {
                    if (item.value is ByzantineAgent byzantine)
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Byzantine : {item.value.Height} : {item.value.Round}");
                    }
                    else
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Innocent : {item.value.Height} : {item.value.Round}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void RemoveAgent(int index)
        {
            try
            {
                Console.WriteLine($"Removing agent at {index}: {_agents[index].BlsPublicKey}");
                _agents.RemoveAt(index);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void UpdatePeers()
        {
            try
            {
                Console.WriteLine($"Updating peers");
                foreach (Agent agent in _agents)
                {
                    foreach (Agent peer in _agents)
                    {

                        agent.AddPeer(peer);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void ListPeers(int index)
        {
            try
            {
                foreach (var peer in _agents[index].Peers)
                {
                    Console.WriteLine(peer.BlsPublicKey);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void NewRound(long height, int round)
        {
            try
            {
                Console.WriteLine($"Starting new round: height {height}, round {round}");
                foreach (Agent agent in _agents)
                {
                    agent.NewRound(height, round);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void ListProofSet(int index)
        {
            try
            {
                Console.WriteLine($"Format : height : round");
                foreach (var proofSet in _agents[index].ProofSets)
                {
                    Console.WriteLine($"{proofSet.Key.Item1} : {proofSet.Key.Item2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void UpdateValidatorSet(int index, int power)
        {
            try
            {
                _validatorSet = _validatorSet.Update(new Validator(_agents[index].BlsPublicKey, new BigInteger(power)));
                Console.WriteLine($"Updated validator : {_agents[index].BlsPublicKey} : {power}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void ApplyValidatorSet()
        {
            try
            {
                Console.WriteLine($"Apply validator set to all agents :");
                foreach (var val in _validatorSet.Validators)
                {
                    Console.WriteLine(val);
                }

                foreach (Agent agent in _agents)
                {
                    agent.UpdateValidatorSet(_validatorSet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }          
        }

        public void ListValidatorSet(int index)
        {
            try
            {
                foreach (var val in _agents[index].ValidatorSet.Validators)
                {
                    Console.WriteLine(val);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void SeedAll()
        {
            try
            {
                foreach (var item in _agents.Select((value, index) => (value, index)))
                {
                    if (item.value is ByzantineAgent byzantine)
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Byzantine : {item.value.Seed()}");
                    }
                    else
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Innocent : {item.value.Seed()}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }

        public void ProposerAll()
        {
            try
            {
                foreach (var item in _agents.Select((value, index) => (value, index)))
                {
                    if (item.value is ByzantineAgent byzantine)
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Byzantine : {item.value.GetProposer()}");
                    }
                    else
                    {
                        Console.WriteLine($"{item.index} : {item.value.BlsPublicKey} : Innocent : {item.value.GetProposer()}");
                    }

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{ex.Message}");
            }
        }
    }
}
