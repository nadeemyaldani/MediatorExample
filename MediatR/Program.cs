using Autofac;
using JetBrains.Annotations;
using MediatR;
using System;
using System.Reflection;
using Autofac.Core;

public class PingCommand : IRequest<PongResponse>
{

}

public class PongResponse
{
    public DateTime TimeStamp { get; set; }
    public PongResponse(DateTime timestamp)
    {
        TimeStamp = timestamp;
    }
}

[UsedImplicitly]
public class PingCommandHandler : IRequestHandler<PingCommand, PongResponse>
{
    public async Task<PongResponse> Handle(PingCommand request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new PongResponse(DateTime.Now))
            .ConfigureAwait(false);
    }
}

public class MediatRProgram
{

    public static async Task Main(string[] args)
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<Mediator>()
            .As<IMediator>()
            .InstancePerLifetimeScope();

        //builder.Register<ServiceFactory>(ctx =>
        //{
        //    var componentContext = ctx.Resolve<IComponentContext>();
        //    return t => componentContext.Resolve(t);
        //});

        builder.RegisterAssemblyTypes(typeof(MediatRProgram).Assembly)
            .AsImplementedInterfaces();

        var container = builder.Build();
        var mediator = container.Resolve<IMediator>();
        var response = await mediator.Send(new PingCommand());
        Console.WriteLine($"We got a response at {response.TimeStamp}");
    }
}
