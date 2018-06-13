module TreeDiagram
{
    enum ServerStatus { 
        Normal,
        Fault  
    }
    interface Server
    {
        ServerStatus status();
    }
}