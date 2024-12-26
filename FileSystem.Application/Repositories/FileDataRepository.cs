using FileSystem.Core.IRepositories;
using FileSystem.Core.Models;

namespace FileSystem.Application.Repositories
{
    public class FileDataRepository : IFileDataRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public FileDataRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(FileData fileData)
        {
            await _dbContext.FileData.AddAsync(fileData);
        }

        public IQueryable<FileData> GetFileData()
        {
            return _dbContext.FileData.AsQueryable();
        }

        public async Task<FileData> GetFileDataByIdAsync(int id) => await _dbContext.FileData.FindAsync(id);

        public async Task SaveAsync()
        {
            await _dbContext.SaveChangesAsync();
        }
    }
}
