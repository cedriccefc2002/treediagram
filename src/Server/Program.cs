using System;

namespace Server
{
    class Node : TreeDiagram.NodeDisp_
    {
        public override void saveData(string s, Ice.Current current)
        {
            Console.WriteLine(s);
        }
    }
    public class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                using (Ice.Communicator communicator = Ice.Util.initialize(ref args))
                {
                    var adapter =
                        communicator.createObjectAdapterWithEndpoints("TreeDiagramAdapter", "default -h localhost -p 10000");
                    adapter.add(new Node(), Ice.Util.stringToIdentity("Node"));
                    adapter.activate();
                    communicator.waitForShutdown();
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
