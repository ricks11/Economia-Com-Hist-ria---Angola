namespace EconomiaComHistoria.API.Services;

public class FileStorageService : IFileStorageService
{
    private readonly IWebHostEnvironment _environment;
    private const long MaxFileSize = 5 * 1024 * 1024; // 5MB

    public FileStorageService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> UploadFileAsync(IFormFile file, string folder = "uploads")
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("Ficheiro vazio");

        if (file.Length > MaxFileSize)
            throw new ArgumentException("Ficheiro excede o tamanho máximo de 5MB");

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!IsAllowedExtension(file.FileName, allowedExtensions))
            throw new ArgumentException("Extensão de ficheiro não permitida. Extensões aceites: jpg, jpeg, png, webp");

        var uploadsFolder = Path.Combine(_environment.WebRootPath, folder);
        if (!Directory.Exists(uploadsFolder))
            Directory.CreateDirectory(uploadsFolder);

        var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(fileStream);
        }

        return Path.Combine(folder, uniqueFileName).Replace("\\", "/");
    }

    public async Task DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return;

        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));

        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
                await Task.CompletedTask;
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException("Erro ao eliminar o ficheiro", ex);
            }
        }
    }

    public bool IsAllowedExtension(string fileName, string[] allowedExtensions)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLowerInvariant();
        return allowedExtensions.Contains(extension);
    }
}
