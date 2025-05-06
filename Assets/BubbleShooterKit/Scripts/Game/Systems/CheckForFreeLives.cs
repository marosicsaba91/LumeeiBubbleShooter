using System;
using UnityEngine;

namespace BubbleShooterKit
{
    public class CheckForFreeLives : MonoBehaviour
    {
        public GameConfiguration GameConfig;
        public CoinsSystem CoinsSystem;

        public event Action<TimeSpan, int> onCountdownUpdated;
        public event Action<int> onCountdownFinished;

        public TimeSpan TimeLeft => DateTime.Now - UserManager.CurrentUser.NextLifeTime;
        TimeSpan lastTimeLeft;

        void Awake()
        {
            lastTimeLeft = TimeLeft;
        }

        void Update()
        {
            TimeSpan timeLeft = TimeLeft;
            int lastLives = UserManager.CurrentUser.lives;
            int currentLives = lastLives;
            int maxLives = GameConfig.MaxLives;

            if (currentLives >= maxLives)
                return;

            int timeToNextLife = GameConfig.TimeToNextLife;
            DateTime nextLifeTime = UserManager.CurrentUser.NextLifeTime;
            DateTime now = DateTime.Now;
            TimeSpan remainingTime =  nextLifeTime - now;

            while (remainingTime.TotalSeconds < 0 && currentLives < maxLives)
            {
                currentLives++;
                remainingTime += TimeSpan.FromSeconds(timeToNextLife);
                UserManager.CurrentUser.NextLifeTime = nextLifeTime.Add(TimeSpan.FromSeconds(timeToNextLife));
            }

            if (currentLives == maxLives)
                UserManager.CurrentUser.NextLifeTime = DateTime.Now;

            if (remainingTime.TotalSeconds != lastTimeLeft.TotalSeconds)
                onCountdownUpdated?.Invoke(remainingTime, currentLives);

            if (currentLives != lastLives)
            {
                UserManager.CurrentUser.lives = currentLives;
                UserManager.TrySaveUserData();
                onCountdownFinished?.Invoke(currentLives);
            }
        }

        void OnApplicationQuit()
        {
            GameObject gameScreen = GameObject.Find("GameScreen");
            if (gameScreen != null)
                RemoveOneLife();
        }

        public void RemoveOneLife()
        {
            int lastLives = UserManager.CurrentUser.lives;
            if (lastLives <= 0)
                return;

            int currentLives = lastLives - 1;
            UserManager.CurrentUser.lives = currentLives;

            int maxLives = GameConfig.MaxLives;
            if (lastLives == maxLives && currentLives < maxLives)
                JumpNextLifeTimeToNext();

            UserManager.TrySaveUserData();
        }

        public void RefillLives_ForCoins()
        {
            UserManager.CurrentUser.lives = GameConfig.MaxLives;
            UserManager.CurrentUser.NextLifeTime = DateTime.Now;
            CoinsSystem.SpendCoins(GameConfig.LivesRefillCost);
        }
        void JumpNextLifeTimeToNext()
        {
            UserManager.CurrentUser.NextLifeTime = DateTime.Now.Add(TimeSpan.FromSeconds(GameConfig.TimeToNextLife));
        }
    }
}
