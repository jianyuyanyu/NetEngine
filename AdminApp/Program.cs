using AdminApp.Libraries;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Globalization;
using System.Net.Http;
using System.Threading.Tasks;

namespace AdminApp
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);


            CultureInfo.DefaultThreadCurrentCulture = new CultureInfo("zh-CN");
            CultureInfo.DefaultThreadCurrentUICulture = new CultureInfo("zh-CN");

            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped<HttpInterceptor>();

            builder.Services.AddScoped(sp => new HttpClient(sp.GetRequiredService<HttpInterceptor>())
            {
                //BaseAddress = new Uri(builder.HostEnvironment.BaseAddress.ToLower().Replace("admin", "api"))
                BaseAddress = new Uri("https://localhost:9833/api/")
            });


            builder.Services.AddBlazoredLocalStorage();

            builder.Services.AddAntDesign();

            await builder.Build().RunAsync();
        }
    }
}
