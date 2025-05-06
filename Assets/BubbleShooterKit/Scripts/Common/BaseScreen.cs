// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// The base class all the screens in the game derive from, which provides
    /// basic functionality like opening and closing popups. Every scene in the
    /// game has an associated screen game object.
    /// </summary>
	public class BaseScreen : MonoBehaviour
	{
	    [SerializeField]
        protected Canvas Canvas;

        public readonly Stack<GameObject> CurrentPopups = new();
	    
	    private readonly Stack<GameObject> currentPanels = new();

	    protected virtual void Start()
	    {
	        SoundPlayer.Initialize();
	    }

        public void OpenPopup<T>(string popupName, Action<T> onOpened = null) where T : Popup
        {
            StartCoroutine(OpenPopupAsync(popupName, onOpened));
        }

        public void CloseCurrentPopup()
        {
            GameObject currentPopup = CurrentPopups.Peek();
            if (currentPopup != null)
                currentPopup.GetComponent<Popup>().Close();
        }

        public void ClosePopup()
        {
            GameObject topmostPopup = CurrentPopups.Pop();
            if (topmostPopup == null)
                return;

            GameObject topmostPanel = currentPanels.Pop();
            if (topmostPanel != null)
            {
                Sequence seq = DOTween.Sequence();
                seq.Append(topmostPanel.GetComponent<Image>().DOFade(0.0f, 0.2f));
                seq.AppendCallback(() => Destroy(topmostPanel));
            }
        }

	    private IEnumerator OpenPopupAsync<T>(string popupName, Action<T> onOpened) where T : Popup
        {
            ResourceRequest request = Resources.LoadAsync<GameObject>(popupName);
            while (!request.isDone)
                yield return null;

            GameObject panel = new("Panel");
            Image panelImage = panel.AddComponent<Image>();
            Color color = Color.black;
            color.a = 0;
            panelImage.color = color;
            RectTransform panelTransform = panel.GetComponent<RectTransform>();
            panelTransform.anchorMin = new Vector2(0, 0);
            panelTransform.anchorMax = new Vector2(1, 1);
            panelTransform.pivot = new Vector2(0.5f, 0.5f);
            panel.transform.SetParent(Canvas.transform, false);
            currentPanels.Push(panel);
            Sequence seq = DOTween.Sequence();
            seq.Append(panel.GetComponent<Image>().DOFade(220.0f/256.0f, 0.2f));

            GameObject popup = Instantiate(request.asset) as GameObject;
            Assert.IsNotNull((popup));
            popup.transform.SetParent(Canvas.transform, false);
            popup.GetComponent<Popup>().ParentScreen = this;

            onOpened?.Invoke(popup.GetComponent<T>());
            CurrentPopups.Push(popup);
        }
	}
}
