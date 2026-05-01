using Microsoft.VisualBasic;

public static class Times
{
    public const int REMINDER_MINUTE = !Main.DEBUG ? (12 + 8) * 60 : 9 * 60;
    public const int MINUTE_I_SHOULD_BE_SHUT_DOWN = !Main.DEBUG ? (12 + 8) * 60 + 30 : 9 * 60 + 1;

    public static string MinuteTo24Hours(int minute) => $"{minute / 60 % 24}:{minute % 60}";
    
    public static bool IsWarnTime() => DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > REMINDER_MINUTE;
    public static bool IsSleepingTime() => DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > (SleepReminderBetter.ABSOLUTE_MAX_MIN ?? MINUTE_I_SHOULD_BE_SHUT_DOWN - 15);

    public static bool IsAbsoluteSleepTime() => SleepReminderBetter.ABSOLUTE_MAX_MIN is not null && DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > SleepReminderBetter.ABSOLUTE_MAX_MIN;

}