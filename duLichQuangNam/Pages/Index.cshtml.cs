using System.Net.Http;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using duLichQuangNam.Models;

public class IndexModel : PageModel
{
    public List<Foods> FoodList { get; set; } = new();
    public List<Tour> TourList { get; set; } = new();
    public List<Stay> StayList { get; set; } = new();
    public List<Destination> DestinationList { get; set; } = new();
    public List<Service> ServiceList { get; set; } = new();

    public async Task OnGetAsync()
    {
        using (var client = new HttpClient())
        {
           
            var foodResponse = await client.GetAsync("https://localhost:7270/api/foods");
            if (foodResponse.IsSuccessStatusCode)
            {
                var json = await foodResponse.Content.ReadAsStringAsync();
                FoodList = JsonConvert.DeserializeObject<List<Foods>>(json)!;
            }

           
            var tourResponse = await client.GetAsync("https://localhost:7270/api/tours");
            if (tourResponse.IsSuccessStatusCode)
            {
                var json = await tourResponse.Content.ReadAsStringAsync();
                TourList = JsonConvert.DeserializeObject<List<Tour>>(json)!;
            }

          
            var stayResponse = await client.GetAsync("https://localhost:7270/api/stays");
            if (stayResponse.IsSuccessStatusCode)
            {
                var json = await stayResponse.Content.ReadAsStringAsync();
                StayList = JsonConvert.DeserializeObject<List<Stay>>(json)!;
            }

            
            var destinationResponse = await client.GetAsync("https://localhost:7270/api/destinations");
            if (destinationResponse.IsSuccessStatusCode)
            {
                var json = await destinationResponse.Content.ReadAsStringAsync();
                DestinationList = JsonConvert.DeserializeObject<List<Destination>>(json)!;
            }

          
            var serviceResponse = await client.GetAsync("https://localhost:7270/api/services");
            if (serviceResponse.IsSuccessStatusCode)
            {
                var json = await serviceResponse.Content.ReadAsStringAsync();
                ServiceList = JsonConvert.DeserializeObject<List<Service>>(json)!;
            }
        }
    }
}