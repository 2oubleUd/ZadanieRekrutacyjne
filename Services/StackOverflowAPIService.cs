using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.StacMan;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Runtime.InteropServices.JavaScript;
using System.Text.Json;
using System.Text.Json.Nodes;
using ZadanieRekrutacyjne.Interfaces;
using ZadanieRekrutacyjne.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace ZadanieRekrutacyjne.Services
{
    public class StackOverflowAPIService : IStackOverflowAPIService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StackOverflowAPIService> _logger;
        public readonly AppDbContext _appDbContext;
        private readonly HttpClient _httpClient;
        
        string apiKey = "bXI2ukmq)UiZq3fQC6bx7A((";

        public StackOverflowAPIService(IConfiguration configuration, ILogger<StackOverflowAPIService> logger, 
            AppDbContext appDbContext) 
        {
            _configuration = configuration;
            _logger = logger;
            _appDbContext = appDbContext;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", "Bearer bXI2ukmq)UiZq3fQC6bx7A((");
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            _httpClient.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("deflate"));

        }


        public async Task FetchAndSaveTagsAsync()
        {
            for(int i = 1; i <= 11; i++)
            {
                var deserializedJson = await GetTagsFromStackOverflowAPI(i);

                // null reference exception
                if (deserializedJson != null)
                {
                    _logger.LogInformation("Tags have been successfully obtained from StackExchange Api.");
                    await SaveTagsToDatabase(deserializedJson);
                }
            }
        }

        private async Task<Root> GetTagsFromStackOverflowAPI(int pageNumber)
        {
            
            _logger.LogInformation("Started getting data from StackExchange Api...");

            // to do: go for over 10 different pages to save around 1000 tags
            var response = await _httpClient.GetAsync($"https://api.stackexchange.com/2.3/tags?page={pageNumber}&pagesize=100&order=desc&sort=popular&site=stackoverflow");

            if (response.IsSuccessStatusCode)
            {
                return await ConvertFromGZipToJsonAsync(response);
            }
            else
            {
                Console.WriteLine($"Failed to retrieve data. Status code: {response.StatusCode}");
                _logger.LogError($"Failed to retrieve data. Status code: {response.StatusCode}");
                return null;
            }
        }

        private async Task<Root> ConvertFromGZipToJsonAsync(HttpResponseMessage response)
        {
            using (var stream = await response.Content.ReadAsStreamAsync())
            using (var decompressedStream = new GZipStream(stream, CompressionMode.Decompress))
            using (var reader = new System.IO.StreamReader(decompressedStream))
            {
                var responseContent = await reader.ReadToEndAsync();
                Console.WriteLine(responseContent);

                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(responseContent);
                _logger.LogInformation("Successfully compresed obtained data from GZIP format to JSON");
                
                return myDeserializedClass;
            }
        }

        private async Task SaveTagsToDatabase(Root deserializedJson)
        {
            var tags = deserializedJson.items;

            if(tags != null)
            {
                _logger.LogInformation("Obtained data is not null. Starting saving data to local database...");

                foreach (var tag in tags)
                {
                    var existingTag = await _appDbContext.Tags.FirstOrDefaultAsync(x => x.Name == tag.name);

                    if (existingTag != null)
                    {
                        existingTag.Count = +tag.count;
                    }
                    else
                    {
                        _appDbContext.Tags.Add(new StackOverflowTag() { Count = tag.count, Name = tag.name });
                    }

                    await _appDbContext.SaveChangesAsync();
                    _logger.LogInformation("Saved obtained data to local database");
                }
            }
            
        }


        public async Task<IEnumerable<StackOverflowTag>> GetPaginatedResultAsync(int pageSize, int pageNum, char ordering, string orderBy)
        {
            IQueryable<StackOverflowTag> queryResponse = _appDbContext.Tags.AsQueryable();

            switch (orderBy.ToLower())
            {
                case "name":
                    queryResponse = ordering == 'd' ? queryResponse.OrderByDescending(x => x.Name) : queryResponse.OrderBy(x => x.Name);
                    break;
                case "counter":
                    queryResponse = ordering == 'd' ? queryResponse.OrderByDescending(x => x.Count) : queryResponse.OrderBy(x => x.Count);
                    break;
                default:
                    queryResponse = queryResponse.OrderBy(x => x.Name); // Default order by name
                    break;
            }

            List<StackOverflowTag> tagsResponse = new List<StackOverflowTag>();

            try
            {
                tagsResponse = await queryResponse
                .Skip((pageNum - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            }

            catch (Exception ex)
            {
                throw new ArgumentOutOfRangeException("You try to get to index which are out of range. Try to input smaller page size or page number");
            }
            

            return tagsResponse;
        }
       
    }
}
