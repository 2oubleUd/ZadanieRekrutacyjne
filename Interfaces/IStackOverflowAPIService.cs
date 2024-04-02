using ZadanieRekrutacyjne.Models;

namespace ZadanieRekrutacyjne.Interfaces
{
    public interface IStackOverflowAPIService
    {
        Task FetchAndSaveTagsAsync();
        Task<IEnumerable<StackOverflowTag>> GetPaginatedResultAsync(int pageSize, int pageNum, char ordering, string orderBy);

    }
}
