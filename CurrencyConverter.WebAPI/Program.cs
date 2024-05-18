
using CurrencyConverter.WebAPI.Interfaces;
using CurrencyConverter.WebAPI.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly.Extensions.Http;
using Polly;
using System.Net;

namespace CurrencyConverter.WebAPI
{
    public class Program
    {

        static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(msg => (msg.StatusCode == HttpStatusCode.InternalServerError || msg.StatusCode == HttpStatusCode.RequestTimeout))
                .WaitAndRetryAsync(2, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var config = builder.Configuration;

            builder.Logging.ClearProviders();
            builder.Logging.AddConsole();

            // Add services to the container.
            builder.Services.AddScoped<IExchangeService, FrankfurterExchange>();
            builder.Services.AddScoped<ICacheService, CacheService>();

            builder.Services.AddControllers();

            builder.Services.AddMemoryCache();

            builder.Services.AddHttpClient("FrankfurterApiClient", client =>
            {
                client.BaseAddress = new Uri(config["FRANK_API"]!.ToString());
                
            })
                .AddPolicyHandler(GetRetryPolicy());
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            
            

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
