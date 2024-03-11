using System.Text.RegularExpressions;

namespace VrfTestbed
{
    public static class ApplicationExtension
    {
        public static void ParseCommand(this Application application, string command)
        {
            switch (command)
            {
                case "help":
                    application.Help();
                    Console.WriteLine("---\n");
                    break;

                case "add agent":
                    application.AddAgent();
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case "add powered agent":
                    application.AddAgent();
                    application.UpdateValidatorSet(application.Agents.Count - 1, 1);
                    application.ApplyValidatorSet();
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add agents").IsMatch(cmd):
                    for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                    {
                        application.AddAgent();
                    }
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add powered agents").IsMatch(cmd):
                    for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                    {
                        application.AddAgent();
                        application.UpdateValidatorSet(application.Agents.Count - 1, 1);
                    }
                    application.ApplyValidatorSet();
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case "add byzantine":
                    application.AddByzantine();
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add byzantines").IsMatch(cmd):
                    for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                    {
                        application.AddByzantine();
                    }
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("remove agent").IsMatch(cmd):
                    application.RemoveAgent(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("remove powered agent").IsMatch(cmd):
                    var index = Int32.Parse(Regex.Match(cmd, @"\d+").Value);
                    var agent = application.Agents[index];
                    application.UpdateValidatorSet(index, 0);
                    application.RemoveAgent(index);
                    application.ApplyValidatorSet();
                    Console.WriteLine("---\n");
                    break;

                case "list agents":
                    application.ListAgents();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list peers").IsMatch(cmd):
                    application.ListPeers(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case "update peers":
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("new round").IsMatch(cmd):
                    application.NewRound(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                    Console.WriteLine("---\n");
                    break;

                case "list seeds all":
                    application.ListSeeds();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list proof set").IsMatch(cmd):
                    application.ListProofSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("update validator set").IsMatch(cmd):
                    application.UpdateValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                    Console.WriteLine("---\n");
                    break;

                case "apply validator set":
                    application.ApplyValidatorSet();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list validator set").IsMatch(cmd):
                    application.ListValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case "list proposers all":
                    application.ListProposers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("sampling").IsMatch(cmd):
                    application.Sampling(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                    Console.WriteLine("---\n");
                    break;

                default:
                    Console.WriteLine("Invalid command");
                    break;
            }
        }
    }
}
