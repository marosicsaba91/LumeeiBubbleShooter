// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
#if UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace BubbleShooterKit
{
	/// <summary>
	/// The rewarded advertisement button that is present in the level scene.
	/// </summary>
#if UNITY_ADS
	public class RewardedAdButton : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
#else
	public class RewardedAdButton : MonoBehaviour
#endif
	{
#pragma warning disable 649
        [SerializeField]
        private GameConfiguration gameConfig;

        [SerializeField]
        private CoinsSystem coinsSystem;

        [SerializeField]
        private LevelScreen levelScreen;
#pragma warning restore 649

        private AnimatedButton button;

        private string adUnitId;

        private const string iOSAdUnitId = "Rewarded_iOS";
        private const string androidAdUnitId = "Rewarded_Android";

        private void Awake()
        {
            button = GetComponent<AnimatedButton>();
        }

        private void Start()
        {
            gameObject.SetActive(false);

            adUnitId = (Application.platform == RuntimePlatform.IPhonePlayer)
                ? iOSAdUnitId
                : androidAdUnitId;

#if UNITY_ADS
            if (Advertisement.isInitialized)
            {
                LoadAd();
            }
#endif
        }

        public void ShowAd()
        {
#if UNITY_ADS
            Advertisement.Show(adUnitId, this);
#endif
        }

#if UNITY_ADS
        public void LoadAd()
        {
            Advertisement.Load(adUnitId, this);
        }

        public void OnInitializationComplete()
        {
            LoadAd();
        }

        public void OnInitializationFailed(UnityAdsInitializationError error, string message)
        {
            //Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
        }

        public void OnUnityAdsAdLoaded(string id)
        {
            if (id.Equals(adUnitId))
            {
                gameObject.SetActive(true);
            }
        }

        public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
        {
            if (adUnitId.Equals(adUnitId) && showCompletionState.Equals(UnityAdsShowCompletionState.COMPLETED))
            {
				// Reward the user for watching the ad to completion.
                var rewardCoins = gameConfig.RewardedAdCoins;
				coinsSystem.BuyCoins(rewardCoins);
				levelScreen.OpenPopup<AlertPopup>("Popups/AlertPopup",
					popup => { popup.SetText($"You earned {rewardCoins} coins!"); });

                // Load another ad.
                Advertisement.Load(adUnitId, this);
            }
        }

        public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
        {
            //Debug.Log($"Error loading Ad Unit {adUnitId}: {error.ToString()} - {message}");
        }

        public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
        {
            //Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
        }

        public void OnUnityAdsShowStart(string adUnitId) { }
        public void OnUnityAdsShowClick(string adUnitId) { }
#endif
	}
}
