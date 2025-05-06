// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the settings popup that can be
    /// accessed from the home screen.
    /// </summary>
	public class SettingsPopup : Popup
	{
        [SerializeField]
        private Slider soundSlider = null;

        [SerializeField]
        private Slider musicSlider = null;

        [SerializeField]
        private AnimatedButton resetProgressButton = null;

        [SerializeField]
        private Image resetProgressImage = null;

        [SerializeField]
        private Sprite resetProgressDisabledSprite = null;

        private int currentSound;
        private int currentMusic;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(soundSlider);
            Assert.IsNotNull(musicSlider);
            Assert.IsNotNull(resetProgressButton);
            Assert.IsNotNull(resetProgressImage);
            Assert.IsNotNull(resetProgressDisabledSprite);
        }

        protected override void Start()
        {
            base.Start();
            soundSlider.value = UserManager.CurrentUser.soundEnabled ? 0 : 1;
            musicSlider.value = UserManager.CurrentUser.musicEnabled ? 0 : 1;
        }

        public void OnResetProgressButtonPressed()
        {
            LevelManager.lastSelectedLevel = 0;
            UserManager.CurrentUser.levelData.Clear();
            resetProgressImage.sprite = resetProgressDisabledSprite;
            resetProgressButton.Interactable = false;
        }

        public void OnSoundSliderValueChanged()
        {
            User user = UserManager.CurrentUser;
            user.soundEnabled = Mathf.RoundToInt(soundSlider.value) == 1;
        }

        public void OnMusicSliderValueChanged()
        {
            User user = UserManager.CurrentUser;
            user.musicEnabled = Mathf.RoundToInt(musicSlider.value) == 1;
        }
	}
}
