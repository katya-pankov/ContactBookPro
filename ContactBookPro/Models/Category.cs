using System.ComponentModel.DataAnnotations;


namespace ContactBookPro.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required]
        public string? AppUserId { get; set; }
        [Required]
        [Display(Name="Category Name")]
        public string? Name { get; set; }

        //Virtuals (we need a FK for AppUser). 
        public virtual AppUser? AppUser { get; set; }

        public virtual ICollection<Contact> Contacts { get; set; } = new HashSet<Contact>();
    }
}
