using HayatiArac.Modules.Advert.Infrastructure;
using HayatiArac.Modules.User.Infrastructure;
using HayatiArac.SharedKernel.Infrastructure;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HayatiArac API", Version = "v1" });
});

// Add Authorization
builder.Services.AddAuthorization();

// Module registrations
var modules = new IModuleRegistration[]
{
    new UserModuleRegistration(),
    new AdvertModuleRegistration()
};

foreach (var module in modules)
{
    module.RegisterServices(builder.Services, builder.Configuration);
}

// MassTransit for integration events
builder.Services.AddMassTransit(x =>
{
    // Register consumers from User module
    x.AddConsumers(typeof(UserModuleRegistration).Assembly);

    // Register consumers from Advert module
    x.AddConsumers(typeof(AdvertModuleRegistration).Assembly);

    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HayatiArac API v1"));
}

app.UseHttpsRedirection();

// Module middleware
foreach (var module in modules)
{
    module.ConfigureMiddleware(app);
}

// Map endpoints
app.MapEndpoints();

app.Run();
