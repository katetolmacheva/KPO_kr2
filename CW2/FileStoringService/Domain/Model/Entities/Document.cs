using System.Security.Cryptography;
using System.Text;
using FileStoringService.Entities;

namespace FileStoringService.Domain.Model.Entities;
public class Document
{
    public Guid Id { get; private set; }
    
    public string Name { get; private set; }
    
    public string Location { get; private set; }
    
    public string Hash { get; private set; }
    
    public DateTime UploadDate { get; private set; }
    
    private Document() { }

    public static Document Create(string name, string path, string content)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Имя документа не может быть пустым", nameof(name));
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Путь к документу не может быть пустым", nameof(path));

        var hash = Document.CreateFromContent(content);

        return new Document
        {
            Id = Guid.NewGuid(),
            Name = name,
            Location = path,
            Hash = hash,
            UploadDate = DateTime.UtcNow,
        };
    }
    
    public void UpdateHash(string newHash)
    {
        Hash = newHash;
        UploadDate = DateTime.UtcNow;
    }
    
    public static string CreateFromContent(string content)
    {
        if (string.IsNullOrEmpty(content))
            throw new ArgumentException("Содержимое не может быть пустым", nameof(content));
            
        using (var md5 = MD5.Create())
        {
            byte[] contentBytes = Encoding.UTF8.GetBytes(content);
            byte[] hashBytes = md5.ComputeHash(contentBytes);
            
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hashBytes.Length; i++)
            {
                sb.Append(hashBytes[i].ToString("x2"));
            }
            
            return sb.ToString();
        }
    }
}

