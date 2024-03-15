namespace VrfTestbed
{
    public class Program
    {
        public static void Main()
        {
            var application = new Application();

            application.Help();
            Console.WriteLine("---\n");
            while (true)
            {
                string command = Console.ReadLine();
                application.ParseCommand(command);
            }
        }
    }
}
