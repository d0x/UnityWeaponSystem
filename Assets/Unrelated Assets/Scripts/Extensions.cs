using System;

/**
 * Sometimes I like to have Logmessages like this:
 * [WorldManager     ]: Hello World
 *
 * To create them this extension can be used like this:
 * Debug.Log($"{GetType().logName()}: Test");
 */
public static class TypeExtensions {
    private static int length = 17;

    public static String logName(this Type type) {
        var formattedName = type.Name.PadRight(length);
        return $"[{formattedName}]";
    }
}