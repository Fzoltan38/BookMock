using System.ComponentModel.DataAnnotations;

namespace BookStoreApi.DTOs
{
    public class BookUpdateDto
    {
        [Required(ErrorMessage = "A cím megadása kötelező.")]
        [MaxLength(200)]
        public string Title { get; set; }
        [Required(ErrorMessage = "A szerző megadása kötelező.")]
        [MaxLength(150)]
        public string Author { get; set; }
        [Required(ErrorMessage = "Az ISBN megadása kötelező.")]
        [MaxLength(20)]
        public string ISBN { get; set; }
        [Range(1, 100000, ErrorMessage = "A megadott intervallumból válasz.")]
        public int Price { get; set; }
        [Range(1000, 2030, ErrorMessage = "A megadott intervallumból válasz dátumot.")]
        public int PublishedYear { get; set; }
    }
}
