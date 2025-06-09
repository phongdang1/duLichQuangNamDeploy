using System.Net.Http.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace duLichQuangNam.Pages
{
    [Authorize(Roles = "admin")]
    public class DashboardModel : PageModel
    {
        private readonly IHttpClientFactory _factory;
        private readonly ILogger<DashboardModel> _logger;

        public DashboardModel(IHttpClientFactory factory, ILogger<DashboardModel> logger)
        {
            _factory = factory;
            _logger = logger;
        }

        public record CardVM(string Title, int Count, string ListUrl);
        public record SimpleItem(string Name, string Description, string ImgUrl);
        public record SectionVM(
            string Title,
            string CreatePage,
            string ListUrl,
            List<SimpleItem> Items,
            int CurrentPage,
            int TotalPages);

        public List<CardVM> Cards { get; set; } = new();
        public List<SectionVM> Sections { get; set; } = new();

        public int PageSize { get; set; } = 2;

  
        public int PageDestination { get; set; } = 1;
        public int PageTour { get; set; } = 1;
        public int PageStay { get; set; } = 1;
        public int PageFood { get; set; } = 1;
        public int PageService { get; set; } = 1;
        public int PageUser { get; set; } = 1;

        public async Task OnGetAsync(
            int pageDestination = 1,
            int pageTour = 1,
            int pageStay = 1,
            int pageFood = 1,
            int pageService = 1,
            int pageUser = 1)
        {
            PageDestination = pageDestination;
            PageTour = pageTour;
            PageStay = pageStay;
            PageFood = pageFood;
            PageService = pageService;
            PageUser = pageUser;

            var modules = new[]
            {
                new {title="Destination", api="destinations", list="/destinationManager", create="/CreateDestinationModel", page=PageDestination},
                new {title="Tour",      api="tours",        list="/tourManager",        create="/CreateTourModel", page=PageTour},
                new {title="Stay",      api="stays",        list="/stayManager",        create="/CreateStayModel", page=PageStay},
                new {title="Food",      api="foods",        list="/foodsManager",       create="/CreateFoodModel", page=PageFood},
                new {title="Service",   api="services",     list="/serviceManager",     create="/CreateServiceModel", page=PageService},
                new {title="Users",     api="users",        list="/usersManager",       create="/CreateUserModel", page=PageUser}
            };

            var client = _factory.CreateClient("Backend");

            foreach (var m in modules)
            {
                try
                {
                    var url = $"https://dulichquangnamdeploy.onrender.com/api/{m.api}";
                    var data = await client.GetFromJsonAsync<List<ApiItem>>(url) ?? new();

                    Cards.Add(new CardVM(m.title, data.Count, m.list));

                    int totalItems = data.Count;
                    int totalPages = (int)Math.Ceiling(totalItems / (double)PageSize);

                    int currentPage = m.page;
                    if (currentPage < 1) currentPage = 1;
                    if (currentPage > totalPages) currentPage = totalPages;

                    var pagedItems = data
                        .Skip((currentPage - 1) * PageSize)
                        .Take(PageSize)
                        .Select(x => new SimpleItem(
                            x.Name,
                            x.Description ?? "",
                            x.Images?.FirstOrDefault(i => i.IsPrimary)?.ImgUrl ?? "/images/no-image.png"))
                        .ToList();

                    Sections.Add(new SectionVM(m.title, m.create, m.list, pagedItems, currentPage, totalPages));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error call API {Api}", m.api);
                    Cards.Add(new CardVM(m.title, 0, m.list));
                    Sections.Add(new SectionVM(m.title, m.create, m.list, new(), 1, 1));
                }
            }
        }

  
        private class ApiItem
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public string? Description { get; set; }
            public List<ApiImg>? Images { get; set; }
        }
        private class ApiImg
        {
            public string ImgUrl { get; set; } = "";
            public bool IsPrimary { get; set; }
        }
    }
}
