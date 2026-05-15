using CardPaymentApi.Middleware;
using CardPaymentApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "Card Payment API",
        Version = "v1",
        Description = "Stripe-backed API for credit and debit card payments."
    });
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

builder.Services.AddScoped<IStripePaymentService, StripePaymentService>();

var app = builder.Build();

app.UseMiddleware<StripeExceptionMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Card Payment API v1");
    options.RoutePrefix = string.Empty; // serve Swagger UI at root
});

app.UseHttpsRedirection();
app.MapControllers();

app.Run();
