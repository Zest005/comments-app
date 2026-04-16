using CommentsApp.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommentsApp.Application.Common.Interfaces
{
    public interface IFileService
    {
        Task<FileUploadResult> SaveFileAsync(byte[] fileData, string fileName, string contentType);
    }
}
