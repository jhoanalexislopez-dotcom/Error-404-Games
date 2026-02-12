/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [25/01/2026]
 * Description:
 *    Interface definition for all interactable objects in the game. Any object that the player can interact with (like collectibles, doors, switches, etc.) should implement this interface. It includes a method for interaction logic and a method to get a localized description of the object for UI display.
 *******************************************************/

using System.Collections.Generic;
using UnityEngine.Localization;

public interface IInteractable
{
    void Interact();
    LocalizedString GetLocalizedDescription();
    bool CanInteract();
}
