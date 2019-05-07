using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace Tangy.Models
{
    public class MenuItem
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Name  { get; set; }

       
        public string Description { get; set; }
        public string Spicyness { get; set; }
        public enum ESpicy { NA=0,Mild=1,Medium=2,Spicy=3,VerySpicy=4}
        public string Image { get; set; }


        [Range(1,Int32.MaxValue,ErrorMessage ="Price should be greater than ${1}" )]
        public double Price { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        [Display(Name = "SubCategory")]
        public int SubCategoryId { get; set; }

        [ForeignKey("SubCategoryId")]
        public SubCategory SubCategory { get; set; }
    }


}
