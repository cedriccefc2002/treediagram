using System;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                Task task = IceService.StartIceService(args);
                task.Wait();
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
                return 1;
            }
            return 0;
        }
    }
}
