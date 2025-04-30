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
    public class BubbleFactory : MonoBehaviour
    {
	    public GameLogic GameLogic;
	    public BubblePool BubblePool;
	    
		public readonly List<ObjectPool> RandomizedColorBubblePrefabs = new List<ObjectPool>();
		public readonly List<ObjectPool> ShootingColorBubblePrefabs = new List<ObjectPool>();

		public bool poolsInitialized;

	    public void PreLevelInitialize(LevelInfo levelInfo)
	    {
		    if (!PlayerPrefs.HasKey("num_available_colors"))
		    {
			    var randomColors = levelInfo.AvailableColors;
			    randomColors.Shuffle();
			    PlayerPrefs.SetInt("num_available_colors", randomColors.Count);
			    for (var i = 0; i < randomColors.Count; i++)
				    PlayerPrefs.SetInt($"available_colors_{i}", (int)randomColors[i]);
		    }

		    var numColors = PlayerPrefs.GetInt("num_available_colors");
		    var availableColors = new List<ColorBubbleType>();
		    for (var i = 0; i < numColors; i++)
			    availableColors.Add((ColorBubbleType) PlayerPrefs.GetInt($"available_colors_{i}"));
		    foreach (var color in availableColors)
			    RandomizedColorBubblePrefabs.Add(BubblePool.GetColorBubblePool(color));

		    if (!poolsInitialized)
		    {
			    poolsInitialized = true;
			    foreach (var pool in RandomizedColorBubblePrefabs)
				    pool.Initialize();
			    foreach (var pool in ShootingColorBubblePrefabs)
				    pool.Initialize();
		    }
	    }
	    
	    public void PostLevelInitialize(Level level)
	    {
		    ShootingColorBubblePrefabs.Clear();
		    
		    var colors = new List<ColorBubbleType>();
		    foreach (var row in level.Tiles)
		    {
			    foreach (var bubble in row)
			    {
				    if (bubble != null)
				    {
					    var colorBubble = bubble.GetComponent<ColorBubble>();
					    if (colorBubble != null)
					    {
						    if (!colors.Contains(colorBubble.Type))
							    colors.Add(colorBubble.Type);
					    }
				    }
			    }
		    }
		    
		    foreach (var color in colors)
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

		    foreach (var color in levelInfo.AvailableColors)
			    ShootingColorBubblePrefabs.Add(BubblePool.GetColorBubblePool(color));
	    }

		public GameObject CreateRandomColorBubble()
		{
			var idx = Random.Range(0, ShootingColorBubblePrefabs.Count);
			var bubble = ShootingColorBubblePrefabs[idx].GetObject();
			bubble.GetComponent<Bubble>().GameLogic = GameLogic;
			return bubble;
		}
		
		public GameObject CreateBubble(TileInfo tile)
		{
			var bubbleTile = tile as BubbleTileInfo;
			if (bubbleTile != null)
			{
				var bubble = BubblePool.GetColorBubblePool(bubbleTile.Type).GetObject();
				bubble.GetComponent<Bubble>().GameLogic = GameLogic;
				bubble.GetComponent<ColorBubble>().CoverType = bubbleTile.CoverType;
				if (bubbleTile.CoverType != CoverType.None)
					AddCover(bubble, bubbleTile.CoverType);	
				return bubble;
			}

			var randomBubbleTile = tile as RandomBubbleTileInfo;
			if (randomBubbleTile != null)
			{
				var bubble = RandomizedColorBubblePrefabs[(int)randomBubbleTile.Type % RandomizedColorBubblePrefabs.Count].GetObject();
				bubble.GetComponent<Bubble>().GameLogic = GameLogic;
				bubble.GetComponent<ColorBubble>().CoverType = randomBubbleTile.CoverType;
				if (randomBubbleTile.CoverType != CoverType.None)
					AddCover(bubble, randomBubbleTile.CoverType);	
				return bubble;
			}

			var blockerTile = tile as BlockerTileInfo;
			if (blockerTile != null)
			{
				var blocker = BubblePool.GetBlockerBubblePool(blockerTile.BubbleType).GetObject();
				blocker.GetComponent<Bubble>().GameLogic = GameLogic;
				return blocker;
			}

			var boosterTile = tile as BoosterTileInfo;
			if (boosterTile != null)
			{
				var booster = BubblePool.GetBoosterBubblePool(boosterTile.BubbleType).GetObject();
				booster.GetComponent<BoosterBubble>().GameLogic = GameLogic;
				return booster;
			}

			var collectableTile = tile as CollectableTileInfo;
			if (collectableTile != null)
			{
				var collectable = BubblePool.GetCollectableBubblePool(collectableTile.Type).GetObject();
				collectable.GetComponent<CollectableBubble>().GameLogic = GameLogic;
				return collectable;
			}

			return null;
		}

		public GameObject CreateColorBubble(ColorBubbleType type)
		{
			var bubble = BubblePool.GetColorBubblePool(type).GetObject();
			bubble.GetComponent<Bubble>().GameLogic = GameLogic;
			return bubble;
		}
	    
		private void AddCover(GameObject bubble, CoverType type)
		{
			bubble.GetComponent<ColorBubble>().CoverType = type;
			var cover = BubblePool.GetCoverPool(type).GetObject();
			cover.transform.parent = bubble.transform;
			cover.transform.localPosition = Vector3.zero;
		}
    }
}
