namespace Asm03_17_VuDucHuy.Models
{
    public class PostResponse
    {
        public int PostId { get; set; }
        public string AuthorName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool? PublishStatus { get; set; }
        public string CategoryName { get; set; }

    }
}
