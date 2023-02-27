using System.ComponentModel.DataAnnotations;

namespace AddressBook.API.Models
{
    public class Address
    {
        public int Id { get; set; }
        [Required]
        public string Street { get; set; }
        [Required]
        public string HouseNumber { get; set; }
        [Required]
        public string ZipCode { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string Country { get; set; }
    }
}
