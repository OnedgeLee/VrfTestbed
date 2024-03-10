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

                case "add-agent":
                    application.AddAgent();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add-agents").IsMatch(cmd):
                    for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                    {
                        application.AddAgent();
                    }
                    Console.WriteLine("---\n");
                    break;

                case "add-byzantine":
                    application.AddByzantine();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add-byzantines").IsMatch(cmd):
                    for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                    {
                        application.AddByzantine();
                    }
                    Console.WriteLine("---\n");
                    break;

                case "list-agents":
                    application.ListAgents();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list-peers").IsMatch(cmd):
                    application.ListPeers(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case "update-peers":
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("remove-agent").IsMatch(cmd):
                    application.RemoveAgent(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("new-round").IsMatch(cmd):
                    application.NewRound(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                    Console.WriteLine("---\n");
                    break;

                case "seed-all":
                    application.SeedAll();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list-proof-set").IsMatch(cmd):
                    application.ListProofSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add-validator-set").IsMatch(cmd):
                    application.AddValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                    Console.WriteLine("---\n");
                    break;

                case "apply-validator-set":
                    application.ApplyValidatorSet();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list-validator-set").IsMatch(cmd):
                    application.ListValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                    Console.WriteLine("---\n");
                    break;

                case "proposer-all":
                    application.ProposerAll();
                    Console.WriteLine("---\n");
                    break;
            }
        }
    }
}
