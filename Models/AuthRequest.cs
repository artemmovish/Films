
using System.Text.Json.Serialization;

namespace Admin.Models
{
public class AuthRequest
{
        [JsonPropertyName("email")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
}
}