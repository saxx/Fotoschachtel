using System.ComponentModel.DataAnnotations;

namespace Fotoschachtel.ViewModels.Event
{
    public class CreateViewModel
    {
        [Required(ErrorMessage = "Bitte gib einen Namen für dein Event an.")]
        [RegularExpression("^[a-zA-Z0-9_]{5,}$", ErrorMessage = "Der Name eines Events darf keine Sonderzeichen enthalten, und muss mindestens 5 Zeichen lang sein.")]
        public string Event { get; set; }
        [Required(ErrorMessage = "Bitte gib ein Passwort für dein Event an.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Bitte gib deine E-Mail-Adresse an.")]
        [EmailAddress(ErrorMessage = "Bitte gib deine E-Mail-Adresse an.")]
        public string Email { get; set; }
    }
}
