using VrfTestbed.VrfAgent;

namespace VrfTestbed
{
    public class Application
    {
        private List<Agent> _agents;

        public Application()
        {
            _agents = new List<Agent>();
        }

        public void AddAgent(Agent agent)
        {
            _agents.Add(agent);
        }

        public void UpdatePeers()
        {
            foreach (Agent agent in _agents)
            {
                agent.AddPeer(agent);
            }
        }


    }
}
