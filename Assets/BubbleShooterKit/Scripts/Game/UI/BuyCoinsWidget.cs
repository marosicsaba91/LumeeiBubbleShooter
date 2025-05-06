// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class is used to manage the widget to buy coins that is located in the level screen.
    /// </summary>
    public class BuyCoinsWidget : MonoBehaviour
    {
        public GameConfiguration GameConfig;
        public CoinsSystem CoinsSystem;
        
        [SerializeField]
        private TextMeshProUGUI numCoinsText = null;

        private void Awake()
        {
            Assert.IsNotNull(numCoinsText);
        }

        private void Start()
        {
            int numCoins = UserManager.CurrentUser.coins;
            numCoinsText.text = numCoins.ToString("n0");
            CoinsSystem.Subscribe(OnCoinsChanged);
        }

        private void OnDestroy()
        {
            CoinsSystem.Unsubscribe(OnCoinsChanged);
        }

        public void OnBuyButtonPressed()
        {
            BaseScreen scene = FindFirstObjectByType<BaseScreen>();
            BuyCoinsPopup buyCoinsPopup = FindFirstObjectByType<BuyCoinsPopup>();
            if (scene != null && buyCoinsPopup == null)
                scene.OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup");
        }

        private void OnCoinsChanged(int numCoins)
        {
            numCoinsText.text = numCoins.ToString("n0");
        }
    }
}
