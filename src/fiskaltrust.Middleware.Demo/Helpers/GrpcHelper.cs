using System;
using System.Collections.Generic;
using System.Linq;
using Grpc.Core;
using ProtoBuf.Grpc.Client;
using ProtoBuf.Grpc.Server;

namespace fiskaltrust.Middleware.Demo
{
    public static class GrpcHelper
    {
        public static Server StartHost<T>(List<string> urls, T service) where T : class
        {
            var baseAddresses = new List<Uri>(urls.Select(u => new Uri(u)));
            var server = new Server();
            baseAddresses.ForEach(x => server.Ports.Add(new ServerPort(x.Host, x.Port, ServerCredentials.Insecure)));
            server.Services.AddCodeFirst(service);
            server.Start();
            return server;
        }

        public static T GetClient<T>(string url, int port) where T : class
        {
            var channel = new Channel(url, port, ChannelCredentials.Insecure);
            return channel.CreateGrpcService<T>();
        }
    }
}
