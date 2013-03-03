using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCodeFirstProviderWrapperDemo
{
    public class Post
    {
        [Key]
        public Guid PostIdentifier { get; set; }

        [Required]
        public string PostTitle { get; set; }

        [Required]
        public string PostMessage { get; set; }

        [Required]
        public Account PostAuthor { get; set; }
    }
}
