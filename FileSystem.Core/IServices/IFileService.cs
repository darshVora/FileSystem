using FileSystem.Core.Models;
using Microsoft.AspNetCore.Http;

namespace FileSystem.Core.IServices
{
    public interface IFileService
    {
        Task ProcessFileAsync(IFormFile file);
        IQueryable<FileData> GetFileData(int pageno, int pagesize, string content);
    }
}
