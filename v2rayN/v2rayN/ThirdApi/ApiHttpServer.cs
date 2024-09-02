using System.Diagnostics;
using System.Net;
using System.Runtime;
using System.Runtime.InteropServices;
using DotNetty.Codecs.Http;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Libuv;

namespace v2rayN.ThirdApi
{
    public class ApiHttpServer
    {
        private IEventLoopGroup group { get; }
        private IEventLoopGroup workGroup { get; }
        private ServerBootstrap bootstrap { get; }

        private IChannel bootstrapChannel { get; set; }

        private Dictionary<string, IRequestCommand> Commands = new();


        public static string lastIP;

        public ApiHttpServer()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }

            group = new DispatcherEventLoopGroup();
            workGroup = new WorkerEventLoopGroup((DispatcherEventLoopGroup)group);

            bootstrap = new ServerBootstrap();
            bootstrap.Group(group, workGroup);
            bootstrap.Channel<TcpServerChannel>();
            bootstrap
                .Option(ChannelOption.SoBacklog, 8192)
                .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
                {
                    IChannelPipeline pipeline = channel.Pipeline;
                    pipeline.AddLast(new HttpServerCodec()); // 编码解码器
                    pipeline.AddLast(new HttpObjectAggregator(1048576)); // 聚合 HTTP 消息片段
                    pipeline.AddLast("handler", new ServerHandler());
                }));

            Init();

        }

        public void Init()
        {
            //注册所有handler
            Commands = typeof(IRequestCommand).Assembly.GetTypes()
               .Where(p => !p.IsAbstract && typeof(IRequestCommand).IsAssignableFrom(p))
               .Select(p => (IRequestCommand)Activator.CreateInstance(p))
               .ToDictionary(p => p.Path(), p => p);

            Task.Run(async () =>await RunServerAsync());
        }


        public async Task RunServerAsync()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                GCSettings.LatencyMode = GCLatencyMode.SustainedLowLatency;
            }
            bootstrapChannel = await bootstrap.BindAsync(IPAddress.Any, 15002);
            Debug.WriteLine($"Server started and listening on {bootstrapChannel.LocalAddress}");
        }

        public async Task ShutdownAsync()
        {
            if (group != null)
            {
                await group.ShutdownGracefullyAsync();
            }

            if (workGroup != null)
            {
                await workGroup.ShutdownGracefullyAsync();
            }
        }

        public IRequestCommand GetRequestCommand(string path)
        {
            Commands.TryGetValue(path, out var command);
            return command;
        }
    }
}