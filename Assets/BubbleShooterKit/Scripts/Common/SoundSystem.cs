// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooterKit
{
	/// <summary>
	/// The sound system handles the sound pool of the current scene.
	/// </summary>
	public class SoundSystem : MonoBehaviour
	{
		public List<SoundCollection> Collections;

		private ObjectPool soundFxPool;
		private readonly Dictionary<string, AudioClip> nameToSound = new();

		private void Awake()
		{			
			soundFxPool = GetComponent<ObjectPool>();
			foreach (SoundCollection collection in Collections)
				foreach (AudioClip sound in collection.Sounds)
					nameToSound.Add(sound.name, sound);
		}

		private void Start()
		{
			soundFxPool.Initialize();
		}

		public void PlaySoundFx(string soundName)
		{
            AudioClip clip = nameToSound[soundName];
			if (clip != null)
				PlaySoundFx(clip);
		}

		private void PlaySoundFx(AudioClip clip)
		{ 
            if (UserManager.CurrentUser.soundEnabled && clip != null)
                soundFxPool.GetObject().GetComponent<SoundFx>().Play(clip);
		}
		
        public void SetSoundEnabled(bool soundEnabled)
        {
            UserManager.CurrentUser.soundEnabled = soundEnabled;
        }

        public void SetMusicEnabled(bool musicEnabled)
        {
            UserManager.CurrentUser.musicEnabled = musicEnabled;
            BackgroundMusic bgMusic = FindFirstObjectByType<BackgroundMusic>();
	        if (bgMusic != null)
	            bgMusic.GetComponent<AudioSource>().mute = !musicEnabled;
        }
	}
}
