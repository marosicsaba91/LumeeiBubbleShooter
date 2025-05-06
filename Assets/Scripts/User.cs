using BubbleShooterKit;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[Serializable]
public class LevelData 
{
    public int stars;
    public int score;
}

[Serializable]
public class User
{
    public int coins;
    public int lives;
    public List<LevelData> levelData;

    [SerializeField] string nextLiveTime;
    [SerializeField] List<int> boosterAmounts;

    public bool musicEnabled;
    public bool soundEnabled;

    public DateTime NextLifeTime
    {
        get
        {
            if (DateTime.TryParse(nextLiveTime, out DateTime result))
                return result;
            else
            {
                nextLiveTime = DateTime.Now.ToString();
                return DateTime.Now;
            }
        }

        set => nextLiveTime = value.ToString();
    }

    public int GetBoosterAmount(PurchasableBoosterBubbleType bubbleType) =>
        (int)bubbleType >= boosterAmounts.Count ? 0 : boosterAmounts[(int)bubbleType];

    public void SetBoosterAmount(PurchasableBoosterBubbleType bubbleType, int value)
    {
        if ((int)bubbleType >= boosterAmounts.Count)
            for (int i = boosterAmounts.Count; i <= (int)bubbleType; i++)
                boosterAmounts.Add(0);

        boosterAmounts[(int)bubbleType] = value;
    }

    public int GetLevelScore(int levelIndex) =>
        levelData == null || levelData.Count <= levelIndex ? 0 : levelData[levelIndex].score;

    public int GetLevelStars(int levelIndex) =>
        levelData == null || levelData.Count <= levelIndex ? 0 : levelData[levelIndex].stars;

    public void SetLevelScore(int levelIndex, int score)
    {
        if (levelData == null || levelData.Count == 0)
            levelData = new List<LevelData>(levelIndex + 1);

        while (levelData.Count <= levelIndex)
            levelData.Add(new LevelData());

        levelData[levelIndex].score = score;
    }

    public void SetLevelStars(int levelIndex, int stars)
    {
        if (levelData == null || levelData.Count == 0)
            levelData = new List<LevelData>(levelIndex + 1);

        while (levelData.Count <= levelIndex)
            levelData.Add(new LevelData());
        levelData[levelIndex].stars = stars;
    }

    public int GetNextLevelIndex()
    {
        if (levelData == null || levelData.Count == 0)
            return 0;
        for (int i = 0; i < levelData.Count; i++)
        {
            if (levelData[i].stars == 0)
                return i;
        }
        return levelData.Count;
    }

    public User(GameConfiguration gameConfig)
    {
        musicEnabled = true;
        soundEnabled = true;
        coins = gameConfig.InitialCoins;
        lives = gameConfig.MaxLives;

        NextLifeTime = DateTime.Now;
        boosterAmounts = new List<int>();
        levelData = new List<LevelData>();
    }

    public void MergeWith(User other)
    {
        Debug.Log("Merging");
        coins = Math.Max(coins, other.coins);
        lives = Math.Max(lives, other.lives);
        musicEnabled = musicEnabled && other.musicEnabled;
        soundEnabled = soundEnabled && other.soundEnabled;
        NextLifeTime = NextLifeTime < other.NextLifeTime ? NextLifeTime : other.NextLifeTime;

        boosterAmounts = MergeList(boosterAmounts, other.boosterAmounts, Math.Max);
    }

    static List<T> MergeList<T>(List<T> a, List<T> b, Func<T, T, T> merging)
    {
        int length = Math.Max(a.Count, b.Count);
        List<T> result = new();
        for (int i = 0; i < length; i++)
        {
            bool aExists = i < a.Count;
            bool bExists = i < b.Count;

            if (!aExists && !bExists)
                result.Add( default);
            else if (aExists && !bExists)
                result.Add(a[i]);
            else if (!aExists && bExists)
                result.Add(b[i]);
            else
                result.Add(merging(a[i], b[i]));
        }
        return result;
    }

    public override string ToString()
    {
        return
            "Coins: " + coins + "\n" +
            "Lives: " + lives + "\n" +
            "Next life: " + NextLifeTime.ToString() + "\n" +
            "Music: " + musicEnabled + "\n" +
            "Sound: " + soundEnabled + "\n" +
            "Booster amounts: " + string.Join(", ", boosterAmounts) + "\n";
    }
}