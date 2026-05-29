namespace WebApplication1.Models.Entities
{
    public class Admin
    {
        public int ID { get; set; }
        public int IDTaiKhoan { get; set; }
        public string HoTen { get; set; } = null!;
        public string? SDT { get; set; }
        public string? Email { get; set; }

        public TaiKhoan TaiKhoan { get; set; } = null!;
        public ICollection<KhieuNai> KhieuNais { get; set; } = new List<KhieuNai>();
    }
}
