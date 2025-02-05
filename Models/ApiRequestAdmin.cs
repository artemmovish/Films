using System.Text.Json.Serialization;



namespace Admin.Models
{
    public class ApiRequestAdmin
    {
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }
}