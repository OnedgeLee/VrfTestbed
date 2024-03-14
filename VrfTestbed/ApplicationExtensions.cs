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
                    try
                    {
                        for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                        {
                            application.AddAgent();
                        }
                        application.UpdatePeers();
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case var cmd when new Regex("add powered agents").IsMatch(cmd):
                    try
                    {
                        for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                        {
                            application.AddAgent();
                            application.UpdateValidatorSet(application.Agents.Count - 1, 1);
                        }
                        application.ApplyValidatorSet();
                        application.UpdatePeers();
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case "add byzantine":
                    application.AddByzantine();
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("add byzantines").IsMatch(cmd):
                    try
                    {
                        for (int i = 0; i < Int32.Parse(Regex.Match(cmd, @"\d+").Value); i++)
                        {
                            application.AddByzantine();
                        }
                        application.UpdatePeers();
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case var cmd when new Regex("remove agent").IsMatch(cmd):
                    try
                    {
                        application.RemoveAgent(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                        application.UpdatePeers();
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

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
                    try
                    {
                        application.ListPeers(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case "update peers":
                    application.UpdatePeers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("new round").IsMatch(cmd):
                    try
                    {
                        application.NewRound(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case "list seeds":
                    application.ListSeeds();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list proof set").IsMatch(cmd):
                    try
                    {
                        application.ListProofSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case var cmd when new Regex("update validator set").IsMatch(cmd):
                    try
                    {
                        application.UpdateValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case "apply validator set":
                    application.ApplyValidatorSet();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("list validator set").IsMatch(cmd):
                    try
                    {
                        application.ListValidatorSet(Int32.Parse(Regex.Match(cmd, @"\d+").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }
                    break;

                case "list proposers":
                    application.ListProposers();
                    Console.WriteLine("---\n");
                    break;

                case var cmd when new Regex("sampling").IsMatch(cmd):
                    try
                    {
                        application.Sampling(Int32.Parse(Regex.Match(cmd, @"\d+").Value), Int32.Parse(Regex.Match(cmd, @"(\d+)(?!.*\d)").Value));
                        Console.WriteLine("---\n");
                    }
                    catch (FormatException e)
                    {
                        Console.WriteLine("Wrong input format");
                    }

                    break;

                case "power from sheet":
                    var dir = Path.GetDirectoryName(
                        System.Reflection.Assembly.GetExecutingAssembly().Location);                    
                    StreamReader sr = new StreamReader(Path.Combine(dir, @"..\..\..\sample_power_sheet.txt"));
                    string? line;
                    int colIdx = 0;
                    do
                    {
                        line = sr.ReadLine();
                        if (colIdx == 0)
                        {
                            application.AddAgent(Convert.ToInt32(line));
                        }
                        else
                        {
                            application.AddDummy(Convert.ToInt32(line));
                        }

                        colIdx++;
                    }
                    while (line != null);
                    application.UpdatePeers();

                    Console.WriteLine("---\n");
                    break;

                default:
                    Console.WriteLine("Invalid command");
                    break;
            }
        }
    }
}
