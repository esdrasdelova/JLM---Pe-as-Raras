namespace JLM.Services;

public interface IFileUploadService
{
    Task<string?> SaveProductImageAsync(IFormFile? file);
}

public class FileUploadService : IFileUploadService
{
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private const long MaxSize = 5 * 1024 * 1024;
    private readonly IWebHostEnvironment _env;

    public FileUploadService(IWebHostEnvironment env) => _env = env;

    public async Task<string?> SaveProductImageAsync(IFormFile? file)
    {
        if (file is null || file.Length == 0) return null;
        if (file.Length > MaxSize) throw new InvalidOperationException("A imagem deve ter no máximo 5 MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new InvalidOperationException("Formato inválido. Envie JPG, PNG ou WEBP.");

        var folder = Path.Combine(_env.WebRootPath, "images", "uploads", "products");
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, fileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/images/uploads/products/{fileName}";
    }
}
