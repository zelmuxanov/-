using System.ComponentModel.DataAnnotations;

namespace CarRental.Web.ViewModels.Admin;

public class FaqCreateEditViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Введите вопрос")]
    [Display(Name = "Вопрос")]
    public string Question { get; set; } = string.Empty;

    [Required(ErrorMessage = "Введите ответ")]
    [Display(Name = "Ответ")]
    public string Answer { get; set; } = string.Empty;

    [Display(Name = "Категория")]
    public string? Category { get; set; }

    [Display(Name = "Порядок отображения")]
    [Range(0, 1000)]
    public int DisplayOrder { get; set; }

    [Display(Name = "Активен")]
    public bool IsActive { get; set; } = true;
}