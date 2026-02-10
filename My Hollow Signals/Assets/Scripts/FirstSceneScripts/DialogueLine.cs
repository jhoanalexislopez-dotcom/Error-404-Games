/*******************************************************
 * Author: [Alejandro Vila]
 * Updated by: [Ignacio Lopez]
 * Last Modified: [25/01/2026]
 * Description:
 *   A simple serializable data class that represents a single line of dialogue in the dialogue system. It contains a LocalizedString for the text of the dialogue line, allowing for easy localization and integration with Unity's localization system. This class can be used to create dialogue sequences by storing multiple DialogueLine instances in a list or array, which can then be processed by a dialogue manager to display the text to the player.
 *******************************************************/


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[System.Serializable]
public class DialogueLine
{
    public LocalizedString text;
}
