#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace Server.Services
{
    public class GreeterService : Greeter.GreeterBase
    {
        private ClientsManager clientsManager { get; }
        public GreeterService(ClientsManager clientsManager)
        {
            this.clientsManager = clientsManager;

            this.clientsManager.StartListen();
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply { Message = $"Hello {request.Name}" });
        }

        public override async Task SayHellos(HelloRequest request, IServerStreamWriter<HelloReply> responseStream, ServerCallContext context)
        {     
            clientsManager.Clients.TryAdd(context, responseStream);
            while (!context.CancellationToken.IsCancellationRequested)            
                await Task.Delay(TimeSpan.FromSeconds(1));            
            clientsManager.Clients.TryRemove(context, out var stream);
        }
    }
}
