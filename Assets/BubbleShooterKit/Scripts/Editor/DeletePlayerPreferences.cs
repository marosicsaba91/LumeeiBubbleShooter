// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEditor;
using UnityEngine;

namespace BubbleShooterKit
{
    /// <summary>
    /// Utility class for deleting the PlayerPreferences from within a menu option.
    /// </summary>
    public class DeletePlayerPreferences
    {
        [MenuItem("Tools/Bubble Shooter Kit/Delete PlayerPreferences", false, 1)]
        public static void DeleteAllPlayerPreferences()
        {
            PlayerPrefs.DeleteAll();
        }
    }
}
