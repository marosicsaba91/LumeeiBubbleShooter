// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BubbleShooterKit
{
	/// <summary>
	/// Miscellaneous level utilities.
	/// </summary>
	public static class LevelUtils
	{
		public struct EmptyTileInfo
		{
			public int Row;
			public int Column;
			public Vector2 Position;
		}

		public static List<Bubble> GetNeighbours(Level level, Bubble bubble)
		{
            List<Bubble> neighbours = new();

			if (bubble.Row % 2 == 0)
			{
                Bubble topLeft = level.GetTile(bubble.Row - 1, bubble.Column - 1);
                Bubble topRight = level.GetTile(bubble.Row - 1, bubble.Column);
                Bubble left = level.GetTile(bubble.Row, bubble.Column - 1);
                Bubble right = level.GetTile(bubble.Row, bubble.Column + 1);
                Bubble bottomLeft = level.GetTile(bubble.Row + 1, bubble.Column - 1);
                Bubble bottomRight = level.GetTile(bubble.Row + 1, bubble.Column);
				if (topLeft != null) neighbours.Add(topLeft);
				if (topRight != null) neighbours.Add(topRight);
				if (left != null) neighbours.Add(left);
				if (right != null) neighbours.Add(right);
				if (bottomLeft != null)	neighbours.Add(bottomLeft);
				if (bottomRight != null) neighbours.Add(bottomRight);
			}
			else
			{
                Bubble topLeft = level.GetTile(bubble.Row - 1, bubble.Column);
                Bubble topRight = level.GetTile(bubble.Row - 1, bubble.Column + 1);
                Bubble left = level.GetTile(bubble.Row, bubble.Column - 1);
                Bubble right = level.GetTile(bubble.Row, bubble.Column + 1);
                Bubble bottomLeft = level.GetTile(bubble.Row + 1, bubble.Column);
                Bubble bottomRight = level.GetTile(bubble.Row + 1, bubble.Column + 1);
				if (topLeft != null) neighbours.Add(topLeft);
				if (topRight != null) neighbours.Add(topRight);
				if (left != null) neighbours.Add(left);
				if (right != null) neighbours.Add(right);
				if (bottomLeft != null)	neighbours.Add(bottomLeft);
				if (bottomRight != null) neighbours.Add(bottomRight);
			}

			return neighbours;
		}

		public static List<Bubble> GetNeighboursInRadius(Level level, Bubble bubble, int radius)
		{
            List<Bubble> neighbours = new();

			neighbours.AddRange(GetNeighboursInRadius(level, bubble));
			--radius;
			while (radius > 0)
			{
                List<Bubble> newNeighbours = new();
				foreach (Bubble neighbour in neighbours)
				{
					newNeighbours.AddRange(GetNeighboursInRadius(level, neighbour));
				}

				foreach (Bubble neighbour in newNeighbours)
				{
					if (!neighbours.Contains(neighbour))
						neighbours.Add(neighbour);
				}
				
				--radius;
			}
			
			return neighbours;
		}
		
		public static List<Bubble> GetNeighboursInRadius(Level level, Bubble bubble)
		{
            List<Bubble> neighbours = new();

            int row = bubble.Row;
            int column = bubble.Column;
			if (row % 2 == 0)
			{
				AddNeighbour(level, neighbours, row, column);
				AddNeighbour(level, neighbours, row - 1, column - 1);
				AddNeighbour(level, neighbours, row - 1, column);
				AddNeighbour(level, neighbours, row, column - 1);
				AddNeighbour(level, neighbours, row, column + 1);
				AddNeighbour(level, neighbours, row + 1, column - 1);
				AddNeighbour(level, neighbours, row + 1, column);
			}
			else
			{
				AddNeighbour(level, neighbours, row, column);
				AddNeighbour(level, neighbours, row - 1, column);
				AddNeighbour(level, neighbours, row - 1, column + 1);
				AddNeighbour(level, neighbours, row, column - 1);
				AddNeighbour(level, neighbours, row, column + 1);
				AddNeighbour(level, neighbours, row + 1, column);
				AddNeighbour(level, neighbours, row + 1, column + 1);
			}

			return neighbours;
		}
		
		public static List<Bubble> GetRing(Level level, Bubble bubble, int radius)
		{
            List<Bubble> neighbours = new();

			if (radius == 0)
			{
				neighbours.Add(bubble);
				return neighbours;
			}

            int row = bubble.Row;
            int column = bubble.Column;
			
			if (row % 2 == 0)
			{
                int i = 1;
                int j = 0;
                int k = 0;
				while (i <= radius)
				{
                    Bubble leftTop = level.GetTile(row - i, column - radius + j);
                    Bubble rightTop = level.GetTile(row - i, column + j + radius - k - 1);

                    Bubble leftBottom = level.GetTile(row + i, column - radius + j);
                    Bubble rightBottom = level.GetTile(row + i, column + j + radius - k - 1);

					if (leftTop != null) neighbours.Add(leftTop);
					if (rightTop != null) neighbours.Add(rightTop);
					if (leftBottom != null) neighbours.Add(leftBottom);
					if (rightBottom != null) neighbours.Add(rightBottom);

					if (i == radius)
					{
                        int c = column - radius + j + 1;
						while (c < (column + j + radius - k - 1))
						{
                            Bubble tile = level.GetTile(row + i, c);
							if (tile != null) neighbours.Add(tile);

							tile = level.GetTile(row - i, c);
							if (tile != null) neighbours.Add(tile);
							++c;
						}
					}

					++i;
					if (i % 2 == 0)
						++j;
					++k;
				}
			}
			else
			{
                int i = 1;
                int j = 1;
                int k = 0;
				while (i <= radius)
				{
                    Bubble leftTop = level.GetTile(row - i, column - radius + j);
                    Bubble rightTop = level.GetTile(row - i, column + radius + j - k - 1);

                    Bubble leftBottom = level.GetTile(row + i, column - radius + j);
                    Bubble rightBottom = level.GetTile(row + i, column + radius + j - k - 1);

					if (leftTop != null) neighbours.Add(leftTop);
					if (rightTop != null) neighbours.Add(rightTop);
					if (leftBottom != null) neighbours.Add(leftBottom);
					if (rightBottom != null) neighbours.Add(rightBottom);
					
					if (i == radius)
					{
                        int c = column - radius + j + 1;
						while (c < (column + j + radius - k - 1))
						{
                            Bubble tile = level.GetTile(row + i, c);
							if (tile != null) neighbours.Add(tile);

							tile = level.GetTile(row - i, c);
							if (tile != null) neighbours.Add(tile);
							++c;
						}
					}
						
					++i;
					if (i % 2 != 0)
						++j;
					++k;
				}
			}

            Bubble left = level.GetTile(row, column - radius);
            Bubble right = level.GetTile(row, column + radius);
			if (left != null) neighbours.Add(left);
			if (right != null) neighbours.Add(right);

			return neighbours;
		}

		private static void AddNeighbour(Level level, List<Bubble> neighbours, int row, int column)
		{
            Bubble neighbour = level.GetTile(row, column);
			if (neighbour != null && !neighbours.Contains(neighbour))
				neighbours.Add(neighbour);
		}
		
		public static List<ColorBubble> GetMatches(Level level, ColorBubble colorBubble)
		{
            List<ColorBubble> matches = new();
			GetMatchesRecursive(level, colorBubble, matches);
			if (!matches.Contains(colorBubble))
			{
				matches.Add(colorBubble);
			}

			return matches;
		}

		private static void GetMatchesRecursive(Level level, ColorBubble colorBubble, List<ColorBubble> matchedBubbles)
		{
            IEnumerable<ColorBubble> neighbours = GetNeighbours(level, colorBubble).OfType<ColorBubble>();

            bool hasMatch = false;
            ColorBubble[] enumerable = neighbours as ColorBubble[] ?? neighbours.ToArray();
			foreach (ColorBubble neighbour in enumerable)
			{
				if (neighbour.Type == colorBubble.Type)
				{
					hasMatch = true;
				}
			}
			
			if (!hasMatch)
			{
				return;
			}

			if (!matchedBubbles.Contains(colorBubble))
			{
				matchedBubbles.Add(colorBubble);
			}
			
			foreach (ColorBubble neighbour in enumerable)
			{
				if (neighbour.Type == colorBubble.Type &&
				    !matchedBubbles.Contains(neighbour))
				{
					GetMatchesRecursive(level, neighbour, matchedBubbles);
				}
			}
		}

		private static List<Bubble> FindIsland(Level level, Bubble bubble, List<Bubble> processed)
		{
            Stack<Bubble> toProcess = new();
			toProcess.Push(bubble);

			processed.Add(bubble);

            List<Bubble> foundIsland = new();

			while (toProcess.Count > 0)
			{
                Bubble processedBubble = toProcess.Pop();

				if (processedBubble == null)
				{
					continue;
				}
				
				foundIsland.Add(processedBubble);

                List<Bubble> neighbours = GetNeighbours(level, processedBubble);
				foreach (Bubble neighbour in neighbours)
				{
					if (!processed.Contains(neighbour))
					{
						toProcess.Push(neighbour);
						processed.Add(neighbour);
					}
				}
			}

			return foundIsland;
		}
		
		public static List<List<Bubble>> FindFloatingIslands(Level level)
		{
            List<List<Bubble>> foundIslands = new();
            List<Bubble> processed = new();

			foreach (List<Bubble> row in level.Tiles)
			{
				foreach (Bubble tile in row)
				{
					if (!processed.Contains(tile))
					{
                        List<Bubble> foundCluster = FindIsland(level, tile, processed);

						if (foundCluster.Count <= 0)
						{
							continue;
						}

                        bool floating = true;
						foreach (Bubble b in foundCluster)
						{
							if (b.Row == 0)
							{
								floating = false;
								break;
							}
						}

						if (floating)
						{
							foundIslands.Add(foundCluster);
						}
					}
				}
			}

			return foundIslands;
		}

		public static List<EmptyTileInfo> GetEmptyNeighbours(Level level, int row, int column, ScreenLayoutInfo layoutInfo)
		{
            List<EmptyTileInfo> emptyNeighboursInfo = new();
			
			if (row % 2 == 0)
			{
                Bubble self = level.GetTile(row, column);
                Bubble topLeft = level.GetTile(row - 1, column - 1);
                Bubble topRight = level.GetTile(row - 1, column);
                Bubble left = level.GetTile(row, column - 1);
                Bubble right = level.GetTile(row, column + 1);
                Bubble bottomLeft = level.GetTile(row + 1, column - 1);
                Bubble bottomRight = level.GetTile(row + 1, column);
				if (self == null && level.IsValidTile(row, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column));
				}
				if (topLeft == null && level.IsValidTile(row - 1, column - 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row - 1, column - 1));
				}
				if (topRight == null && level.IsValidTile(row - 1, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row - 1, column));
				}
				if (left == null && level.IsValidTile(row, column - 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column - 1));
				}
				if (right == null && level.IsValidTile(row, column + 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column + 1));
				}
				if (bottomLeft == null && level.IsValidTile(row + 1, column - 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row + 1, column - 1));
				}
				if (bottomRight == null && level.IsValidTile(row + 1, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row + 1, column));
				}
			}
			else
			{
                Bubble self = level.GetTile(row, column);
                Bubble topLeft = level.GetTile(row - 1, column);
                Bubble topRight = level.GetTile(row - 1, column + 1);
                Bubble left = level.GetTile(row, column - 1);
                Bubble right = level.GetTile(row, column + 1);
                Bubble bottomLeft = level.GetTile(row + 1, column);
                Bubble bottomRight = level.GetTile(row + 1, column + 1);
				if (self == null && level.IsValidTile(row, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column));
				}
				if (topLeft == null && level.IsValidTile(row - 1, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row - 1, column));
				}
				if (topRight == null && level.IsValidTile(row - 1, column + 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row - 1, column + 1));
				}
				if (left == null && level.IsValidTile(row, column - 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column - 1));
				}
				if (right == null && level.IsValidTile(row, column + 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row, column + 1));
				}
				if (bottomLeft == null && level.IsValidTile(row + 1, column))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row + 1, column));
				}
				if (bottomRight == null && level.IsValidTile(row + 1, column + 1))
				{
					emptyNeighboursInfo.Add(GenerateEmptyTileInfo(layoutInfo, row + 1, column + 1));
				}
			}

			return emptyNeighboursInfo;
		}

		private static EmptyTileInfo GenerateEmptyTileInfo(ScreenLayoutInfo layoutInfo, int row, int column)
		{
			return new EmptyTileInfo
			{
				Row = row,
				Column = column,
				Position = CalculatePosition(layoutInfo, row, column)
			};	
		}
		
		private static Vector2 CalculatePosition(ScreenLayoutInfo layoutInfo, int row, int column)
		{
			float rowOffset;
			if (row % 2 == 0)
			{
				rowOffset = 0;
			}
			else
			{
				rowOffset = layoutInfo.TileWidth * 0.5f;
			}

            Vector2 bottomPivot = new(0, Camera.main.pixelHeight * GameplayConstants.BottomPivotHeight);
            Vector3 bottomPivotPos = Camera.main.ScreenToWorldPoint(bottomPivot);

            Vector2 pos = new(
				(column * layoutInfo.TileWidth * GameplayConstants.TileWidthMultiplier) + rowOffset,
				-row * layoutInfo.TileHeight * GameplayConstants.TileHeightMultiplier);
            Vector2 newPos = pos;
			newPos.x -= layoutInfo.TotalWidth / 2f;
			newPos.x += (layoutInfo.TileWidth * GameplayConstants.TileWidthMultiplier) / 2f;
			newPos.y += bottomPivotPos.y + layoutInfo.TotalHeight;
			newPos.y += LevelManager.scrolledHeight;

            return newPos;
		}
	}
}
