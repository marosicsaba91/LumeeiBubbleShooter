// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class contains the logic associated to the end game (win/lose) popup.
    /// </summary>
	public class EndGamePopup : Popup
	{
        [SerializeField]
        private TextMeshProUGUI scoreText = null;

        [SerializeField]
        private GameObject goalGroup = null;

	    [SerializeField]
	    private GameObject endGameGoalWidgetPrefab = null;
	    
        protected override void Awake()
        {
            base.Awake();
            Assert.IsNotNull(scoreText);
            Assert.IsNotNull(goalGroup);
            Assert.IsNotNull(endGameGoalWidgetPrefab);
        }

	    protected override void Start()
	    {
	        base.Start();
	        if (UserManager.CurrentUser.soundEnabled)
    	        GetComponent<AudioSource>().Play();
	    }

	    public void OnCloseButtonPressed()
	    {
	        GetComponent<ScreenTransition>().PerformTransition();
	    }

        public void OnReplayButtonPressed()
        {
            GameScreen gameScreen = ParentScreen as GameScreen;
            if (gameScreen != null)
            { 
                if (UserManager.CurrentUser.lives > 0)
                {
                    gameScreen.GameLogic.RestartGame();
                    gameScreen.CloseTopCanvas();
                    Close();
                }
                else
                {
                    gameScreen.OpenPopup<BuyLivesPopup>("Popups/BuyLivesPopup");
                }
            }
        }

        public void SetScore(int score)
        {
            scoreText.text = score.ToString();
        }

        public void SetGoals(List<LevelGoal> goals, GameState gameState, LevelGoalsWidget goalsWidget)
        {
            int i = 0;
            foreach (LevelGoal goal in goals)
            {
                GameObject goalObject = Instantiate(endGameGoalWidgetPrefab);
                goalObject.transform.SetParent(goalGroup.transform, false);
                goalObject.GetComponent<EndGameGoalWidget>().Initialize(
                    goal.IsComplete(gameState),
                    goalsWidget.transform.GetChild(i).GetComponent<LevelGoalWidget>());
                ++i;
            }
        }
	}
}
