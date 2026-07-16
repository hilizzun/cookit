using CookIt.Api.CustomTokenProviders;
using CookIt.Api.Middleware;
using CookIt.Api.Services.Auth;
using CookIt.Application.BackgroundServices;
using CookIt.Application.Services;
using CookIt.Application.Services.Admin;
using CookIt.Core.Entities;
using CookIt.Core.Interfaces;
using CookIt.Core.Interfaces.Admin;
using CookIt.Core.Interfaces.Auth;
using CookIt.Core.Settings;
using CookIt.Infrastructure;
using CookIt.Infrastructure.Configuration;
using CookIt.Infrastructure.Repositories;
using CookIt.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Minio;
using OllamaSharp;
using Serilog;
using Serilog.Events;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} " +
                       "{Properties:j}{NewLine}{Exception}"
    )
    .WriteTo.Seq(serverUrl: "http://localhost:5341")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint("localhost:9000")
    .WithCredentials("admin", "password")
    .WithSSL(false));

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 1;

    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
    options.Tokens.EmailConfirmationTokenProvider = "emailconfirmation";
})
.AddEntityFrameworkStores<CookItContext>()
.AddDefaultTokenProviders()
.AddTokenProvider<EmailConfirmationTokenProvider<ApplicationUser>>("emailconfirmation");

builder.Services.Configure<DataProtectionTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromHours(2));

builder.Services.Configure<EmailConfirmationTokenProviderOptions>(opt =>
    opt.TokenLifespan = TimeSpan.FromDays(3));

builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddDbContext<DbContext, CookItContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserManager<ApplicationUser>>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<IAdminIngredientService, AdminIngredientService>();
builder.Services.AddScoped<IAdminEquipmentService, AdminEquipmentService>();
builder.Services.AddScoped<IAdminDishTypeService, AdminDishTypeService>();
builder.Services.AddScoped<IAdminUnitService, AdminUnitService>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IMinioImageStorage, MinioImageStorage>();

builder.Services.AddScoped<IIngredientService, IngredientService>();
builder.Services.AddScoped<IEquipmentService, EquipmentService>();
builder.Services.AddScoped<IDishTypeService, DishTypeService>();
builder.Services.AddScoped<IFavoriteService, FavoriteService>();

builder.Services.AddScoped<IDishTypeRepository, DishTypeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IEquipmentRepository, EquipmentRepository>();
builder.Services.AddScoped<IFavoriteRepository, FavoriteRepository>();

builder.Services.AddScoped<IRecipeService, RecipeService>();

builder.Services.AddScoped<IRatingRepository, RatingRepository>();
builder.Services.AddScoped<IRatingService, RatingService>();

builder.Services.AddScoped<IUnitService, UnitService>();
builder.Services.AddScoped<IUnitRepository, UnitRepository>();

builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

builder.Services.AddScoped<IComplaintRepository, ComplaintRepository>();
builder.Services.AddScoped<IComplaintService, ComplaintService>();

builder.Services.AddScoped<IInterestingFactRepository, InterestingFactRepository>();
builder.Services.AddScoped<IFactService, FactService>();
builder.Services.AddHostedService<FactGenerationService>();
builder.Services.AddSingleton<IChatClient>(sp =>
{
    var ollamaClient = new OllamaApiClient(new Uri("http://localhost:11434/"));
    ollamaClient.SelectedModel = "qwen2.5:3b";
    return ollamaClient;
});
builder.Services.AddScoped<IAiService, OllamaService>();

builder.Services.AddHostedService<CommentModerationService>();

builder.Services.AddScoped<IShoppingListRepository, ShoppingListRepository>();
builder.Services.AddScoped<IShoppingListService, ShoppingListService>();

builder.Services.AddScoped<IUserStatisticsService, UserStatisticsService>();

builder.Services.AddScoped<IAchievementService, AchievementService>();


builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddRouting();

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowCredentials()
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders("Location", "X-Pagination");
    });
});

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "CookIt API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        Description = "Ââåäèòå âàø JWT òîêåí â ôîðìàòå Bearer"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
builder.Services.Configure<JwtSettings>(jwtSettings);

var achievementsConfig = new AchievementSettings();
var section = builder.Configuration.GetSection("Achievements");
foreach (var child in section.GetChildren())
{
    var definition = new AchievementDefinition
    {
        Title = child["Title"] ?? "",
        Thresholds = child.GetSection("Thresholds").Get<List<int>>() ?? new List<int>(),
        Icons = child.GetSection("Icons").Get<Dictionary<string, string>>() ?? new Dictionary<string, string>()
    };
    achievementsConfig.Achievements[child.Key] = definition;
}

builder.Services.AddSingleton(achievementsConfig);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
    };
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CookIt API v1");
    });
}

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("User", httpContext.User?.Identity?.Name ?? "anonymous");
        diagnosticContext.Set("RemoteIp", httpContext.Connection.RemoteIpAddress?.ToString());
    };

    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null || httpContext.Response.StatusCode > 499)
            return LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 399)
            return LogEventLevel.Warning;
        return LogEventLevel.Information;
    };
});

app.UseMiddleware<ErrorHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "CookIt API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseMiddleware<UseRefreshTokenFromCookieMiddleware>();
app.UseMiddleware<CheckUserBlockedMiddleware>();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();