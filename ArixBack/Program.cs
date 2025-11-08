using ArixBack.Models;
using ArixBack.Services;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.Google;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:5173", "http://localhost:*");
                      });
});

builder.Services.Configure<ArixDatabaseSettings>(
    builder.Configuration.GetSection("ArixDatabase"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<ArixDatabaseSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddSingleton(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var settings = sp.GetRequiredService<IOptions<ArixDatabaseSettings>>().Value;
    return mongoClient.GetDatabase(settings.DatabaseName);
});

builder.Services.AddSingleton<DatabaseService>();
builder.Services.AddSingleton<WeaponService>();
builder.Services.AddSingleton<PlayerService>();
builder.Services.AddSingleton<TokenProvider>();

//authorization for swagger - can be removed later
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter your Bearer token in the format **'Bearer {your token}'**"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });
    
});

builder.Services.AddAuthentication()
.AddJwtBearer("some-scheme", jwtOptions =>
{
    jwtOptions.MetadataAddress = builder.Configuration["Api:MetadataAddress"];
    jwtOptions.Authority = builder.Configuration["Api:Authority"];
    jwtOptions.Audience = builder.Configuration["Api:Audience"];
    jwtOptions.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };

    jwtOptions.MapInboundClaims = false;
});

builder.Services
.AddAuthentication()
.AddCookie(options => {
    options.Cookie.SecurePolicy = Microsoft.AspNetCore.Http.CookieSecurePolicy.SameAsRequest;
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration["Authentication:Google:Client_id"];
    googleOptions.ClientSecret = builder.Configuration["Authentication:Google:Client_secret"];
});
Console.WriteLine(builder.Configuration["Authentication:Google:Client_id"]);
Console.WriteLine(builder.Configuration["Authentication:Google:Client_secret"]);



builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.MapOpenApi();
}

app.UseCors(MyAllowSpecificOrigins);
app.MapControllers();
app.UseHttpsRedirection();

app.MapGet("/", () => "MongoDB connected!");

app.Run();