/*******************************************************
 * Author: [Bianca Marinica]
 * Last Modified: [21/11/2025]
 * Description:
 *    Interface definition for all interactable objects in the game.
 *******************************************************/

using System.Collections.Generic;
using UnityEngine.Localization;

public interface IInteractable
{
    void Interact();
    LocalizedString GetLocalizedDescription();
}
