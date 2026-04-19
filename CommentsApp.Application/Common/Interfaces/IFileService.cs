using CommentsApp.Application.Common.Models;

namespace CommentsApp.Application.Common.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult> SaveFileAsync(byte[] fileData, string fileName, string contentType);
    }
}
