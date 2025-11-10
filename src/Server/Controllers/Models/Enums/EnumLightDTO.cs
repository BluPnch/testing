namespace Server.Controllers.Models.Enums;

/// <summary>
/// Требования к освещению для проращивания семян
/// </summary>
public enum EnumLight
{
    /// <summary>
    /// Полное отсутствие света
    /// </summary>
    Darkness = 0,
    
    /// <summary>
    /// Очень слабое освещение
    /// </summary>
    VeryLow = 1,
    
    /// <summary>
    /// Слабое освещение
    /// </summary>
    Low = 2,
    
    /// <summary>
    /// Умеренное освещение
    /// </summary>
    Medium = 3,
    
    /// <summary>
    /// Яркое освещение
    /// </summary>
    High = 4,
    
    /// <summary>
    /// Очень яркое освещение
    /// </summary>
    VeryHigh = 5,
    
    /// <summary>
    /// Прямой солнечный свет
    /// </summary>
    DirectSunlight = 6,
    
    /// <summary>
    /// Требуется чередование света и темноты
    /// </summary>
    Alternating = 7,
    
    /// <summary>
    /// Нет особых требований (подходит любой свет)
    /// </summary>
    Any = 8
}