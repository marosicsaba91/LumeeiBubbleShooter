using System;

[Serializable]
public class User
{ 
    public int coins;
    public int lives;
    public int unlockedNextLevel;
    public int lastSelectedLevel;

    // "next_level"
    // "unlocked_next_level"
    // "num_lives"
    // "num_coins"
    // "last_selected_level"
    // "level_stars_{i}"
    // "level_stars_{i}"

    public void MergeWith(User other) 
    {
        other.coins = Math.Max(coins, other.coins);
        other.lives = Math.Max(lives, other.lives);
        other.unlockedNextLevel = Math.Max(unlockedNextLevel, other.unlockedNextLevel);
        other.lastSelectedLevel = Math.Max(lastSelectedLevel, other.lastSelectedLevel);
    }
}