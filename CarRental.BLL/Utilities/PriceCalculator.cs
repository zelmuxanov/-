namespace CarRental.BLL.Utilities;

public class PriceCalculator
{
    public decimal CalculatePrice(decimal pricePerDay, DateTime startDate, DateTime endDate)
    {
        var days = (endDate - startDate).Days;
        return days > 0 ? pricePerDay * days : pricePerDay;
    }
}