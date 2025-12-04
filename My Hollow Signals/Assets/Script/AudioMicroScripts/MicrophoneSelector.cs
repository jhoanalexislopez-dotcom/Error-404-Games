using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class MicrophoneSelector : MonoBehaviour
{
    public TMP_Dropdown sourceDropdown;
    public int chosenDeviceIndex = 0;

    public static UnityAction<int> OnMicrophoneChoiceChanged;
    void Start()
    {
        PopulateSourceDropDown();
    }

    private void PopulateSourceDropDown()
    {
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (var microphone in Microphone.devices)
        {
            TMP_Dropdown.OptionData optiondata = new TMP_Dropdown.OptionData(microphone, null);

            options.Add(optiondata);
        }

        sourceDropdown.options = options;
    }

    public void ChooseMicrophone(int optionIndex)
    {
        chosenDeviceIndex = optionIndex;
        OnMicrophoneChoiceChanged?.Invoke(chosenDeviceIndex);
    }
    
}
