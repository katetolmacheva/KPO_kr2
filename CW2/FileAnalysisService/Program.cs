using System.Reflection;
using FileAnalysisService.Application.Interfaces;
using FileAnalysisService.Application.Services;
using FileAnalysisService.Domain.Interfaces;
using FileAnalysisService.Infrastructure.Clients;
using FileAnalysisService.Infrastructure.Data;
using FileAnalysisService.Infrastructure.Repositories;
using FileAnalysisService.Infrastructure.WordCloud;
using FileStoringService.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TextAnalysisDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IFileAnalysisRepository, FileAnalysisRepository>();
builder.Services.AddScoped<IWordCloudGenerator, QuickChartWordCloudGenerator>();
builder.Services.AddScoped<IAnalyzeFileService, AnalyzeFileService>();

builder.Services.AddHttpClient<IFileStoringService, FileStorageHttpClient>(client =>
{
    var baseUrl = builder.Configuration.GetValue<string>("FileStoringService:BaseUrl");
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddScoped<AnalyzeFileService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "File Analysis API",
        Version = "v1"
    });


    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);


    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (apiDesc.ActionDescriptor is not ControllerActionDescriptor descriptor)
            return false;

        return descriptor.ControllerTypeInfo.Namespace?.Contains("FileAnalysisService") == true;
    });

    c.AddServer(new OpenApiServer { Url = "/api/analyze" });
});

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<TextAnalysisDbContext>();
    db.Database.EnsureCreated();
}


app.UseSwagger(c =>
{
    c.RouteTemplate = "api/analyze/swagger/{documentName}/swagger.json";
});

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/api/analyze/swagger/v1/swagger.json", "File Analysis API");
    c.RoutePrefix = "api/analyze/swagger";
});

builder.Logging.AddConsole();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.MapControllers();
app.Run();
