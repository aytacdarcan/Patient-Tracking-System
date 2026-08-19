namespace HastaTakip.api.Dtos
{
    public class LoginResultDto
    {
        public bool Success { get; set; }
        public string? Username { get; set; }
        public string? Role { get; set; }
        public string? Message { get; set; }
    }
}
