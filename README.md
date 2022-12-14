# GrpcRemoting

GrpcRemoting is based on CoreRemoting  
https://github.com/theRainbird/CoreRemoting  

GrpcRemoting is (just like CoreRemoting) a way to migrate from .NET Remoting.  

Some limitations:  
Method that return IEnumerable and yield (crashes)  
Method that return IAsyncEnumerable and yield (crashes)  
Async Func's, eg. an argument like Task Message(Func<string, Task<string>> message) (crashes)  

CoreRemoting use websockets while GrpcRemoting is a rewrite (sort of) to use Grpc instead.  
GrpcRemoting only support BinaryFormatter, while CoreRemoting also supported BSON.
Encryption, authentication, session management, DependencyInjection, Linq expression arguments are also remved (maybe some can be added back if demand).
Idea in the future is to support MessagePack or MemoryPack in addition.
Idea is to make it possible to specify formatter on a per method basis, so slowly can migrate away from BinaryFormatter, method by method.
GrpcRemoting does not use .proto files but simply interfaces. Look at the examples for info, there is no documentation.  

Other Rpc framework of interest:

StreamJsonRpc  
https://github.com/microsoft/vs-streamjsonrpc  

ServiceModel.Grpc   
https://max-ieremenko.github.io/ServiceModel.Grpc/  
https://github.com/max-ieremenko/ServiceModel.Grpc  

protobuf-net.Grpc  
https://github.com/protobuf-net/protobuf-net.Grpc  

SignalR.Strong  
https://github.com/mehmetakbulut/SignalR.Strong  

The examples:

Client and Server in .NET Framework 4.8 using Grpc.Core native.

Client and Server in .NET 6.0 using Grpc.Net managed.

BinaryFormatter does not work well between .NET Framework and .NET bcause types are different,
eg. string in .NET is "System.String,System.Private.CoreLib" while in .NET Framework "System.String,mscorlib"

There exists hacks (links may not be relevant):
https://programmingflow.com/2020/02/18/could-not-load-system-private-corelib.html  
https://stackoverflow.com/questions/50190568/net-standard-4-7-1-could-not-load-system-private-corelib-during-serialization/56184385#56184385  

You will need to add some hacks yourself if using BinaryFormatter across .NET Framework and .NET

There should be possible to add more formatters. I hope to add for MessagePack or MemoryPack in the future.

Performance:  
The file copy test:
.NET 4.8 server\client:  
File sent to server and written by server: 18 seconds (why so slow?)  
File read from server and written by client: 11 seconds  

.NET 6.0 server\client:  
File sent to server and written by server: 31 seconds (oh noes...)  
File read from server and written by client: 13 seconds  

There is something fishy here:-)

When calling the server too fast(?) with grpc-dotnet, I get ENHANCE_YOUR_CALM:
Bug filed: https://github.com/grpc/grpc-dotnet/issues/2010
Workaround added: use a hangup sequence
