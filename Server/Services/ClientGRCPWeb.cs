using Greet;
using Grpc.Core;

namespace Server.Services
{
    public class ClientGRCPWeb
    {
        public ClientGRCPWeb(IServerStreamWriter<HelloReply> stream, ServerCallContext context)
        {
            Stream = stream;
            Context = context;
        }

        public IServerStreamWriter<HelloReply> Stream { get; set; }

        public ServerCallContext Context { get; set; }
    }
}
