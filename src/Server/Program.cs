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
                Task task1 = IceService.StartService(args);
                Task task2 = gRPCService.StartService(args);
                Task.WaitAll(new Task[]{task1, task2});
                // task2.Wait();
                // Console.ReadKey();
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
