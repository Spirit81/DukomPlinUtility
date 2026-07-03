namespace DukomPlinUtility.Helpers;

public static class CroatianTextHelper
{
    public static string Normalize(string? input)
    {
        if (input == null) return string.Empty;
        return input.Replace('č','c').Replace('ć','c').Replace('š','s').Replace('ž','z').Replace('đ','d')
                    .Replace('Č','C').Replace('Ć','C').Replace('Š','S').Replace('Ž','Z').Replace('Đ','D');
    }
}
