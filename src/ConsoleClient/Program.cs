using TreeDiagram;
using System;

namespace Client
{
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                using (Ice.Communicator communicator = Ice.Util.initialize(ref args))
                {
                    var obj = communicator.stringToProxy("Server:default -h localhost -p 10000");
                    var server = ServerPrxHelper.checkedCast(obj);
                    if (server == null)
                    {
                        throw new ApplicationException("Invalid proxy");
                    }

                    var result = server.listAllTreesAsync().Result;
                    foreach (var tree in result)
                    {
                        Console.WriteLine($"{tree.uuid} {tree.type}");
                    }
                }
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