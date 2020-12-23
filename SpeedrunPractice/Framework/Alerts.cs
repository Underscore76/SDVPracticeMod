using StardewModdingAPI;
using StardewValley;

class Alerts
{
    public static void Success(string message)
    {
        HUDMessage hudMessage = new HUDMessage(message, HUDMessage.achievement_type);
        hudMessage.timeLeft = 1500f;
        Game1.addHUDMessage(hudMessage);
    }

    public static void Failure(string message)
    {
        HUDMessage hudMessage = new HUDMessage(message, HUDMessage.error_type);
        hudMessage.timeLeft = 1500f;
        Game1.addHUDMessage(hudMessage);
    }

    public static void Info(string message)
    {
        HUDMessage hudMessage = new HUDMessage(message, HUDMessage.newQuest_type);
        hudMessage.timeLeft = 1500f;
        Game1.addHUDMessage(hudMessage);
    }
}