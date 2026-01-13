using System.Text.Json.Serialization;

public class LoginReqDto
  {
      [JsonPropertyName("email")]
      public string Email { get; set; }
      [JsonPropertyName("password")]
      public string Password { get; set; }
  }    