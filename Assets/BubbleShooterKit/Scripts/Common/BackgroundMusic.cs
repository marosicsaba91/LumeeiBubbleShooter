// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class manages the background music of the game.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class BackgroundMusic : MonoBehaviour
    {
        void Awake()
        {
            DontDestroyOnLoad(gameObject);
            GetComponent<AudioSource>().mute = !UserManager.CurrentUser.musicEnabled;
        }
    }
}
