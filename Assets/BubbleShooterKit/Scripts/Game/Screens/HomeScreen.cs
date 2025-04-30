// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace BubbleShooterKit
{
	/// <summary>
	/// This class manages the high-level logic of the home screen.
	/// </summary>
	public class HomeScreen : BaseScreen
	{
#pragma warning disable 649
		[SerializeField]
		private GameObject bgMusicPrefab;

		[SerializeField]
		private GameObject adsSystemPrefab;
		
		[SerializeField]
		private GameObject purchaseManagerPrefab;
#pragma warning restore 649
		
		protected override void Start()
		{
			base.Start();
			
			var bgMusic = FindFirstObjectByType<BackgroundMusic>();
			if (bgMusic == null)
				Instantiate(bgMusicPrefab);

			var adsSystem = FindFirstObjectByType<AdsSystem>();
			if (adsSystem == null)
				Instantiate(adsSystemPrefab);
			
#if UNITY_IAP
			var purchaseManager = FindFirstObjectByType<PurchaseManager>();
			if (purchaseManager == null)
				Instantiate(purchaseManagerPrefab);
#endif
		}
		
        public void OnSettingsButtonPressed()
        {
            OpenPopup<SettingsPopup>("Popups/SettingsPopup");
        }
	}
}
