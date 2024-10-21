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


// Register your handlers
builder.Services.AddSingleton<IHandler, HandlerA>();
builder.Services.AddSingleton<IHandler, HandlerB>();

//here is there workers
builder.Services.AddWorkerService(
    builder.Services.BuildServiceProvider().GetRequiredService<ILogger<IServiceCollection>>(),
    builder.Configuration);


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