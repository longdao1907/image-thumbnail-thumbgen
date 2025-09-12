using CloudNative.CloudEvents.AspNetCore;
using CloudNative.CloudEvents.NewtonsoftJson;
using Google.Cloud.SecretManager.V1;
using ImageAPI;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using ThumbnailGenerator;
using ThumbnailGenerator.Core.Application.Interfaces;
using ThumbnailGenerator.Core.Application.Services;
using ThumbnailGenerator.Infrastructure.APIClients;
using ThumbnailGenerator.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure EF Core with PostgreSQL
string projectId = builder.Configuration.GetSection("Gcp").GetValue<string>("ProjectID") ?? throw new ArgumentNullException("Gcp Project ID not configured.");
string jwtSecret = builder.Configuration.GetSection("Gcp").GetValue<string>("JWTSecretKey") ?? throw new ArgumentNullException("Gcp JWTSecretKey not configured.");
string secretVersion = builder.Configuration.GetSection("Gcp").GetValue<string>("SecretVersion") ?? throw new ArgumentNullException("Gcp Secret Version not configured.");

//Init Secret Manager Client
SecretManagerServiceClient client = await SecretManagerServiceClient.CreateAsync();

//get the secret value for database connection
SecretVersionName secretVersionName = new(projectId, jwtSecret, secretVersion);
AccessSecretVersionResponse result = await client.AccessSecretVersionAsync(secretVersionName);
string jwtKey = result.Payload.Data.ToStringUtf8();

// Register application and infrastructure services for DI
builder.Services.AddScoped<IStorageService, GcsStorageService>();
builder.Services.AddScoped<IImageProcessor, ImageSharpProcessor>();
builder.Services.AddScoped<IThumbnailService, ThumbnailService>();

// Configure a typed HttpClient for the ImageApiClient
builder.Services.AddHttpClient<IImageApiClient, ImageApiClient>(client =>
{
    var baseUrl = builder.Configuration["ServiceUrls:ImageAPI"];
    if (string.IsNullOrEmpty(baseUrl))
    {
        throw new InvalidOperationException("ImageApi:BaseUrl is not configured.");
    }
    client.BaseAddress = new Uri(baseUrl);
});

builder.Services.AddControllers(opts =>
    opts.InputFormatters.Insert(0, new CloudEventJsonInputFormatter(new JsonEventFormatter())));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
});
builder.AddAppAuthetication(jwtKey);
builder.Services.AddAuthorization();

var app = builder.Build();

// 2. Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add authentication and authorization middleware
app.UseAuthentication();
app.UseAuthorization();

app.UseHttpsRedirection();

app.MapControllers();

app.Run();