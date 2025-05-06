// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class handles the core gameplay logic of the game.
    /// </summary>
    public class GameLogic : MonoBehaviour
    {
        [SerializeField]
        private GameConfiguration gameConfig = null;

        [SerializeField]
        private GameScreen gameScreen = null;

        [SerializeField]
        private GameScroll gameScroll = null;

        [SerializeField]
        private PlayerBubbles playerBubbles = null;

        [SerializeField]
        private BubbleFactory bubbleFactory = null;

        [SerializeField]
        private GameUi gameUi = null;

        [SerializeField]
        private FxPool fxPool = null;
        [SerializeField]
        private ObjectPool scoreTextPool = null;

        public GameState GameState { get; } = new GameState();

        public bool GameStarted { get; private set; }
        public bool GameWon { get; private set; }
        public bool GameLost { get; private set; }

        private readonly List<Bubble> newBubbles = new();

        public bool IsChainingBoosters { get; private set; }
        public bool IsChainingVoids { get; private set; }

        private bool shouldChainVoids;
        private int voidCounter;

        private readonly List<Bubble> currentExplodingBubbles = new();

        private Level level;
        private LevelInfo levelInfo;
        private float tileWidth;
        private float tileHeight;
        private float totalWidth;
        private float totalHeight;
        private List<List<Vector2>> tilePositions;
        private List<GameObject> leaves;

        private Bubble lastShotBubble;
        private Bubble lastTouchedBubble;

        private bool didBubbleCollideWithTop;

        public void SetGameInfo(Level lvl, LevelInfo lvlInfo, float tileW, float tileH, float totalW, float totalH, List<List<Vector2>> positions, List<GameObject> levelLeaves)
        {
            level = lvl;
            levelInfo = lvlInfo;
            tileWidth = tileW;
            tileHeight = tileH;
            totalWidth = totalW;
            totalHeight = totalH;
            tilePositions = positions;
            leaves = levelLeaves;
        }

        public void Reset()
        {
            GameState.Reset();
            GameStarted = false;
            GameWon = false;
            GameLost = false;
        }

        public void HandleMatches(Bubble shotColorBubble, Bubble touchedBubble)
        {
            lastShotBubble = shotColorBubble;
            lastTouchedBubble = touchedBubble;
            HandleMatches(shotColorBubble, touchedBubble.Row, touchedBubble.Column);
        }

        public void HandleMatches(Bubble shotColorBubble, int touchedRow, int touchedColumn)
        {
            ScreenLayoutInfo layoutInfo = new()
            {
                TileWidth = tileWidth,
                TileHeight = tileHeight,
                TotalWidth = totalWidth,
                TotalHeight = totalHeight
            };
            List<LevelUtils.EmptyTileInfo> emptyNeighboursInfo = LevelUtils.GetEmptyNeighbours(level, touchedRow, touchedColumn, layoutInfo);

            float minDistance = float.MaxValue;
            int minIndex = -1;
            for (int i = 0; i < emptyNeighboursInfo.Count; i++)
            {
                Vector2 pos = emptyNeighboursInfo[i].Position;
                float distance = Vector2.Distance(shotColorBubble.transform.position, pos);
                if (distance < minDistance && emptyNeighboursInfo[i].Row >= 0)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

            if (minIndex == -1)
                return;

            LevelUtils.EmptyTileInfo tileInfo = emptyNeighboursInfo[minIndex];
            Sequence seq = DOTween.Sequence();
            seq.Append(shotColorBubble.transform.DOMove(tileInfo.Position, 0.05f));
            seq.AppendCallback(() => RunPostShootingLogic(shotColorBubble));
            if (tileInfo.Row >= level.Rows)
            {
                level.AddBottomRow();
            }

            level.Tiles[tileInfo.Row][tileInfo.Column] = shotColorBubble;
            shotColorBubble.Row = tileInfo.Row;
            shotColorBubble.Column = tileInfo.Column;

            const float strength = GameplayConstants.BubbleHitStrength;
            gameScreen.MoveNeighbours(shotColorBubble, shotColorBubble.Row - 2, strength * 0.5f);
            gameScreen.MoveNeighbours(shotColorBubble, shotColorBubble.Row - 1, strength);
            gameScreen.MoveNeighbours(shotColorBubble, shotColorBubble.Row + 1, strength);
            gameScreen.MoveNeighbours(shotColorBubble, shotColorBubble.Row + 2, strength * 0.5f);
            gameScreen.ShakeBubble(shotColorBubble, playerBubbles.LastShotDir, strength).AppendCallback(() =>
            {
                gameScroll.PerformScroll();
            });
        }

        private void RunPostShootingLogic(Bubble shotColorBubble)
        {
            SoundPlayer.PlaySoundFx("Bubble");

            if (!ResolveTouchedBoosterBubbles(shotColorBubble))
            {
                ResolveTouchedClouds(shotColorBubble);

                if (shotColorBubble.GetComponent<ColorBubble>() != null)
                {
                    List<ColorBubble> matches = LevelUtils.GetMatches(level, shotColorBubble.GetComponent<ColorBubble>());
                    if (matches.Count >= GameplayConstants.NumBubblesNeededForMatch)
                    {
                        DestroyTiles(matches.OfType<Bubble>().ToList());
                    }
                    else
                    {
                        CheckGoals();
                    }
                }
            }

            if (shotColorBubble.GetComponent<SpecialBubble>() != null)
            {
                List<Bubble> bubblesToExplode = LevelUtils.GetNeighboursInRadius(level, shotColorBubble, 2);
                DestroyTiles(bubblesToExplode, false, false);

                GameObject fx = fxPool.EnergyBubblePool.GetObject();
                if (didBubbleCollideWithTop)
                    fx.transform.position = shotColorBubble.transform.position;
                else
                    fx.transform.position = lastTouchedBubble.transform.position;
                playerBubbles.OnSpecialBubbleShot();
            }
            else if (shotColorBubble.GetComponent<PurchasableBoosterBubble>() != null)
            {
                PurchasableBoosterBubble purchasableBooster = shotColorBubble.GetComponent<PurchasableBoosterBubble>();

                if (didBubbleCollideWithTop)
                {
                    foreach (Bubble tile in level.Tiles[0])
                        if (tile != null)
                            lastTouchedBubble = tile;
                }

                List<Bubble> bubblesToExplode = purchasableBooster.Resolve(level, shotColorBubble, lastTouchedBubble);
                DestroyTiles(bubblesToExplode, false, false);
                DestroyBubble(shotColorBubble);

                GameObject fx;
                if (shotColorBubble.GetComponent<HorizontalBombBoosterBubble>() != null)
                    fx = fxPool.GetBoosterBubbleParticlePool(BoosterBubbleType.HorizontalBomb).GetObject();
                else
                    fx = fxPool.EnergyBubblePool.GetObject();

                if (didBubbleCollideWithTop)
                    fx.transform.position = shotColorBubble.transform.position;
                else
                    fx.transform.position = lastTouchedBubble.transform.position;

                playerBubbles.OnSpecialBubbleShot();

                didBubbleCollideWithTop = false;
            }
        }

        private bool ResolveTouchedBoosterBubbles(Bubble shotBubble)
        {
            bool resolvedBooster = false;
            List<Bubble> neighbours = LevelUtils.GetNeighbours(level, shotBubble);
            foreach (Bubble bubble in neighbours)
            {
                BoosterBubble boosterBubble = bubble.GetComponent<BoosterBubble>();
                if (boosterBubble != null)
                {
                    List<Bubble> bubblesToExplode = boosterBubble.Resolve(level, shotBubble);
                    DestroyTiles(bubblesToExplode, false, false);
                    resolvedBooster = true;
                }
            }

            if (resolvedBooster)
                DestroyBubble(shotBubble);

            return resolvedBooster;
        }

        private void ResolveTouchedClouds(Bubble shotBubble)
        {
            List<Bubble> neighbours = LevelUtils.GetNeighbours(level, shotBubble);
            foreach (ColorBubble bubble in neighbours.OfType<ColorBubble>())
            {
                if (bubble.CoverType == CoverType.Cloud)
                {
                    RemoveCover(bubble.gameObject);
                }
            }
        }

        private void DestroyTiles(List<Bubble> tiles, bool fall = false, bool transformVoids = true)
        {
            playerBubbles.FillEnergyOrb();

            currentExplodingBubbles.Clear();
            currentExplodingBubbles.AddRange(tiles);

            List<Bubble> bubblesToExplode = new();
            foreach (Bubble bubble in tiles)
            {
                if (fall)
                    DestroyBubbleFalling(bubble);
                else
                    bubblesToExplode.Add(bubble);
            }

            List<Bubble> simulatedBubblesToExplode = SimulatedRingExplodeBubbles(bubblesToExplode);
            RingExplodeBubbles(simulatedBubblesToExplode, transformVoids);

            UpdateAvailableColors();

            if (fall)
            {
                CheckGoals();
                gameScroll.PerformScroll();
            }
        }

        private void UpdateAvailableColors()
        {
            bubbleFactory.PostLevelInitialize(level);
        }

        private void RingExplodeBubbles(List<Bubble> bubbles, bool transformVoids = true)
        {
            int i = 0;
            foreach (Bubble bubble in bubbles)
            {
                Sequence seq = DOTween.Sequence();

                seq.AppendInterval(0.1f * i);
                if (bubble.CanBeDestroyed())
                {
                    ColorBubble colorBubble = bubble.GetComponent<ColorBubble>();
                    if (colorBubble != null && colorBubble.CoverType != CoverType.Ice)
                    {
                        seq.AppendCallback(() =>
                        {
                            bubble.Explode();
                            Animator animator = bubble.transform.GetChild(0).GetComponent<Animator>();
                            if (animator != null && animator.gameObject.activeInHierarchy)
                                animator.SetTrigger("Explode");
                        });
                    }

                    seq.AppendInterval(0.03f);
                    seq.AppendCallback(() => { DestroyBubble(bubble, transformVoids); });
                }

                if (i == bubbles.Count - 1)
                {
                    seq.AppendCallback(RemoveFloatingBubbles);
                    seq.AppendCallback(() => gameScroll.PerformScroll());
                    seq.AppendCallback(() => IsChainingBoosters = false);
                    seq.AppendCallback(() =>
                    {
                        if (shouldChainVoids)
                        {
                            shouldChainVoids = false;
                            IsChainingVoids = true;
                            gameScreen.LockInput();
                            ++voidCounter;
                            StartCoroutine(ChainVoids());
                        }
                    });
                }

                ++i;
            }
        }

        private List<Bubble> SimulatedRingExplodeBubbles(List<Bubble> bubbles)
        {
            List<Bubble> explodedBubbles = new();
            int i = 0;
            while (bubbles.Count > 0)
            {
                List<Bubble> ring = LevelUtils.GetRing(level, playerBubbles.LastShotBubble, i);
                foreach (Bubble bubble in ring)
                {
                    if (bubbles.Contains(bubble))
                    {
                        bubbles.Remove(bubble);
                        explodedBubbles.Add(bubble);
                    }
                }
                ++i;

                if (i >= 20)
                {
                    Debug.Log("This should never happen. Aborting loop.");
                    Debug.Log(bubbles.Count);
                    foreach (Bubble bubble in bubbles)
                        Debug.Log(bubble);
                    break;
                }
            }

            return explodedBubbles;
        }

        private IEnumerator ChainVoids()
        {
            yield return new WaitForSeconds(GameplayConstants.VoidChainSpeed);

            List<ColorBubble> processedBubbles = new();
            foreach (Bubble bubble in newBubbles)
            {
                List<ColorBubble> matches = LevelUtils.GetMatches(level, bubble.GetComponent<ColorBubble>());
                List<ColorBubble> newMatches = new(matches.Count);
                foreach (ColorBubble match in matches)
                    if (!processedBubbles.Contains(match))
                        newMatches.Add(match);

                processedBubbles.AddRange(matches);

                if (newMatches.Count >= GameplayConstants.NumBubblesNeededForMatch)
                    DestroyTiles(newMatches.OfType<Bubble>().ToList());
            }

            newBubbles.Clear();

            yield return new WaitForSeconds(GameplayConstants.VoidChainFinishDelay);
            --voidCounter;
            if (voidCounter == 0)
            {
                IsChainingVoids = false;
                gameScreen.UnlockInput();
            }
        }

        public void DestroyBubble(Bubble bubble, bool transformVoids = true)
        {
            if (bubble != null)
            {
                if (bubble.IsBeingDestroyed)
                    return;

                bubble.IsBeingDestroyed = true;

                ColorBubble colorBubble = bubble.GetComponent<ColorBubble>();

                if (bubble.Row == 0 &&
                    leaves.Count > 0 &&
                    leaves[bubble.Column] != null)
                {
                    if (colorBubble == null ||
                        colorBubble.CoverType == CoverType.None ||
                        colorBubble.CoverType == CoverType.Cloud)
                    {
                        DestroyLeaf(bubble.Column);
                        EventManager.RaiseEvent(new LeavesCollectedEvent(1));
                    }
                }

                if (colorBubble != null)
                {
                    if (colorBubble.CoverType == CoverType.Ice)
                    {
                        RemoveCover(bubble.gameObject);
                        bubble.IsBeingDestroyed = false;
                        return;
                    }

                    CheckForBlockers(bubble);

                    if (colorBubble.CoverType == CoverType.Cloud)
                        RemoveCover(colorBubble.gameObject);

                    if (transformVoids)
                    {
                        List<Bubble> transformedBubbles = TransformVoidBubbles(bubble);
                        foreach (Bubble tbubble in transformedBubbles)
                            if (!newBubbles.Contains(tbubble))
                                newBubbles.Add(tbubble);

                        foreach (Bubble tbubble in transformedBubbles)
                        {
                            List<ColorBubble> matches = LevelUtils.GetMatches(level, tbubble.GetComponent<ColorBubble>());
                            if (matches.Count >= GameplayConstants.NumBubblesNeededForMatch)
                            {
                                shouldChainVoids = true;
                                break;
                            }
                        }
                    }

                    EventManager.RaiseEvent(new BubblesCollectedEvent(colorBubble.Type, 1));

                    SoundPlayer.PlaySoundFx("Explode");
                }

                if (bubble.CanBeDestroyed())
                    bubble.ShowExplosionFx(fxPool);

                if (bubble.GetComponent<BoosterBubble>() != null)
                {
                    List<Bubble> bubblesToExplode = bubble.GetComponent<BoosterBubble>().Resolve(level, lastShotBubble);
                    bubblesToExplode.RemoveAll(x => currentExplodingBubbles.Contains(x));
                    DestroyTiles(bubblesToExplode);
                }

                if (bubble.GetComponent<BombBubble>() != null)
                    SoundPlayer.PlaySoundFx("Bomb");
                else if (bubble.GetComponent<HorizontalBombBubble>() != null)
                    SoundPlayer.PlaySoundFx("BombHorizontal");
                else if (bubble.GetComponent<ColorBombBubble>() != null)
                    SoundPlayer.PlaySoundFx("ColorBomb");

                bubble.GetComponent<PooledObject>().Pool.ReturnObject(bubble.gameObject);
                level.Tiles[bubble.Row][bubble.Column] = null;

                OnBubbleExploded(bubble);
            }
        }

        private void DestroyBubbleFalling(Bubble bubble)
        {
            if (bubble.GetComponent<BlockerBubble>() != null &&
                bubble.GetComponent<BlockerBubble>().Type == BlockerBubbleType.StickyBubble)
            {
                DestroyBubble(bubble);
                SoundPlayer.PlaySoundFx("Sticky");
            }
            else
            {
                level.Tiles[bubble.Row][bubble.Column] = null;
                Falling falling = bubble.GetComponent<Falling>();
                if (falling != null)
                    falling.Fall();

                OnBubbleExploded(bubble);
                ColorBubble colorBubble = bubble.GetComponent<ColorBubble>();
                if (colorBubble != null)
                    EventManager.RaiseEvent(new BubblesCollectedEvent(colorBubble.Type, 1));
            }
        }

        private void OnBubbleExploded(Bubble bubble)
        {
            CollectableBubble collectableBubble = bubble.GetComponent<CollectableBubble>();
            if (collectableBubble != null)
                EventManager.RaiseEvent(new CollectablesCollectedEvent(collectableBubble.Type, 1));

            GameState.Score += gameConfig.DefaultBubbleScore;
            gameUi.UpdateScore(GameState.Score);

            GameObject scoreText = scoreTextPool.GetObject();
            scoreText.transform.position = bubble.transform.position;
            scoreText.GetComponent<ScoreText>().Initialize(gameConfig.DefaultBubbleScore);
        }

        private void CheckForBlockers(Bubble bubble)
        {
            DestroyStones(bubble);
        }

        private void DestroyStones(Bubble bubble)
        {
            List<Bubble> neighbours = new();
            List<BlockerBubble> stonesToDestroy = new();
            List<Bubble> tileNeighbours = LevelUtils.GetNeighbours(level, bubble);
            foreach (Bubble n in tileNeighbours)
            {
                if (!currentExplodingBubbles.Contains(n) && !neighbours.Contains(n))
                    neighbours.Add(n);
            }

            foreach (Bubble n in neighbours)
            {
                BlockerBubble blocker = n.GetComponent<BlockerBubble>();
                if (blocker != null && blocker.Type == BlockerBubbleType.Stone)
                    stonesToDestroy.Add(blocker);
            }

            foreach (BlockerBubble stone in stonesToDestroy)
            {
                DestroyBubble(stone);
                SoundPlayer.PlaySoundFx("Stone");
            }
        }

        private List<Bubble> TransformVoidBubbles(Bubble bubble)
        {
            List<Bubble> retBubbles = new();

            List<BlockerBubble> adjacentVoidBubbles = new();
            IEnumerable<BlockerBubble> tileNeighbours = LevelUtils.GetNeighbours(level, bubble).OfType<BlockerBubble>();
            foreach (BlockerBubble n in tileNeighbours)
            {
                if (!adjacentVoidBubbles.Contains(n) &&
                    n.GetComponent<BlockerBubble>().Type == BlockerBubbleType.VoidBubble &&
                    !currentExplodingBubbles.Contains(n))
                    adjacentVoidBubbles.Add(n);
            }

            foreach (BlockerBubble voidBubble in adjacentVoidBubbles)
            {
                GameObject newBubble = bubbleFactory.CreateColorBubble(bubble.GetComponent<ColorBubble>().Type);
                newBubble.GetComponent<Bubble>().Row = voidBubble.Row;
                newBubble.GetComponent<Bubble>().Column = voidBubble.Column;
                newBubble.GetComponent<Bubble>().GameLogic = this;
                newBubble.transform.position = voidBubble.transform.position;
                retBubbles.Add(newBubble.GetComponent<Bubble>());
                level.Tiles[voidBubble.Row][voidBubble.Column] = newBubble.GetComponent<Bubble>();
            }

            foreach (BlockerBubble voidBubble in adjacentVoidBubbles)
            {
                voidBubble.GetComponent<PooledObject>().Pool.ReturnObject(voidBubble.gameObject);
                voidBubble.ShowExplosionFx(fxPool);
                SoundPlayer.PlaySoundFx("Void");
            }

            return retBubbles;
        }

        private void DestroyLeaf(int column)
        {
            if (leaves[column] != null)
            {
                leaves[column].GetComponent<Animator>().SetTrigger("Release");
                leaves[column].GetComponent<Leaf>().Destroy();
                leaves[column] = null;
                SoundPlayer.PlaySoundFx("Leaf");
            }
        }

        private void RemoveFloatingBubbles()
        {
            StartCoroutine(RemoveFloatingBubblesCoroutine());
        }

        private IEnumerator RemoveFloatingBubblesCoroutine()
        {
            List<List<Bubble>> floatingIslands = LevelUtils.FindFloatingIslands(level);
            List<Bubble> tilesToRemove = new();
            foreach (List<Bubble> island in floatingIslands)
            {
                bool isSticky = island.Count >= 2 && island.Find(x =>
                                   x.GetComponent<BlockerBubble>() != null &&
                                   x.GetComponent<BlockerBubble>().Type == BlockerBubbleType.StickyBubble);
                if (!isSticky)
                {
                    foreach (Bubble tile in island)
                    {
                        tilesToRemove.Add(tile);
                    }
                }
            }

            foreach (Bubble bubble in tilesToRemove)
            {
                BlockerBubble blocker = bubble.GetComponent<BlockerBubble>();
                if (blocker != null && blocker.Type != BlockerBubbleType.IronBubble)
                {
                    blocker.ShowExplosionFx(fxPool);
                }
                else
                {
                    if (bubble.GetComponent<ColorBubble>() != null)
                    {
                        Animator animator = bubble.GetComponentInChildren<Animator>();
                        if (animator != null && animator.gameObject.activeInHierarchy)
                            animator.SetTrigger("Falling");
                    }
                }
            }

            if (tilesToRemove.Count > 0)
            {
                yield return new WaitForSeconds(GameplayConstants.FloatingIslandsRemovalDelay);
                DestroyTiles(tilesToRemove, true);
            }
            else
            {
                yield return null;
                CheckGoals();
            }
        }

        public void HandleTopRowMatches(Bubble bubble)
        {
            bubble.ForceStop();
            didBubbleCollideWithTop = true;

            int column = 0;
            float minDist = Mathf.Infinity;
            for (int i = 0; i < level.Tiles[0].Count; i++)
            {
                Vector2 tilePos = tilePositions[0][i];
                Vector2 newPos = tilePos;
                float newDist = Vector2.Distance(bubble.transform.position, newPos);
                if (newDist <= minDist)
                {
                    minDist = newDist;
                    column = i;
                }
            }

            HandleMatches(bubble, 0, column);
        }

        private void RemoveCover(GameObject bubble)
        {
            ColorBubble colorBubble = bubble.GetComponent<ColorBubble>();
            CoverType coverType = colorBubble.CoverType;
            Vector3 pos = colorBubble.transform.position;
            colorBubble.CoverType = CoverType.None;

            if (coverType == CoverType.Ice)
                SoundPlayer.PlaySoundFx("Ice");
            else if (coverType == CoverType.Cloud)
                SoundPlayer.PlaySoundFx("Cloud");

            GameObject cover = bubble.transform.GetChild(1).gameObject;
            Sequence seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                Animator animator = cover.GetComponent<Animator>();
                if (animator != null && cover.activeInHierarchy)
                    cover.GetComponent<Animator>().SetTrigger("Explode");
            });
            seq.AppendInterval(0.1f);
            seq.AppendCallback(() =>
            {
                GameObject fx = fxPool.GetCoverParticlePool(coverType).GetObject();
                fx.transform.position = pos;
                cover.GetComponent<PooledObject>().Pool.ReturnObject(cover);
            });
        }

        private void CheckGoals()
        {
            if (GameWon || GameLost)
                return;

            bool allGoalsCompleted = true;
            foreach (LevelGoal goal in levelInfo.Goals)
            {
                if (!goal.IsComplete(GameState))
                {
                    allGoalsCompleted = false;
                    break;
                }
            }

            if (allGoalsCompleted && !GameWon)
            {
                GameWon = true;
                EndGame();

                int nextLevel = UserManager.CurrentUser.GetNextLevelIndex() + 1;
                LevelManager.unlockedNextLevel = levelInfo.Number == nextLevel;

                UserManager.TrySaveUserData();

                if (playerBubbles.NumBubblesLeft > 1)
                {
                    gameScreen.OpenLevelCompletedAnimation();
                    playerBubbles.PlayEndOfGameSequence();
                }
                else
                {
                    gameScreen.StartCoroutine(gameScreen.OpenWinPopupAsync());
                }
            }

            if (!allGoalsCompleted && playerBubbles.NumBubblesLeft <= 1 && !playerBubbles.HasBubblesLeftToShoot)
            {
                GameLost = true;
                EndGame();

                gameScreen.StartCoroutine(gameScreen.OpenOutOfBubblesPopupAsync());
            }
        }

        public void StartGame()
        {
            GameStarted = true;
        }

        public void EndGame()
        {
            GameStarted = false;
            playerBubbles.OnGameEnded();
        }

        public void RestartGame()
        {
            gameScreen.OnGameRestarted();
            gameScreen.InitializeLevel();
            StartGame();
        }

        public void ContinueGame()
        {
            GameStarted = true;
            GameWon = false;
            GameLost = false;
            gameScreen.OnGameContinued();
            playerBubbles.OnGameContinued();
        }
    }
}
