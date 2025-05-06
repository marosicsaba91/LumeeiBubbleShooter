// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the popup that shows the goals
    /// of the level when a game starts.
    /// </summary>
    public class LevelGoalsPopup : Popup
    {
        public List<Sprite> ColorBubbleSprites;
        public List<Sprite> CollectableBubbleSprites;
        public Sprite LeafSprite;

        [SerializeField]
        private GameObject goalGroup = null;

        [SerializeField]
        private GameObject goalPrefab = null;

        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(goalPrefab);
        }

        protected override void Start()
        {
            base.Start();
            StartCoroutine(AutoKill());
        }

        private IEnumerator AutoKill()
        {
            yield return new WaitForSeconds(1.5f);
            Close();
            GameScreen gameScreen = ParentScreen as GameScreen;
            if (gameScreen != null)
                gameScreen.GameLogic.StartGame();
        }

        public void SetGoals(LevelInfo levelInfo)
        {
            List<ColorBubbleType> availableColors = new();

            for (int i = 0; i < LevelManager.availableColors.Count; i++)
                availableColors.Add(LevelManager.availableColors[i]);

            LevelManager.availableColors.Clear();

            foreach (LevelGoal goal in levelInfo.Goals)
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
                    goalItem.GetComponent<GoalItem>().Initialize(ColorBubbleSprites[(int)availableColors[(int)concreteGoal.Type]], concreteGoal.Amount);
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
    }
}
