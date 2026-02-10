/*******************************************************
 * Author: [Ignacio Lopez]
 * Last Modified: [16/01/2026]
 * Description:
 *    This script manages the in-game messaging system, allowing for the display of messages from the player and contacts, as well as timestamps. It provides methods to add new messages and timestamps to the UI, and automatically scrolls to the bottom of the message list when new content is added. The script uses prefabs for player messages, contact messages, and timestamps, which can be customized in the Unity editor.
 *******************************************************/


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections;

public class MessageManager : MonoBehaviour
{
    public static MessageManager instance;

    public Transform Content_Parent;

    public GameObject Playersms_Ins;
    public GameObject ContactSms_Ins;
    public GameObject CurrentTime_Ins;

    private ScrollRect scrollRect;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        scrollRect = Content_Parent.GetComponentInParent<ScrollRect>();
    }

    void Start()
    {
        //AddTimestamp("Yesterday", "9:41 AM");

        //AddMessage(true, "Hello!");
        //AddMessage(false, "Hi there!");

        //AddTimestamp("Today", "2:15 PM");

        //AddMessage(true, "Ayudame bibiiiiiiit");
        //AddMessage(false, "nau nau");
    }

    public void AddMessage(bool Sender_Player, string Message)
    {
        GameObject prefabToInstantiate = Sender_Player ? Playersms_Ins : ContactSms_Ins;
        
        GameObject go = Instantiate(prefabToInstantiate, Content_Parent.transform.position, Quaternion.identity);
        go.transform.SetParent(Content_Parent.transform, false);

        TextMeshProUGUI txt = go.GetComponentInChildren<TextMeshProUGUI>();
        if(txt != null)
        {
            txt.text = Message;
        }
        
        StartCoroutine(ScrollToBottom());
    }

    public void AddTimestamp()
    {
        string currentDay = DateTime.Now.ToString("dddd");
        string currentTime = DateTime.Now.ToString("h:mm tt");
        AddTimestamp(currentDay, currentTime);
    }

    public void AddTimestamp(string dayLabel, string timeText)
    {
        GameObject go = Instantiate(CurrentTime_Ins, Content_Parent.transform.position, Quaternion.identity);
        go.transform.SetParent(Content_Parent.transform, false);

        TextMeshProUGUI[] textComponents = go.GetComponentsInChildren<TextMeshProUGUI>();
        
        foreach(TextMeshProUGUI txt in textComponents)
        {
            if(txt.gameObject.name == "TimeText")
            {
                txt.text = timeText;
            }
            else if(txt.gameObject.name == "DayText")
            {
                txt.text = dayLabel;
            }
        }
        
        StartCoroutine(ScrollToBottom());
    }
    
    private IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        
        if (scrollRect != null)
        {
            Canvas.ForceUpdateCanvases();
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }
}
