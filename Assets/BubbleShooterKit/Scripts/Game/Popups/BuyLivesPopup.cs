// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the popup for buying lives.
    /// </summary>
    public class BuyLivesPopup : Popup
    {
        public GameConfiguration GameConfig;

        [SerializeField]
        private TextMeshProUGUI refillCostText = null;

        [SerializeField]
        private AnimatedButton refillButton = null;

        [SerializeField]
        private Image refillButtonImage = null;

        [SerializeField]
        private Sprite refillButtonDisabledSprite = null;

        [SerializeField]
        private ParticleSystem livesParticles = null;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(refillCostText);
            Assert.IsNotNull(refillButton);
            Assert.IsNotNull(refillButtonImage);
            Assert.IsNotNull(refillButtonDisabledSprite);
            Assert.IsNotNull(livesParticles);
        }

        protected override void Start()
        {
            base.Start();
            int maxLives = GameConfig.MaxLives; 
            if (UserManager.CurrentUser.lives >= maxLives)
                DisableRefillButton();
            refillCostText.text = GameConfig.LivesRefillCost.ToString();
        }

        public void OnRefillButtonPressed()
        {
            int numCoins = UserManager.CurrentUser.coins;
            if (numCoins >= GameConfig.LivesRefillCost)
            {
                CheckForFreeLives freeLivesChecker = FindFirstObjectByType<CheckForFreeLives>();
                if (freeLivesChecker != null)
                {
                    freeLivesChecker.RefillLives_ForCoins();

                    livesParticles.Play();
                    //SoundManager.instance.PlaySound("BuyPopButton");
                    DisableRefillButton();
                }
            }
            else
            {
                BaseScreen screen = ParentScreen;
                if (screen != null)
                {
                    screen.CloseCurrentPopup();
                    //SoundManager.instance.PlaySound("Button");
                    screen.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup",
                        popup =>
                        {
                            popup.OnClose.AddListener(
                                () =>
                                {
                                    screen.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                                });
                        });
                }
            }
        }

        private void DisableRefillButton()
        {
            refillButtonImage.sprite = refillButtonDisabledSprite;
            refillButton.Interactable = false;
        }
    }
}
