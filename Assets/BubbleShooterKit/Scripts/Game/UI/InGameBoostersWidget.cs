// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooterKit
{
	public class InGameBoostersWidget : MonoBehaviour
	{
		[SerializeField]
		private GameScreen gameScreen = null;
		
		[SerializeField]
		private List<Sprite> boosterSprites = null;
			
		[SerializeField]
		private List<InGameBoosterButton> buttons = null;

		public void Initialize( LevelInfo levelInfo)
		{
			for (int i = 0; i < buttons.Count; i++)
            {
                PurchasableBoosterBubbleType booster = (PurchasableBoosterBubbleType)i;
                int numBooster = UserManager.CurrentUser.GetBoosterAmount(booster);
                buttons[i].Initialize(gameScreen, gameScreen.PlayerBubbles, boosterSprites[i], numBooster, levelInfo.IsBoosterAvailable(booster));
            }
        }
	}
}
