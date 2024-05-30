using MassTransit;
using NotificationService.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Configure RabbitMQ through MassTransit
builder.Services.AddMassTransit(x =>
{
    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("nt", false));

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMq:Host"], "/", host => 
        {
            host.Username(builder.Configuration.GetValue("RabbitMq:Username", "guest"));
            host.Password(builder.Configuration.GetValue("RabbitMq:Password", "guest"));
        });

        cfg.ConfigureEndpoints(context);
    });
});

// Add SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Configure SignalR
app.MapHub<NotificationHub>("/notifications");

app.Run();
