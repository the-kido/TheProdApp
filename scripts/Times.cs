using Microsoft.VisualBasic;

public static class Times
{
    const int REMINDER_MINUTE = (12 + 8) * 60 + 30;
    const int MINUTE_I_SHOULD_BE_SHUT_DOWN = (12 + 9) * 60;

    public static bool IsWarnTime() => DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > REMINDER_MINUTE;
    public static bool IsSleepingTime() => DateAndTime.Now.Hour * 60 + DateAndTime.Now.Minute > MINUTE_I_SHOULD_BE_SHUT_DOWN;
}