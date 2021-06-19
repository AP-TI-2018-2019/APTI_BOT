using System.Threading.Tasks;

namespace APTI_BOT
{
    internal class Program
    {
        public static Task Main(string[] args)
        {
            return Startup.RunAsync(args);
        }
    }
}