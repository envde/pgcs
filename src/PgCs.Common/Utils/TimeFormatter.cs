using System;

namespace PgCs.Common.Utils;

/// <summary>
/// Утилиты для форматирования TimeSpan
/// </summary>
public static class TimeFormatter
{
    /// <summary>
    /// Форматирует TimeSpan в читаемую строку
    /// </summary>
    /// <param name="elapsed">Временной интервал</param>
    /// <returns>Отформатированная строка (например: "150ms", "2.5s", "1m 30s")</returns>
    public static string FormatElapsedTime(TimeSpan elapsed)
    {
        if (elapsed.TotalSeconds < 1)
            return $"{elapsed.TotalMilliseconds:F0}ms";
        
        if (elapsed.TotalMinutes < 1)
            return $"{elapsed.TotalSeconds:F2}s";
        
        // Для времени > 1 минуты показываем минуты и секунды
        return $"{elapsed.Minutes}m {elapsed.Seconds}s";
    }
}
