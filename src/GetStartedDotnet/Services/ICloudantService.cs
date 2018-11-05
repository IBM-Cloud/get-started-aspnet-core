using GetStartedDotnet.Models;
using System.Threading.Tasks;

namespace GetStartedDotnet.Services
{
    public interface ICloudantService
    {
        Task<dynamic> CreateAsync(Visitor item);
        Task<dynamic> GetAllAsync();
    }
}