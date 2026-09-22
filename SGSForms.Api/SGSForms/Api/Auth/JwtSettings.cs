namespace SGSForms.Api.Auth;

public class JwtSettings
{
	public string Key { get; set; } = "";

	public string Issuer { get; set; } = "SGSForms";

	public string Audience { get; set; } = "SGSForms";

	public int ExpireMinutes { get; set; } = 480;
}
