namespace NessWebApi.Models
{
    public class UploadedFile
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public DateTime UploadDateTime { get; set; }
        public long FileSize { get; set; }
        public string ImageUrl { get; set; }
    }
}
