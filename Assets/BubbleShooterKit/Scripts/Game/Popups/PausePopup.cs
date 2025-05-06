// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the in-game pause popup.
    /// </summary>
	public class PausePopup : Popup
	{
		[SerializeField]
		private AnimatedButton musicButton = null;
		
		[SerializeField]
		private AnimatedButton soundButton = null;

		protected override void Awake()
		{
			base.Awake();
			Assert.IsNotNull(musicButton);
			Assert.IsNotNull(soundButton);
		}

		protected override void Start()
		{
			base.Start();

            if (!UserManager.CurrentUser.musicEnabled)
				musicButton.GetComponent<SpriteSwapper>().SwapSprite();

			 
            if (!UserManager.CurrentUser.soundEnabled)
                soundButton.GetComponent<SpriteSwapper>().SwapSprite();
		}

		public void OnContinueButtonPressed()
		{
            GameScreen gameScreen = ParentScreen as GameScreen;
			if (gameScreen != null)
				gameScreen.UnlockInput();
			Close();
		}

		public void OnRestartButtonPressed()
		{
			ParentScreen.OpenPopup<ConfirmationPopup>("Popups/ConfirmationPopup", popup =>
			{
				popup.SetInfo("Do you really want to restart the game?", "(You will lose a life)", () =>
				{
                    GameScreen gameScreen = ParentScreen as GameScreen;
					if (gameScreen != null)
					{
						gameScreen.UnlockInput();
						gameScreen.PenalizePlayer();
						gameScreen.GameLogic.RestartGame();
					}
					
					popup.Close();
					Close();
				});
			});
		}

		public void OnQuitButtonPressed()
		{
			ParentScreen.OpenPopup<ConfirmationPopup>("Popups/ConfirmationPopup", popup =>
			{
				popup.SetInfo("Do you really want to quit the game?", "(You will lose a life)", () =>
				{
                    GameScreen gameScreen = ParentScreen as GameScreen;
					if (gameScreen != null)
						gameScreen.PenalizePlayer();
					
					GetComponent<ScreenTransition>().PerformTransition();
				});
			});
		}

		public void OnMusicButtonPressed()
        {
			User user = UserManager.CurrentUser;
            user.musicEnabled = !user.musicEnabled;
            SoundPlayer.SetSoundEnabled(user.musicEnabled);
        }

		public void OnSoundButtonPressed()
        {
            User user = UserManager.CurrentUser;
            user.soundEnabled = !user.soundEnabled; 
            SoundPlayer.SetSoundEnabled(user.soundEnabled);
		}
	}
}
