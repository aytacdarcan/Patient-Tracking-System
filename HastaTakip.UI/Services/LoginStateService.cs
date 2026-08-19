namespace HastaTakip.UI.Services;

public class LoginStateService
{
    public string? Username { get; set; }
    public string? Role { get; set; }

    public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Role);

    public bool IsDoctor => Role == "Doktor";
    public bool IsSecretary => Role == "Sekreter";

    public event Action? OnChange;

    public void Login(string username, string role)
    {
        Username = username;
        Role = role;
        OnChange?.Invoke();
    }

    public void Logout()
    {
        Username = null;
        Role = null;
        OnChange?.Invoke();
    }
}