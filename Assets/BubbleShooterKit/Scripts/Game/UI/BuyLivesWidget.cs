// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class is used to manage the widget to buy lives that is located in the level screen.
    /// </summary>
    public class BuyLivesWidget : MonoBehaviour
    {
        public GameConfiguration GameConfig;

        [SerializeField]
        private Sprite enabledLifeSprite = null;

        [SerializeField]
        private Sprite disabledLifeSprite = null;

        [SerializeField]
        private Image lifeImage = null;

        [SerializeField]
        private TextMeshProUGUI numLivesText = null;

        [SerializeField]
        private TextMeshProUGUI timeToNextLifeText = null;

        [SerializeField]
        private Image buttonImage = null;

        [SerializeField]
        private Sprite enabledButtonSprite = null;

        [SerializeField]
        private Sprite disabledButtonSprite = null;

        private CheckForFreeLives freeLivesChecker;

        private void Awake()
        {
            Assert.IsNotNull(enabledLifeSprite);
            Assert.IsNotNull(disabledLifeSprite);
            Assert.IsNotNull(lifeImage);
            Assert.IsNotNull(numLivesText);
            Assert.IsNotNull(timeToNextLifeText);
            Assert.IsNotNull(buttonImage);
            Assert.IsNotNull(enabledButtonSprite);
            Assert.IsNotNull(disabledButtonSprite);
        }

        void Start()
        {
            freeLivesChecker = FindFirstObjectByType<CheckForFreeLives>();

            int numLives = UserManager.CurrentUser.lives;
            int maxLives = GameConfig.MaxLives;
            numLivesText.text = numLives.ToString();
            buttonImage.sprite = numLives == maxLives ? disabledButtonSprite : enabledButtonSprite;

            freeLivesChecker.onCountdownUpdated += OnLivesCountdownUpdated;
            freeLivesChecker.onCountdownFinished += OnLivesCountdownFinished;

            int lives = UserManager.CurrentUser.lives;
            if (lives < GameConfig.MaxLives)
                OnLivesCountdownUpdated(freeLivesChecker.TimeLeft, lives);
            else
                OnLivesCountdownFinished(lives);
        }

        void OnDestroy()
        {
            freeLivesChecker.onCountdownUpdated -= OnLivesCountdownUpdated;
            freeLivesChecker.onCountdownFinished -= OnLivesCountdownFinished;
        }

        public void OnBuyButtonPressed()
        {
            if (UserManager.CurrentUser.lives < GameConfig.MaxLives)
            {
                BaseScreen scene = FindFirstObjectByType<BaseScreen>();
                BuyLivesPopup buyLivesPopup = FindFirstObjectByType<BuyLivesPopup>();
                if (scene != null && buyLivesPopup == null)
                    scene.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
            }
        }

        private void OnLivesCountdownUpdated(TimeSpan timeSpan, int lives)
        {
            timeToNextLifeText.text = $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            numLivesText.text = lives.ToString();
            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            int maxLives = GameConfig.MaxLives;
            buttonImage.sprite = lives == maxLives ? disabledButtonSprite : enabledButtonSprite;
        }

        private void OnLivesCountdownFinished(int lives)
        {
            timeToNextLifeText.text = "Full";
            numLivesText.text = lives.ToString();
            lifeImage.sprite = lives == 0 ? disabledLifeSprite : enabledLifeSprite;
            buttonImage.sprite = disabledButtonSprite;
        }
    }
}
