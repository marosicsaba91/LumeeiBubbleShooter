using BubbleShooterKit;
using System.Collections.Generic;

public static class LevelManager
{
    public static int availableColorsCount = 0;
    public static List<ColorBubbleType> availableColors = new();

    public static bool unlockedNextLevel;

    public static int lastSelectedLevel = 0;
    public static float scrolledHeight = 0;
}
