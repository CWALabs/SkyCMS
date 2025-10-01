using System;
using System.Security.Cryptography;
using System.Text;
using Sky.GitAPI.Models;

namespace Sky.GitAPI.Services
{
    /// <summary>
    /// Service for Git operations
    /// </summary>
    public class GitService : IGitService
    {
        public string GenerateSha(string content)
        {
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(content);
            var hash = sha1.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string GenerateGitObjectSha(string type, string content)
        {
            // Git SHA-1 is calculated as: SHA1("blob " + content.length + "\0" + content)
            var header = $"{type} {Encoding.UTF8.GetByteCount(content)}\0";
            var fullContent = header + content;
            
            using var sha1 = SHA1.Create();
            var bytes = Encoding.UTF8.GetBytes(fullContent);
            var hash = sha1.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        public string CreateUrl(string baseUrl, string path)
        {
            return $"{baseUrl.TrimEnd('/')}/{path.TrimStart('/')}";
        }
    }
}