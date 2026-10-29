using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace JLM.Helpers;

public static class FormatExtensions
{
    private static readonly CultureInfo Br = CultureInfo.GetCultureInfo("pt-BR");

    public static string ToBrl(this decimal value) => value.ToString("C", Br);

    public static string ToBrl(this decimal? value) => value.HasValue ? value.Value.ToBrl() : "—";

    public static string ToShortDate(this DateTime value) => value.ToString("dd/MM/yyyy", Br);

    public static string ToLongDate(this DateTime value) => value.ToString("dd 'de' MMMM 'de' yyyy", Br);

    public static string ToMonthYear(this DateTime value) => value.ToString("MMM/yy", Br).Replace(".", string.Empty).ToUpperInvariant();

    public static string Installments(this decimal total, int max = 6)
    {
        var count = total >= 300 ? max : total >= 150 ? 3 : 2;
        return $"{count}x de {(total / count).ToBrl()} sem juros";
    }

    public static string Initials(this string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "JL";
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 1
            ? parts[0][..Math.Min(2, parts[0].Length)].ToUpperInvariant()
            : $"{parts[0][0]}{parts[^1][0]}".ToUpperInvariant();
    }

    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }
        var text = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        text = Regex.Replace(text, @"[^a-z0-9\s-]", string.Empty);
        text = Regex.Replace(text, @"\s+", "-");
        return Regex.Replace(text, @"-+", "-").Trim('-');
    }

    public static string OnlyDigits(this string? value) =>
        string.IsNullOrWhiteSpace(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());

    public static string MaskCpf(this string? value)
    {
        var digits = value.OnlyDigits();
        return digits.Length == 11 ? $"***.***.{digits.Substring(6, 3)}-{digits.Substring(9, 2)}" : "—";
    }
}

public static class OrderDisplay
{
    public static string CssClass(Models.OrderStatus status) => status switch
    {
        Models.OrderStatus.Pending => "status status--warning",
        Models.OrderStatus.Paid => "status status--info",
        Models.OrderStatus.Processing => "status status--gold",
        Models.OrderStatus.Shipped => "status status--info",
        Models.OrderStatus.Delivered => "status status--success",
        Models.OrderStatus.Cancelled => "status status--danger",
        _ => "status"
    };

    public static string StatusLabel(Models.OrderStatus status) => status switch
    {
        Models.OrderStatus.Pending => "Aguardando pagamento",
        Models.OrderStatus.Paid => "Pagamento aprovado",
        Models.OrderStatus.Processing => "Em preparação",
        Models.OrderStatus.Shipped => "Enviado",
        Models.OrderStatus.Delivered => "Entregue",
        Models.OrderStatus.Cancelled => "Cancelado",
        _ => status.ToString()
    };

    public static string PaymentLabel(Models.PaymentMethod method) => method switch
    {
        Models.PaymentMethod.Pix => "Pix",
        Models.PaymentMethod.CreditCard => "Cartão de crédito",
        Models.PaymentMethod.Boleto => "Boleto bancário",
        _ => method.ToString()
    };

    public static string ShippingLabel(Models.ShippingMethod method) => method switch
    {
        Models.ShippingMethod.Standard => "Entrega padrão",
        Models.ShippingMethod.Express => "Entrega expressa",
        _ => method.ToString()
    };
}
