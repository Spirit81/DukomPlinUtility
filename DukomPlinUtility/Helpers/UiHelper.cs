using System;
using System.Linq;

namespace DukomPlinUtility.Helpers;

public static class UiHelper
{
    public static string FirstNonEmpty(params string[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;
}
