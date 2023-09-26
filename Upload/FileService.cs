using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace Upload
{
    public class FileService
    {
        private readonly string rootPath;
        private readonly string root = "/wwwroot";
        public FileService()
        {
            rootPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + root;
        }
        public FileStreamResult GetFile(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string fullPath = Path.Combine(rootPath, filePath);
                bool fileExists = System.IO.File.Exists(fullPath);
                if (fileExists)
                {
                    // Xác định kiểu MIME dựa trên phần mở rộng của tệp
                    var contentType = GetContentType(filePath);
                    if (contentType == null)
                    {
                        // Kiểu MIME không xác định, mặc định thành "application/octet-stream"
                        contentType = "application/octet-stream";
                    }

                    // Mở file thành một FileStream
                    var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read);

                    // Trả về FileStreamResult để truyền dữ liệu file và thông tin kiểu MIME
                    return new FileStreamResult(fileStream, contentType)
                    {
                        FileDownloadName = Path.GetFileName(filePath)
                    };
                }
            }
            return null;
        }

        public string getFilePath(string filePath)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string fullPath = Path.Combine(rootPath + filePath);
                bool fileExists = System.IO.File.Exists(fullPath);
                if (fileExists)
                {
                    return fullPath;
                }
            }
            return null;
        }

        public string GetContentType(string fileName)
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(fileName, out var contentType))
            {
                contentType = null;
            }
            return contentType;
        }
        public string SaveFile(string folder, IFormFile file, string? fileName = null)
        {
            if (file != null)
            {
                string path = folder + "/" + (fileName != null? fileName : file.FileName);
                if (file.Length > 0)
                {
                    if (!Directory.Exists(rootPath + folder))
                    {
                        Directory.CreateDirectory(rootPath + folder);
                    }
                    bool fileExists = System.IO.File.Exists(rootPath + path);
                    if (fileExists)
                    {
                        return path;
                    }
                    using (FileStream filestream = System.IO.File.Create(rootPath + path))
                    {
                        file.CopyTo(filestream);
                        filestream.Flush();
                        return path;
                    }
                }
            }
            return null;
        }
        public void ResizeImage(IFormFile sourceImageFile, string folder, int newWidth, int newHeight, string? fileName = null)
        {
            fileName = (fileName != null ? fileName : sourceImageFile.FileName);
            string filePath = rootPath + folder + "/" + fileName;
            if (sourceImageFile.Length > 0)
            {
                if (!Directory.Exists(rootPath + folder))
                {
                    Directory.CreateDirectory(rootPath + folder);
                }
                bool fileExists = System.IO.File.Exists(rootPath + folder + "/" + fileName);
                if (fileExists)
                {
                    return;
                }
            }
            using (var imageStream = sourceImageFile.OpenReadStream())
            using (var image = Image.Load(imageStream))
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));
                image.Save(filePath);
            }
        }
        public IFormFile ResizeImage(IFormFile sourceImageFile, int newWidth, int newHeight)
        {
            using (var image = Image.Load(sourceImageFile.OpenReadStream()))
            {
                image.Mutate(x => x.Resize(newWidth, newHeight));

                // Tạo một MemoryStream mới và lưu ảnh đã giảm độ phân giải vào đó
                using (var resizedImageStream = new MemoryStream())
                {
                    switch (sourceImageFile.ContentType)
                    {
                        case "image/jpeg":
                            image.Save(resizedImageStream, new JpegEncoder());
                            break;
                        case "image/png":
                            image.Save(resizedImageStream, new PngEncoder());
                            break;
                        default:
                            break;
                    }
                    image.Save(resizedImageStream, new JpegEncoder());

                    // Tạo một đối tượng FormFile mới từ MemoryStream và các thông tin từ IFormFile gốc
                    var resizedImageBytes = resizedImageStream.ToArray();
                    var resizedImageContentType = sourceImageFile.ContentType;

                    var resizedImage = new FormFile(resizedImageStream, 0, resizedImageBytes.Length,
                        sourceImageFile.Name, sourceImageFile.FileName)
                    {
                        Headers = sourceImageFile.Headers,
                        ContentType = resizedImageContentType
                    };

                    return resizedImage;
                }
            }
        }
        public string GetFileName(string fileName)
        {
            try
            {
                return fileName.Split("/")[fileName.Split("/").Length - 1];
            }
            catch (Exception ex)
            {
                return fileName;
            }
        }
        public string getRootPath()
        {
            return rootPath;
        }
        public string getExtension(IFormFile file)
        {
            string extension = Path.GetExtension(file.FileName);
            if (extension == "")
            {
                try
                {
                    extension = "." + file.ContentType.Split('/')[1];
                }
                catch (Exception ex)
                {
                    extension = ".png";
                }
            }
            return extension;
        }
    }
}
