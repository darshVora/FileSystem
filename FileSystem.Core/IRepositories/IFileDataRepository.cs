using FileSystem.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.Core.IRepositories
{
    public interface IFileDataRepository
    {
        Task AddAsync(FileData fileData);
        Task SaveAsync();
        IQueryable<FileData> GetFileData();
        Task<FileData> GetFileDataByIdAsync(int id);
    }
}
