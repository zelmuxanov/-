using Microsoft.AspNetCore.Http;

namespace CarRental.Web.ViewModels.Admin;

public class ContractTemplateViewModel
{
    public IFormFile? TemplateFile { get; set; }
    public string? CurrentTemplatePath { get; set; }
}