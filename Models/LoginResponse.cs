using ApiAppPay.Models;
using System.Text.Json.Serialization;

namespace ApiAppPay.Models.Responses
{
    public class LoginResponse
    {
        [JsonPropertyName("Token")]
      
        public string AccessToken { get; set; }
        
        public LoginResponse() { }  
        public LoginResponse(string accessToken)
        {
            AccessToken = accessToken;
          
        }
    }
}
