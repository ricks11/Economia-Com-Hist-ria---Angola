namespace EconomiaComHistoria.API.Services;

public interface IFileStorageService
{
    /// <summary>
    /// Uploads a file to storage.
    /// </summary>
    /// <param name="file">The file to upload</param>
    /// <param name="folder">Folder path within storage</param>
    /// <returns>The file path/URL</returns>
    Task<string> UploadFileAsync(IFormFile file, string folder = "uploads");

    /// <summary>
    /// Deletes a file from storage.
    /// </summary>
    /// <param name="filePath">The file path to delete</param>
    Task DeleteFileAsync(string filePath);

    /// <summary>
    /// Checks if a file extension is allowed.
    /// </summary>
    bool IsAllowedExtension(string fileName, string[] allowedExtensions);
}
