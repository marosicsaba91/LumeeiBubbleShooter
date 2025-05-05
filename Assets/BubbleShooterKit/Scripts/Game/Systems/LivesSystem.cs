// Copyright (C) 2018 gamevanilla. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

using UnityEngine;

namespace BubbleShooterKit
{
    /// <summary>
    /// This class handles the lives system in the game. It is used to add and remove lives and other classes
    /// can subscribe to it in order to receive a notification when the number of lives changes.
    /// </summary>
	[CreateAssetMenu(fileName = "LivesSystem", menuName = "Bubble Shooter Kit/Systems/Lives system", order = 0)]
	public class LivesSystem : ScriptableObject
    {
	    public void AddLife(GameConfiguration gameConfig)
	    {
		    var numLives = UserManager.CurrentUser.lives;
		    numLives += 1;
		    if (numLives > gameConfig.MaxLives)
			    numLives = gameConfig.MaxLives;
            UserManager.CurrentUser.lives=numLives;
			UserManager.SaveUserData(); 
        }

	    public void RemoveLife(GameConfiguration gameConfig)
	    {
		    var numLives = UserManager.CurrentUser.lives;
		    numLives -= 1;
		    if (numLives < 0)
			    numLives = 0; 
            UserManager.CurrentUser.lives = numLives;
            UserManager.SaveUserData();
        }
	    
	    public void Refill(GameConfiguration gameConfig)
	    {
		    var amount = gameConfig.MaxLives;
            var numLives = UserManager.CurrentUser.lives;
			UserManager.SaveUserData();
        }
	}
}
