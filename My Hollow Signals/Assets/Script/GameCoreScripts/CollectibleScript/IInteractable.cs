/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Interface definition for all interactable objects in the game.
 *******************************************************/

using System.Collections.Generic;

public interface IInteractable
{
    void Interact();
    string GetDescription();
}
