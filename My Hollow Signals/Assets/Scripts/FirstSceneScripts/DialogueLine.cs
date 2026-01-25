/*******************************************************
 * Author: [Alejandro Vila]
 * Last Modified: [21/11/2025]
 * Description:
 *   A simple serializable data class that represents a single line of dialogue in the dialogue system.
 *******************************************************/


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Localization;

[System.Serializable]
public class DialogueLine
{
    public LocalizedString text;
}
