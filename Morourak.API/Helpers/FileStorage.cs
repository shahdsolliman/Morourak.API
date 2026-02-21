using Microsoft.AspNetCore.Http;

public static class FileStorage
{
    public static async Task<string> Save(IFormFile file, string folder)
    {
        var root = Path.Combine("wwwroot", "uploads", folder);
        Directory.CreateDirectory(root);

        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var fullPath = Path.Combine(root, fileName);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return fullPath;
    }
}
