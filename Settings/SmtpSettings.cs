// Settings/SmtpSettings.cs
namespace EcommerceMVC.Settings
{
    public class SmtpSettings
    {
        public string Host       { get; set; }
        public int    Port       { get; set; }
        public string UserName   { get; set; }
        public string Password   { get; set; }
        public bool   EnableSsl  { get; set; } = true;
        public string FromEmail  { get; set; }
    }
}
