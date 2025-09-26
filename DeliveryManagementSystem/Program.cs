using AutoMapper;
using DeliveryManagementSystem.BLL.Healpers;
using DeliveryManagementSystem.BLL.Services;
using DeliveryManagementSystem.Core.DTOs;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using DeliveryManagementSystem.DAL.Contexts;
using DeliveryManagementSystem.DAL.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
namespace DeliveryManagementSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(typeof(DeliveryManagementSystem.BLL.Healpers.MappingProfiles).Assembly);

            
            builder.Services.AddScoped<EmailServices>();
            builder.Services.AddScoped<JWTReader>();

            builder.Configuration
              .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            builder.Services.Configure<JwtSettings>(jwtSettings);

            builder.Services.AddIdentity<User, Roles>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new() { Title = "DeliveryManagementSystem API", Version = "v1" });
                c.UseInlineDefinitionsForEnums();
                // JWT Auth config
                c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = Microsoft.OpenApi.Models.ParameterLocation.Header,
                    Description = "Enter: Bearer {your JWT token} (with space)"
                });


                c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
                {
                    {
                        new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                        {
                            Reference = new Microsoft.OpenApi.Models.OpenApiReference
                            {
                                Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
            });

            builder.Services.AddFluentValidationAutoValidation();
            //builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<CreateBookingDTOValidator>();
            // Add Identity



            builder.Services.AddDbContext<ApplicationDbContext>(options =>
           options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));


            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Identity with custom user & role (int as key)
            //builder.Services.AddIdentity<User, Roles>(options =>
            //{
            //    // Optional: Password and User settings
            //    options.Password.RequireDigit = false;
            //    options.Password.RequireUppercase = false;
            //    options.User.RequireUniqueEmail = true;
            //})
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders();

            builder.Services.AddMemoryCache();

            // CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // JWT Settings
            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
            var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

            // AutoMapper - Manual configuration (NO AddAutoMapper method)
            builder.Services.AddSingleton<IMapper>(serviceProvider =>
            {
                var mapperConfiguration = new MapperConfiguration(configure =>
                {
                    configure.AddProfile<MappingProfiles>();
                });

                return mapperConfiguration.CreateMapper();
            });

            // JWT Configuration
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
                    ValidIssuer = jwtSettings["Issuer"], // من الـ config
                    ValidAudience = jwtSettings["Audience"], // من الـ config
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ClockSkew = TimeSpan.Zero,
                    RoleClaimType = ClaimTypes.Role
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated successfully");
                        var claims = context.Principal.Claims.Select(c => $"{c.Type}: {c.Value}");
                        Console.WriteLine($"Claims: {string.Join(", ", claims)}");
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();
            app.UseCors("AllowAll");
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
            app.Run();


            using (var scope = app.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roles = { "User", "RestaurantOwner", "SuperAdmin" };

                foreach (var role in roles)
                {
                    var roleExists = await roleManager.RoleExistsAsync(role);
                    if (!roleExists)
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
        }
    }
}
