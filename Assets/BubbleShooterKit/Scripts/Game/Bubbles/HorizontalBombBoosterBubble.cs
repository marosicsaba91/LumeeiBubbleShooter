// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using System.Collections.Generic;

namespace BubbleShooterKit
{
	/// <summary>
	/// The type associated to horizontal bomb bubbles.
	/// </summary>
	public class HorizontalBombBoosterBubble : PurchasableBoosterBubble
	{
        public override List<Bubble> Resolve(Level level, Bubble shotBubble, Bubble touchedBubble)
        {
            List<Bubble> bubblesToExplode = new();
	        
	        for (int i = 0; i < level.Tiles[touchedBubble.Row].Count; i++)
	        {
                Bubble bubble = level.Tiles[touchedBubble.Row][i];
		        if (bubble != null)
			        bubblesToExplode.Add(bubble);
	        }

	        return bubblesToExplode;
        }
	}
}
