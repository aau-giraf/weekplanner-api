namespace GirafAPI.Utils;

public static class FileUtils
{
    public static IFormFile CreateFormFile(string filePath)
    {
        var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        return new FormFile(fileStream, 0, fileStream.Length, null, Path.GetFileName(filePath))
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg" // Set the content type for the image
        };
    }
}