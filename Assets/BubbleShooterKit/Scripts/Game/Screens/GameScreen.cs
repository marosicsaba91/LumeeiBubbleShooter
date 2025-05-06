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

        private List<List<Vector2>> tilePositions = new();

        private GameObject topLine;
        private readonly List<GameObject> leaves = new();

        private void Awake()
        {
            Assert.IsNotNull(TopLinePrefab);
        }

        protected override void Start()
        {
            base.Start();

            GameObject bubblePrefab = BubblePool.GetColorBubblePool(ColorBubbleType.Black).Prefab;
            tileWidth = bubblePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size.x;
            tileHeight = bubblePrefab.GetComponentInChildren<SpriteRenderer>().bounds.size.y;

            InitializeObjectPools();
            InitializeLevel();

            BackgroundImage.sprite = levelInfo.BackgroundSprite;

            inGameBoostersWidget.Initialize(levelInfo);

            OpenPopup<LevelGoalsPopup>("Popups/LevelGoalsPopup", popup =>
            {
                popup.SetGoals(levelInfo);
            });
        }

        public void OnGameRestarted()
        {
            foreach (IceCover cover in FindObjectsByType<IceCover>(FindObjectsSortMode.None))
                cover.GetComponent<PooledObject>().Pool.ReturnObject(cover.gameObject);

            foreach (CloudCover cover in FindObjectsByType<CloudCover>(FindObjectsSortMode.None))
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
            foreach (ObjectPool pool in FxPool.GetComponentsInChildren<ObjectPool>())
                pool.Initialize();

            ScoreTextPool.Initialize();
        }

        private void ResetObjectPools()
        {
            foreach (ObjectPool pool in BubblePool.GetComponentsInChildren<ObjectPool>())
                pool.Reset();

            foreach (ObjectPool pool in FxPool.GetComponentsInChildren<ObjectPool>())
                pool.Reset();

            ScoreTextPool.Reset();
        }

        public void InitializeLevel()
        {
            if (LevelManager.lastSelectedLevel == 0)
                LevelManager.lastSelectedLevel = LevelNum;

            LoadLevel(LevelManager.lastSelectedLevel);
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
            int evenWidth = level.Columns;
            int oddWidth = level.Columns - 1;
            for (int i = 0; i < level.Rows; i++)
            {
                if (i % 2 == 0)
                {
                    List<Vector2> row = new(evenWidth);
                    row.AddRange(Enumerable.Repeat(new Vector2(), evenWidth));
                    tilePositions.Add(row);
                }
                else
                {
                    List<Vector2> row = new(oddWidth);
                    row.AddRange(Enumerable.Repeat(new Vector2(), oddWidth));
                    tilePositions.Add(row);
                }
            }

            for (int j = 0; j < level.Rows; j++)
            {
                List<Bubble> selectedRow = level.Tiles[j];
                for (int i = 0; i < selectedRow.Count; i++)
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

            Vector2 bottomPivot = new(0, Camera.main.pixelHeight * GameplayConstants.BottomPivotHeight);
            Vector3 bottomPivotPos = Camera.main.ScreenToWorldPoint(bottomPivot);
            foreach (List<Vector2> row in tilePositions)
            {
                for (int i = 0; i < row.Count; i++)
                {
                    Vector2 tile = row[i];
                    Vector2 newPos = tile;
                    newPos.x -= totalWidth / 2f;
                    newPos.x += (tileWidth * tileWidthMultiplier) / 2f;
                    newPos.y += bottomPivotPos.y + totalHeight;
                    row[i] = newPos;
                }
            }

            for (int j = 0; j < level.Rows; j++)
            {
                List<Bubble> selectedRow = level.Tiles[j];
                for (int i = 0; i < selectedRow.Count; i++)
                {
                    TileInfo tileInfo = levelInfo.Tiles[j].Tiles[i];
                    GameObject tile = BubbleFactory.CreateBubble(tileInfo);
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
            float topRowHeight = GetTopRowHeight();
            Vector3 newPos = topLine.transform.position;
            newPos.y = topRowHeight + (tileHeight * 0.6f);
            topLine.transform.position = newPos;
        }

        private void DrawTopLeaves()
        {
            if (levelInfo.Goals.Find(x => x is CollectLeavesGoal) != null)
            {
                float topRowHeight = GetTopRowHeight();
                for (int i = 0; i < level.Columns; i++)
                {
                    if (level.Tiles[0][i] != null)
                    {
                        GameObject leaf = BubblePool.LeafPool.GetObject();
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
            List<Bubble> topRow = level.Tiles[0];
            float topRowHeight = 0f;
            foreach (Bubble tile in topRow)
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

            foreach (Bubble bubble in level.Tiles[row])
            {
                if (bubble != null)
                {
                    if (Math.Abs(bubble.Column - shotColorBubble.Column) <= 1)
                    {
                        Vector3 offsetDir = bubble.transform.position - shotColorBubble.transform.position;
                        offsetDir.Normalize();
                        ShakeBubble(bubble, offsetDir, strength);
                    }
                }
            }
        }

        public Sequence ShakeBubble(Bubble bubble, Vector3 offsetDir, float strength)
        {
            Sequence seq = DOTween.Sequence();
            Transform child = bubble.transform.GetChild(0);
            seq.Append(child.transform.DOBlendableMoveBy(offsetDir * strength, 0.15f)
                .SetEase(Ease.Linear));
            seq.Append(child.transform.DOBlendableMoveBy(-offsetDir * strength, 0.2f).SetEase(Ease.Linear));
            seq.Play();

            ColorBubble colorBubble = bubble.GetComponent<ColorBubble>();
            if (colorBubble != null && colorBubble.CoverType != CoverType.None)
            {
                seq = DOTween.Sequence();
                Transform cover = bubble.transform.GetChild(1);
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
                GameState gameState = GameLogic.GameState;

                int levelIndex = levelInfo.Number-1;

                int lastLevelStars = UserManager.CurrentUser.GetLevelStars(levelIndex);
                int lastLevelScore = UserManager.CurrentUser.GetLevelScore(levelIndex);
                int currentScore = gameState.Score;

                int currentStars =
                    currentScore >= levelInfo.Star3Score ? 3 : 
                    currentScore >= levelInfo.Star2Score ? 2 :
                    currentScore >= levelInfo.Star1Score ? 1 :
                    0;

                popup.SetStars(currentStars);
                if (lastLevelStars < currentStars)
                    UserManager.CurrentUser.SetLevelStars(levelIndex, currentStars);

                if (lastLevelScore < currentScore)
                    UserManager.CurrentUser.SetLevelScore(levelIndex, currentScore);

                popup.SetScore(currentScore);
                popup.SetGoals(levelInfo.Goals, gameState, LevelGoalsWidget);
            });
        }

        public void OpenLosePopup()
        {
            FreeLivesChecker.RemoveOneLife();
            OpenPopup<LosePopup>("Popups/LosePopup", popup =>
            {
                GameState gameState = GameLogic.GameState;
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
            FreeLivesChecker.RemoveOneLife();
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
