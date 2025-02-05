
using System.Text.Json.Serialization;



namespace Admin.Models
{
public class AuthResponse
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;
}
}