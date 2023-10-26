using System;
using System.Collections.Generic;

namespace Asm03_17_VuDucHuy.Models
{
    public partial class Post
    {
        public int PostId { get; set; }
        public int? AuthorId { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public bool? PublishStatus { get; set; }
        public int? CategoryId { get; set; }

        public virtual AppUser? Author { get; set; }
        public virtual PostCategory? Category { get; set; }
    }
}
