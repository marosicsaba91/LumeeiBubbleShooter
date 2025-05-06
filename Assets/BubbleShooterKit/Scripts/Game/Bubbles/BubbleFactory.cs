// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooterKit
{
    /// <summary>
    /// Utility class used to instantiate new bubbles according to the
    /// level's configuration.
    /// </summary>
    ///

    public class BubbleFactory : MonoBehaviour
    {
        public GameLogic GameLogic;
        public BubblePool BubblePool;

        public readonly List<ObjectPool> RandomizedColorBubblePrefabs = new();
        public readonly List<ObjectPool> ShootingColorBubblePrefabs = new();

        public bool poolsInitialized;

        public void PreLevelInitialize(LevelInfo levelInfo)
        {
            if (LevelManager.availableColors.Count == 0)
            {
                LevelManager.availableColors.AddRange(levelInfo.AvailableColors);
                LevelManager.availableColors.Shuffle();
            }

            foreach (ColorBubbleType color in LevelManager.availableColors)
                RandomizedColorBubblePrefabs.Add(BubblePool.GetColorBubblePool(color));

            if (!poolsInitialized)
            {
                poolsInitialized = true;
                foreach (ObjectPool pool in RandomizedColorBubblePrefabs)
                    pool.Initialize();
                foreach (ObjectPool pool in ShootingColorBubblePrefabs)
                    pool.Initialize();
            }
        }

        public void PostLevelInitialize(Level level)
        {
            ShootingColorBubblePrefabs.Clear();

            List<ColorBubbleType> colors = new();
            foreach (List<Bubble> row in level.Tiles)
            {
                foreach (Bubble bubble in row)
                {
                    if (bubble != null && bubble.TryGetComponent<ColorBubble>(out var colorBubble) && !colors.Contains(colorBubble.Type))
                        colors.Add(colorBubble.Type);
                }
            }

            foreach (ColorBubbleType color in colors)
                ShootingColorBubblePrefabs.Add(BubblePool.GetColorBubblePool(color));
        }

        public void Reset()
        {
            RandomizedColorBubblePrefabs.Clear();
            ShootingColorBubblePrefabs.Clear();
        }

        public void ResetAvailableShootingBubbles(LevelInfo levelInfo)
        {
            ShootingColorBubblePrefabs.Clear();

            foreach (ColorBubbleType color in levelInfo.AvailableColors)
                ShootingColorBubblePrefabs.Add(BubblePool.GetColorBubblePool(color));
        }

        public GameObject CreateRandomColorBubble()
        {
            int idx = Random.Range(0, ShootingColorBubblePrefabs.Count);
            GameObject bubble = ShootingColorBubblePrefabs[idx].GetObject();
            bubble.GetComponent<Bubble>().GameLogic = GameLogic;
            return bubble;
        }

        public GameObject CreateBubble(TileInfo tile)
        {
            BubbleTileInfo bubbleTile = tile as BubbleTileInfo;
            if (bubbleTile != null)
            {
                GameObject bubble = BubblePool.GetColorBubblePool(bubbleTile.Type).GetObject();
                bubble.GetComponent<Bubble>().GameLogic = GameLogic;
                bubble.GetComponent<ColorBubble>().CoverType = bubbleTile.CoverType;
                if (bubbleTile.CoverType != CoverType.None)
                    AddCover(bubble, bubbleTile.CoverType);
                return bubble;
            }

            RandomBubbleTileInfo randomBubbleTile = tile as RandomBubbleTileInfo;
            if (randomBubbleTile != null)
            {
                GameObject bubble = RandomizedColorBubblePrefabs[(int)randomBubbleTile.Type % RandomizedColorBubblePrefabs.Count].GetObject();
                bubble.GetComponent<Bubble>().GameLogic = GameLogic;
                bubble.GetComponent<ColorBubble>().CoverType = randomBubbleTile.CoverType;
                if (randomBubbleTile.CoverType != CoverType.None)
                    AddCover(bubble, randomBubbleTile.CoverType);
                return bubble;
            }

            BlockerTileInfo blockerTile = tile as BlockerTileInfo;
            if (blockerTile != null)
            {
                GameObject blocker = BubblePool.GetBlockerBubblePool(blockerTile.BubbleType).GetObject();
                blocker.GetComponent<Bubble>().GameLogic = GameLogic;
                return blocker;
            }

            BoosterTileInfo boosterTile = tile as BoosterTileInfo;
            if (boosterTile != null)
            {
                GameObject booster = BubblePool.GetBoosterBubblePool(boosterTile.BubbleType).GetObject();
                booster.GetComponent<BoosterBubble>().GameLogic = GameLogic;
                return booster;
            }

            CollectableTileInfo collectableTile = tile as CollectableTileInfo;
            if (collectableTile != null)
            {
                GameObject collectable = BubblePool.GetCollectableBubblePool(collectableTile.Type).GetObject();
                collectable.GetComponent<CollectableBubble>().GameLogic = GameLogic;
                return collectable;
            }

            return null;
        }

        public GameObject CreateColorBubble(ColorBubbleType type)
        {
            GameObject bubble = BubblePool.GetColorBubblePool(type).GetObject();
            bubble.GetComponent<Bubble>().GameLogic = GameLogic;
            return bubble;
        }

        private void AddCover(GameObject bubble, CoverType type)
        {
            bubble.GetComponent<ColorBubble>().CoverType = type;
            GameObject cover = BubblePool.GetCoverPool(type).GetObject();
            cover.transform.parent = bubble.transform;
            cover.transform.localPosition = Vector3.zero;
        }
    }
}
