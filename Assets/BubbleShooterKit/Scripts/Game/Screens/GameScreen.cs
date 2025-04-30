// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace BubbleShooterKit
{
	/// <summary>
	/// This class manages the high-level logic of the game screen.
	/// </summary>
	public class GameScreen : BaseScreen
	{
		public int LevelNum = 1;

		public GameConfiguration GameConfig;
		public GameLogic GameLogic;
		public GameScroll GameScroll;
		public Shooter Shooter;

		public CheckForFreeLives FreeLivesChecker;
		
		public BubbleFactory BubbleFactory;
		public BubblePool BubblePool;
		public FxPool FxPool;
		public ObjectPool ScoreTextPool;
		public GameObject TopLinePrefab;

		public GameUi GameUi;
		public LevelGoalsWidget LevelGoalsWidget;

		public PlayerBubbles PlayerBubbles;

		public Image BackgroundImage;

		public GameObject TopCanvas;

		public GameObject LevelCompletedAnimationPrefab;
		private GameObject levelCompletedAnimation;

		[SerializeField]
		private InGameBoostersWidget inGameBoostersWidget = null;

		[SerializeField]
		private Fox fox = null;

		[HideInInspector]
		public bool IsInputLocked;

		private Vector3 bubblePos;

		private float tileWidth;
		private float tileHeight;

		private float totalWidth;
		private float totalHeight;

		private Level level;
		private LevelInfo levelInfo;

		private List<List<Vector2>> tilePositions = new List<List<Vector2>>();

		private GameObject topLine;
		private readonly List<GameObject> leaves = new List<GameObject>();

		private void Awake()
		{
			Assert.IsNotNull(TopLinePrefab);
		}

		protected override void Start()
		{
			base.Start();
			
			var bubblePrefab = BubblePool.GetColorBubblePool(ColorBubbleType.Black).Prefab;
			tileWidth = bubblePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size.x;
			tileHeight = bubblePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size.y;
	
			InitializeObjectPools();
			InitializeLevel();
			
			BackgroundImage.sprite = levelInfo.BackgroundSprite;

			inGameBoostersWidget.Initialize(GameConfig, levelInfo);
			
			OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup =>
			{
				popup.SetGoals(levelInfo);	
			});
		}

		public void OnGameRestarted()
		{
			foreach (var cover in FindObjectsByType<IceCover>(FindObjectsSortMode.None))
				cover.GetComponent<PooledObject>().Pool.ReturnObject(cover.gameObject);
			
			foreach (var cover in FindObjectsByType<CloudCover>(FindObjectsSortMode.None))
				cover.GetComponent<PooledObject>().Pool.ReturnObject(cover.gameObject);
			
			ResetObjectPools();
			
			Destroy(topLine);

			GameLogic.Reset();
			GameScroll.Reset();
			
			leaves.Clear();
			tilePositions.Clear();
			
			LevelGoalsWidget.Reset();
			
			BubbleFactory.Reset();
		}

		private void InitializeObjectPools()
		{
            foreach (var pool in FxPool.GetComponentsInChildren<ObjectPool>())
                pool.Initialize();
			
			ScoreTextPool.Initialize();
		}

		private void ResetObjectPools()
		{
            foreach (var pool in BubblePool.GetComponentsInChildren<ObjectPool>())
                pool.Reset();

            foreach (var pool in FxPool.GetComponentsInChildren<ObjectPool>())
                pool.Reset();
			
			ScoreTextPool.Reset();
		}

		public void InitializeLevel()
		{
			var lastSelectedLevel = PlayerPrefs.GetInt("last_selected_level");
			if (lastSelectedLevel == 0)
				lastSelectedLevel = LevelNum;
			
			LoadLevel(lastSelectedLevel);
			BubbleFactory.PreLevelInitialize(levelInfo);	
			CreateLevel();
			BubbleFactory.PostLevelInitialize(level);
			Shooter.Initialize(tileHeight);
		
			GameUi.ScoreWidget.Fill(levelInfo.Star1Score, levelInfo.Star2Score, levelInfo.Star3Score);
		
			LevelGoalsWidget.Initialize(levelInfo.Goals, BubbleFactory.RandomizedColorBubblePrefabs);
			
			PlayerBubbles.Initialize(levelInfo);
			
			GameLogic.SetGameInfo(level, levelInfo, tileWidth, tileHeight, totalWidth, totalHeight, tilePositions, leaves);
			GameScroll.SetGameInfo(level, tileHeight, tilePositions, topLine, leaves);
		}

		private void LoadLevel(int levelNum)
		{
            levelInfo = FileUtils.LoadLevel(levelNum);
			level = new Level(levelInfo.Rows, levelInfo.Columns);
		}

		private void CreateLevel()
		{
			const float tileWidthMultiplier = GameplayConstants.TileWidthMultiplier;
			const float tileHeightMultiplier = GameplayConstants.TileHeightMultiplier;

			tilePositions = new List<List<Vector2>>();
			var evenWidth = level.Columns;
			var oddWidth = level.Columns - 1;
			for (var i = 0; i < level.Rows; i++)
			{
				if (i % 2 == 0)
				{
					var row = new List<Vector2>(evenWidth);
					row.AddRange(Enumerable.Repeat(new Vector2(), evenWidth));
					tilePositions.Add(row);
				}
				else
				{
					var row = new List<Vector2>(oddWidth);
					row.AddRange(Enumerable.Repeat(new Vector2(), oddWidth));
					tilePositions.Add(row);
				}
			}
			
			for (var j = 0; j < level.Rows; j++)
			{
				var selectedRow = level.Tiles[j];
				for (var i = 0; i < selectedRow.Count; i++)
				{
					float rowOffset;
					if (j % 2 == 0)
						rowOffset = 0;
					else
						rowOffset = tileWidth * 0.5f;
					
					tilePositions[j][i] = new Vector2(
						(i * tileWidth * tileWidthMultiplier) + rowOffset,
						-j * tileHeight * tileHeightMultiplier);	
				}
			}
			
			totalWidth = level.Columns * tileWidth * tileWidthMultiplier;
			totalHeight = level.Rows * tileHeight * tileHeightMultiplier;
			
			Camera.main.orthographicSize = (totalWidth * 1.02f) * (Screen.height / (float)Screen.width) * 0.5f;

			var bottomPivot = new Vector2(0, Camera.main.pixelHeight * GameplayConstants.BottomPivotHeight);
			var bottomPivotPos = Camera.main.ScreenToWorldPoint(bottomPivot);
			foreach (var row in tilePositions)
			{
				for (var i = 0; i < row.Count; i++)
				{
					var tile = row[i];
					var newPos = tile;
					newPos.x -= totalWidth / 2f;
					newPos.x += (tileWidth * tileWidthMultiplier) / 2f;
					newPos.y += bottomPivotPos.y + totalHeight;
					row[i] = newPos;
				}
			}
			
			for (var j = 0; j < level.Rows; j++)
			{
				var selectedRow = level.Tiles[j];
				for (var i = 0; i < selectedRow.Count; i++)
				{
					var tileInfo = levelInfo.Tiles[j].Tiles[i];
					var tile = BubbleFactory.CreateBubble(tileInfo);
					if (tile != null)
					{
						tile.transform.position = tilePositions[j][i];
						level.Tiles[j][i] = tile.GetComponent<Bubble>();
						tile.GetComponent<Bubble>().Row = j;
						tile.GetComponent<Bubble>().Column = i;
					}
				}
			}

			DrawTopLine();
			DrawTopLeaves();
		}

		private void DrawTopLine()
		{
			topLine = Instantiate(TopLinePrefab);
			var topRowHeight = GetTopRowHeight();
			var newPos = topLine.transform.position;
			newPos.y = topRowHeight + (tileHeight * 0.6f);
			topLine.transform.position = newPos;
		}

		private void DrawTopLeaves()
		{
			if (levelInfo.Goals.Find(x => x is CollectLeavesGoal) != null)
			{
				var topRowHeight = GetTopRowHeight();
				for (var i = 0; i < level.Columns; i++)
				{
					if (level.Tiles[0][i] != null)
					{
						var leaf = BubblePool.LeafPool.GetObject();
						leaf.GetComponent<Leaf>().FxPool = FxPool;
						leaf.transform.position = new Vector2(tilePositions[0][i].x, topRowHeight + tileHeight);
						leaves.Add(leaf);
					}
					else
					{
						leaves.Add(null);
					}
				}
			}
		}

		private float GetTopRowHeight()
		{
			var topRow = level.Tiles[0];
			var topRowHeight = 0f;
			foreach (var tile in topRow)
			{
				if (tile != null)
				{
					topRowHeight = tile.transform.position.y;
					break;
				}
			}

			return topRowHeight;
		}

		public void LockInput()
		{
			IsInputLocked = true;
		}

		public void UnlockInput()
		{
			if (!GameLogic.IsChainingBoosters && !GameLogic.IsChainingVoids)
				IsInputLocked = false;
		}

		public void MoveNeighbours(Bubble shotColorBubble, int row, float strength)
		{
			if (row < 0 || row >= level.Rows)
				return;
			
			foreach (var bubble in level.Tiles[row])
			{
				if (bubble != null)
				{
					if (Math.Abs(bubble.Column - shotColorBubble.Column) <= 1)
					{
						var offsetDir = bubble.transform.position - shotColorBubble.transform.position;
						offsetDir.Normalize();
						ShakeBubble(bubble, offsetDir, strength);
					}
				}
			}
		}

		public Sequence ShakeBubble(Bubble bubble, Vector3 offsetDir, float strength)
		{
			var seq = DOTween.Sequence();
			var child = bubble.transform.GetChild(0);
			seq.Append(child.transform.DOBlendableMoveBy(offsetDir * strength, 0.15f)
				.SetEase(Ease.Linear));
			seq.Append(child.transform.DOBlendableMoveBy(-offsetDir * strength, 0.2f).SetEase(Ease.Linear));
			seq.Play();

			var colorBubble = bubble.GetComponent<ColorBubble>();
			if (colorBubble != null && colorBubble.CoverType != CoverType.None)
			{
				seq = DOTween.Sequence();
				var cover = bubble.transform.GetChild(1);
				seq.Append(cover.transform.DOBlendableMoveBy(offsetDir * strength, 0.15f)
					.SetEase(Ease.Linear));
				seq.Append(cover.transform.DOBlendableMoveBy(-offsetDir * strength, 0.2f).SetEase(Ease.Linear));
				seq.Play();
			}

			return seq;
		}

		public bool CanPlayerShoot()
		{
			return PlayerBubbles.NumBubblesLeft >= 1 &&
			       !IsInputLocked &&
			       GameLogic.GameStarted &&
				   CurrentPopups.Count == 0;
		}

        public IEnumerator OpenWinPopupAsync()
        {
	        fox.PlayHappyAnimation();
            yield return new WaitForSeconds(GameplayConstants.WinPopupDelay);
            OpenWinPopup();
        }
		
        public IEnumerator OpenLosePopupAsync()
        {
	        fox.PlaySadAnimation();
            yield return new WaitForSeconds(GameplayConstants.LosePopupDelay);
            OpenLosePopup();
        }
		
        public void OpenWinPopup()
        {
            OpenPopup<WinPopup>("Popups/WinPopup", popup =>
            {
	            var gameState = GameLogic.GameState;
                var levelStars = PlayerPrefs.GetInt("level_stars_" + levelInfo.Number);
                if (gameState.Score >= levelInfo.Star3Score)
                {
                    popup.SetStars(3);
                    PlayerPrefs.SetInt("level_stars_" + levelInfo.Number, 3);
                }
                else if (gameState.Score >= levelInfo.Star2Score)
                {
                    popup.SetStars(2);
                    if (levelStars < 3)
                    {
                        PlayerPrefs.SetInt("level_stars_" + levelInfo.Number, 2);
                    }
                }
                else if (gameState.Score >= levelInfo.Star3Score)
                {
                    popup.SetStars(1);
                    if (levelStars < 2)
                    {
                        PlayerPrefs.SetInt("level_stars_" + levelInfo.Number, 1);
                    }
                }
                else
                {
                    popup.SetStars(0);
                }

                var levelScore = PlayerPrefs.GetInt("level_score_" + levelInfo.Number);
                if (levelScore < gameState.Score)
                {
                    PlayerPrefs.SetInt("level_score_" + levelInfo.Number, gameState.Score);
                }

                popup.SetScore(gameState.Score);
                popup.SetGoals(levelInfo.Goals, gameState, LevelGoalsWidget);
            });
        }

        public void OpenLosePopup()
        {
            FreeLivesChecker.RemoveLife();
            OpenPopup<LosePopup>("Popups/LosePopup", popup =>
            {
	            var gameState = GameLogic.GameState;
                popup.SetScore(gameState.Score);
                popup.SetGoals(levelInfo.Goals, gameState, LevelGoalsWidget);
            });
        }

		public IEnumerator OpenOutOfBubblesPopupAsync()
		{
			yield return new WaitForSeconds(GameplayConstants.OutOfBubblesPopupDelay);
			OpenOutOfBubblesPopup();
		}

		private void OpenOutOfBubblesPopup()
		{
			OpenPopup<OutOfBubblesPopup>("Popups/OutOfBubblesPopup", popup =>
			{
				popup.SetInfo(this);
				OpenTopCanvas();
			});
		}

		public void OpenCoinsPopup()
		{
			OpenPopup<BuyCoinsPopup>("Popups/BuyCoinsPopup");
		}

		public void OpenLevelCompletedAnimation()
		{
			SoundPlayer.PlaySoundFx("LevelComplete");
			levelCompletedAnimation = Instantiate(LevelCompletedAnimationPrefab);
			levelCompletedAnimation.transform.SetParent(Canvas.transform, false);
		}

		public void CloseLevelCompletedAnimation()
		{
			if (levelCompletedAnimation != null)
				Destroy(levelCompletedAnimation);
		}

		public void OpenTopCanvas()
		{
			TopCanvas.SetActive(true);				
		}

		public void CloseTopCanvas()
		{
			TopCanvas.SetActive(false);				
		}

		public void OnPauseButtonPressed()
		{
			if (!PlayerBubbles.IsPlayingEndGameSequence())
			{
				LockInput();
				OpenPopup<PausePopup>("Popups/PausePopup");
			}
		}

		public void PenalizePlayer()
		{
			FreeLivesChecker.RemoveLife();
		}

		public void OnGameContinued()
		{
			CloseTopCanvas();
			UnlockInput();
		}

		public void ApplyBooster(PurchasableBoosterBubbleType boosterBubbleType)
		{
			switch (boosterBubbleType)
			{
				case PurchasableBoosterBubbleType.SuperAim:
					Shooter.ApplySuperAim();
					break;
				
				case PurchasableBoosterBubbleType.RainbowBubble:
				case PurchasableBoosterBubbleType.HorizontalBomb:
				case PurchasableBoosterBubbleType.CircleBomb:
					PlayerBubbles.CreatePurchasableBoosterBubble(boosterBubbleType);
					break;
			}
		}
	}
}
