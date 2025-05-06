// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the popup that is shown before
    /// starting a game.
    /// </summary>
    public class StartGamePopup : Popup
    {
        public List<Sprite> ColorBubbleSprites;
        public List<Sprite> CollectableBubbleSprites;
        public Sprite LeafSprite;

        [SerializeField]
        private TextMeshProUGUI levelText = null;

        [SerializeField]
        private TextMeshProUGUI numBubblesText = null;

        [SerializeField]
        private Sprite enabledStarSprite = null;

        [SerializeField]
        private Image star1Image = null;

        [SerializeField]
        private Image star2Image = null;

        [SerializeField]
        private Image star3Image = null;

        [SerializeField]
        private GameObject goalPrefab = null;

        [SerializeField]
        private GameObject goalGroup = null;

        [SerializeField]
        private GameObject playButton = null;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(levelText);
            Assert.IsNotNull(numBubblesText);
            Assert.IsNotNull(enabledStarSprite);
            Assert.IsNotNull(star1Image);
            Assert.IsNotNull(star2Image);
            Assert.IsNotNull(star3Image);
            Assert.IsNotNull(goalPrefab);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(playButton);
        }

        public void LoadLevelData(int levelNum)
        {
            LevelManager.lastSelectedLevel = levelNum;

            LevelInfo level = FileUtils.LoadLevel(levelNum);
            levelText.text = "Level " + levelNum;
            numBubblesText.text = level.NumBubbles.ToString();
            int stars = UserManager.CurrentUser.GetLevelStars(levelNum - 1);
            if (stars == 1)
            {
                star1Image.sprite = enabledStarSprite;
            }
            else if (stars == 2)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
            }
            else if (stars == 3)
            {
                star1Image.sprite = enabledStarSprite;
                star2Image.sprite = enabledStarSprite;
                star3Image.sprite = enabledStarSprite;
            }

            List<ColorBubbleType> randomColors = new();
            randomColors.AddRange(level.AvailableColors);
            randomColors.Shuffle();

            LevelManager.availableColors.Clear();
            LevelManager.availableColors.AddRange(randomColors);

            foreach (LevelGoal goal in level.Goals)
            {
                GameObject goalItem = Instantiate(goalPrefab);
                goalItem.transform.SetParent(goalGroup.transform, false);
                if (goal is CollectBubblesGoal)
                {
                    CollectBubblesGoal concreteGoal = (CollectBubblesGoal)goal;
                    goalItem.GetComponent<GoalItem>().Initialize(ColorBubbleSprites[(int)concreteGoal.Type], concreteGoal.Amount);
                }
                else if (goal is CollectRandomBubblesGoal)
                {
                    CollectRandomBubblesGoal concreteGoal = (CollectRandomBubblesGoal)goal;
                    goalItem.GetComponent<GoalItem>().Initialize(ColorBubbleSprites[(int)randomColors[(int)concreteGoal.Type]], concreteGoal.Amount);
                }
                else if (goal is CollectCollectablesGoal)
                {
                    CollectCollectablesGoal concreteGoal = (CollectCollectablesGoal)goal;
                    goalItem.GetComponent<GoalItem>().Initialize(CollectableBubbleSprites[(int)concreteGoal.Type], concreteGoal.Amount);
                }
                else if (goal is CollectLeavesGoal)
                {
                    CollectLeavesGoal concreteGoal = (CollectLeavesGoal)goal;
                    goalItem.GetComponent<GoalItem>().Initialize(LeafSprite, concreteGoal.Amount);
                }
            }
        }

        public void OnPlayButtonPressed()
        {
            GetComponent<ScreenTransition>().PerformTransition();
        }
    }
}
