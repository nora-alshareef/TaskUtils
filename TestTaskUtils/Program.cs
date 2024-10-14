using TaskUtils.Service;
using TaskUtils.Workers;
using WebApplication1;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var configuration = builder.Configuration;

// Register services
builder.Services.AddLogging();

// Build the service provider to access the registered services

// Now pass the instances of the handlers to the AddWorkerService method
var tempServiceProvider = builder.Services.BuildServiceProvider();

// make logger for your handlers
var logger= tempServiceProvider.GetRequiredService<ILogger<IHandler>>();

// Configure and add the worker service
// Note: Handlers are  registered as singletons here inside AddWorkerService
// Pass all your handlers that are specified in appsettings.json
builder.Services.AddWorkerService(
    builder.Configuration,
     new HandlerA(logger),
     new HandlerB(logger),
    new HandlerC(logger)
    // Add more handlers as needed
);


// Add Swagger services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

// Build the app
var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
        c.RoutePrefix = string.Empty; // Set Swagger UI at the app's root
    });
}


// Run the application
app.MapControllers();
Thread.Sleep(2000);
app.Run();

//inject all stateful classes that have methods need to be retried