using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFCodeFirstProviderWrapperDemo
{
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [MaxLength(150)]
        public string FullName { get; set; }

        [MaxLength(150)]
        public string CompanyName { get; set; }

        public DateTime BirthDay { get; set; }

        public virtual ICollection<Post> Posts { get; set; }
    }
}
