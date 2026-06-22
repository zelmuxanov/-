using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Globalization;

namespace CarRental.Web.HtmlHelpers;

public static class PriceHtmlHelper
{
    public static IHtmlContent FormatPrice(this IHtmlHelper htmlHelper, decimal price, bool showDecimal = false)
    {
        var format = showDecimal ? "N2" : "N0";
        var formattedPrice = price.ToString(format, CultureInfo.CurrentCulture);
        formattedPrice = formattedPrice.Replace("\u00A0", " ");
        
        return new HtmlString($"{formattedPrice} ₽");
    }
    
    public static IHtmlContent FormatPriceWithCurrency(this IHtmlHelper htmlHelper, decimal price, string currency = "₽", bool showDecimal = false)
    {
        var format = showDecimal ? "N2" : "N0";
        var formattedPrice = price.ToString(format, CultureInfo.CurrentCulture);
        formattedPrice = formattedPrice.Replace("\u00A0", " ");
        
        return new HtmlString($"{formattedPrice} {currency}");
    }
}