namespace Server.Controllers.Models.Enums;

public enum EnumViability
{
    /// <summary>
    /// Спящие семена
    /// </summary>
    Dormant = 0,
    
    /// <summary>
    /// Поврежденные семена (механические/химические повреждения)
    /// </summary>
    Damaged = 1,
    
    /// <summary>
    /// Пересушенные семена
    /// </summary>
    Overdried = 2,
    
    /// <summary>
    /// Плесневые/зараженные семена
    /// </summary>
    Contaminated = 3,
    
    /// <summary>
    /// Свежесобранные (требуют дозревания)
    /// </summary>
    FreshlyHarvested = 4,
    
    /// <summary>
    /// Старые семена
    /// </summary>
    Expired = 5,
    
    /// <summary>
    /// Жизнеспособность не определена
    /// </summary>
    Unknown = 6
}