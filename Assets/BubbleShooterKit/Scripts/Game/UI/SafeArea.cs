// Copyright (C) 2022 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store EULA,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace BubbleShooterKit
{
    public class SafeArea : MonoBehaviour
    {
        private RectTransform panel;
        private Rect lastSafeArea = new Rect(0, 0, 0, 0);

        private void Awake()
        {
            panel = GetComponent<RectTransform>();
            Refresh();
        }

        private void Update()
        {
            Refresh();
        }

        private void Refresh()
        {
            var safeArea = GetSafeArea();
            if (safeArea != lastSafeArea)
            {
                ApplySafeArea(safeArea);
            }
        }

        private Rect GetSafeArea()
        {
            return Screen.safeArea;
        }

        private void ApplySafeArea(Rect rect)
        {
            lastSafeArea = rect;

            var anchorMin = rect.position;
            var anchorMax = rect.position + rect.size;
            anchorMin.x /= Screen.width;
            anchorMin.y /= Screen.height;
            anchorMax.x /= Screen.width;
            anchorMax.y /= Screen.height;
            panel.anchorMin = anchorMin;
            panel.anchorMax = anchorMax;
        }
    }
}