using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Globalization;

namespace CarRental.Web.TagHelpers;

[HtmlTargetElement("price", TagStructure = TagStructure.NormalOrSelfClosing)]
public class PriceTagHelper : TagHelper
{
    [HtmlAttributeName("value")]
    public decimal Value { get; set; }
    
    [HtmlAttributeName("currency")]
    public string Currency { get; set; } = "₽";
    
    [HtmlAttributeName("show-decimal")]
    public bool ShowDecimal { get; set; } = false;
    
    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.TagMode = TagMode.StartTagAndEndTag;
        
        var format = ShowDecimal ? "N2" : "N0";
        var formattedValue = Value.ToString(format, CultureInfo.CurrentCulture);
        
        // Заменяем неразрывный пробел на обычный для лучшей читаемости
        formattedValue = formattedValue.Replace("\u00A0", " ");
        
        output.Content.SetHtmlContent($"{formattedValue} {Currency}");
        
        // Добавляем CSS класс для стилизации
        output.Attributes.SetAttribute("class", "price-value");
    }
}