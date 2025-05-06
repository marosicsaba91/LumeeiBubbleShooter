// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class manages the high-level logic of the level screen.
    /// </summary>
    public class LevelScreen : BaseScreen
    {
        [SerializeField]
        private ScrollRect scrollRect = null;

        [SerializeField]
        private GameObject scrollView = null;

        [SerializeField]
        private GameObject avatarPrefab = null;

        private void Awake()
        {
            Assert.IsNotNull(scrollRect);
            Assert.IsNotNull(scrollView);
            Assert.IsNotNull(avatarPrefab);
        }

        protected override void Start()
        {
            base.Start();

            scrollRect.vertical = false;

            GameObject avatar = Instantiate(avatarPrefab, scrollView.transform, false);

            int nextLevel = UserManager.CurrentUser.GetNextLevelIndex() + 1;

            LevelButton currentButton = null;
            LevelButton[] levelButtons = scrollView.GetComponentsInChildren<LevelButton>();
            foreach (LevelButton button in levelButtons)
            {
                if (button.NumLevel != nextLevel)
                    continue;
                currentButton = button;
                break;
            }

            if (currentButton == null)
                currentButton = levelButtons[levelButtons.Length - 1];

            Vector2 newPos = scrollView.GetComponent<RectTransform>().anchoredPosition;
            newPos.y =
                scrollRect.transform.InverseTransformPoint(scrollView.GetComponent<RectTransform>().position).y -
                scrollRect.transform.InverseTransformPoint(currentButton.transform.position).y;
            newPos.y += Canvas.GetComponent<RectTransform>().rect.height / 2.0f;
            if (newPos.y < scrollView.GetComponent<RectTransform>().anchoredPosition.y)
                scrollView.GetComponent<RectTransform>().anchoredPosition = newPos;

            Vector3 targetPos = currentButton.transform.position + new Vector3(0, 1.0f, 0);

            LevelButton prevButton = null;
            if (LevelManager.unlockedNextLevel)
            {
                foreach (LevelButton button in scrollView.GetComponentsInChildren<LevelButton>())
                {
                    if (button.NumLevel != LevelManager.lastSelectedLevel)
                        continue;
                    prevButton = button;
                    break;
                }
            }

            if (prevButton != null)
            {
                avatar.transform.position = prevButton.transform.position + new Vector3(0, 1.0f, 0);
                Sequence sequence = DOTween.Sequence();
                sequence.AppendInterval(0.5f);
                sequence.Append(avatar.transform.DOMove(targetPos, 0.8f));
                sequence.AppendCallback(() => avatar.GetComponent<LevelAvatar>().StartFloatingAnimation());
                sequence.AppendCallback(() => scrollRect.vertical = true);
            }
            else
            {
                avatar.transform.position = targetPos;
                avatar.GetComponent<LevelAvatar>().StartFloatingAnimation();
                scrollRect.vertical = true;
            }
        }
    }
}
