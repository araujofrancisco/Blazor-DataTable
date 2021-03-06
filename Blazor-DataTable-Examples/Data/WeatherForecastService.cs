using Microsoft.VisualBasic.CompilerServices;
using Sve.Blazor.Core.Models;
using Blazor.DataTable.Components;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace Blazor.DataTable.Examples.Data
{
    public class WeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public Task<WeatherForecast[]> GetForecastAsync(DateTime startDate, int amount = 10)
        {
            var rng = new Random();

            return Task.FromResult(Enumerable.Range(1, amount).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)],
                MyNullableInt = rng.Next(1, 10) > 3 ? (int?)null : 1,
                Country = (Country) Enum.GetValues(typeof(Country)).GetValue(rng.Next(0, Enum.GetValues(typeof(Country)).Length)),
                UpdatedRecently = rng.Next(1, 10) > 3 ? true : false
            }).ToArray());
        }

        private WeatherForecast[]? generatedForecasts = null;
        public Task<Sve.Blazor.Core.Models.PagedResult<WeatherForecast>> SearchForecastAsync(RequestArgs<WeatherForecast> args, int amount = 10)
        {
            Sve.Blazor.Core.Models.PagedResult<WeatherForecast> pagedResult = null;
            Pager pager = null;

            if (generatedForecasts == null)
            {
                var rng = new Random();

                generatedForecasts = Enumerable.Range(1, amount).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(-10).AddDays(index),
                    TemperatureC = rng.Next(-20, 55),
                    Summary = Summaries[rng.Next(Summaries.Length)],
                    MyNullableInt = rng.Next(1, 10) > 3 ? (int?)null : 1,
                    Country = (Country)Enum.GetValues(typeof(Country)).GetValue(rng.Next(0, Enum.GetValues(typeof(Country)).Length)),
                    UpdatedRecently = rng.Next(1, 10) > 3 ? true : false
                }).ToArray();

                pager = new Pager(pageNr: 1, pageSize: 10, sortColumn: "", SortDirection.Ascending);
            }
            else if (args == null && pager == null) pager = new Pager(pageNr: 1, pageSize: 10, sortColumn: "", SortDirection.Ascending); // generatedForecasts isn't cleared when leaving the server side example, so pager could be null
            else pager = args.Pager;
            
            // This is the important part
            IQueryable<WeatherForecast> result = generatedForecasts.AsQueryable();

            if(args != null) result = result.Where(args.GetFilterExpression());

            pagedResult = Sve.Blazor.Core.Utils.ApplyPaging(result, pager);

            return Task.FromResult(pagedResult);
        }
    }
}
