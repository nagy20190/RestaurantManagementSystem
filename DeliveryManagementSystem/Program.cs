using DeliveryManagementSystem.BLL.Healpers;
using AutoMapper;
using DeliveryManagementSystem.Core.Entities;
using DeliveryManagementSystem.Core.Interfaces;
using DeliveryManagementSystem.DAL.Contexts;
using DeliveryManagementSystem.DAL.Repositories;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
namespace DeliveryManagementSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthorization();
            builder.Services.AddScoped<IGenericRepository<User>>();

            // Add services to the container.

            builder.Services.AddControllers();
           
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddAutoMapper(typeof(MappingProfiles));

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Fix for CS1061: Add the correct FluentValidation method
            builder.Services.AddFluentValidationAutoValidation();
            //builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<BookingValidator>();
            //builder.Services.AddValidatorsFromAssemblyContaining<CreateBookingDTOValidator>();


            builder.Services.AddDbContext<ApplicationDbContext>(options =>
          options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));

            builder.Services.AddMemoryCache();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
 }
