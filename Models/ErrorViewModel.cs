namespace Topluluk_Yonetim.MVC.Models
{
    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public int StatusCode { get; set; } = 500;
        public string? Message { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string Title => StatusCode switch
        {
            400 => "Geçersiz İstek",
            401 => "Yetkisiz Erişim",
            404 => "Sayfa Bulunamadı",
            422 => "Geçersiz Veri",
            _ => "Bir şeyler ters gitti"
        };
    }
}
