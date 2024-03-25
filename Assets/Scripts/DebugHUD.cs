using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class DebugHUD : MonoBehaviour
{
    public static DebugHUD instance;

    [SerializeField] TextMeshProUGUI debugLogText;
    [SerializeField] GameObject debugPanel;
    [SerializeField] float logDuration = 10f;
    [SerializeField] bool isActive = false;

    class Log
    {
        public Log(string txt)
        {
            text = txt;
            // Record the time that this msg was logged
            timestamp = Time.fixedTime;
        }
        public string text;
        public float timestamp;
    }

    private List<Log> queue = new List<Log>();
    private Dictionary<string, Log> fixedFields = new Dictionary<string, Log>();
    private float timer = 0f;

    void Awake()
    {
        if (!Application.isEditor && !Debug.isDebugBuild)
        {
            gameObject.SetActive(false);
            return;
        }

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        debugLogText.text = "";
        debugPanel.SetActive(false);
    }
    private void LateUpdate()
    {
        if (!isActive)
        {
            debugLogText.text = "";
            debugPanel.SetActive(false);
            return;
        }

        // Remove old logs
        bool updated = false;
        HashSet<Log> toRemove = new HashSet<Log>();
        float curr = Time.fixedTime;
        foreach (Log log in queue)
        {
            if ((curr - log.timestamp) > logDuration)
            {
                toRemove.Add(log);
                updated = true;
            }
        }
        if (updated)
        {
            foreach (Log log in toRemove)
                queue.Remove(log);

            // Update text
            var builder = new StringBuilder();
            foreach (Log log in queue)
            {
                builder.Append(log.text).Append("\n");
            }

            debugLogText.text = builder.ToString();
        }

        // If idle for > log duration, clear queue
        /*if (timer > logDuration)
        {
            queue.Clear();
            timer = 0f;
        }*/

        // Close panel if we have no logs
        if (queue.Count == 0)
        {
            debugLogText.text = "";
            debugPanel.SetActive(false);
        }

        timer += Time.fixedDeltaTime;
    }

    public void PrintToHUD(string logString, string field)
    {
        if (!isActive)
            return;

        if (logString == null || field == null)
            return;

        // Create a new log
        Log newLog;
        if (fixedFields.ContainsKey(field))
        {
            newLog = fixedFields[field];
            newLog.timestamp = Time.fixedTime;
            newLog.text = field + ": " + logString;
        }
        else
        {
            newLog = new Log(logString);
            fixedFields.Add(field, newLog);
            newLog.text = field + ": " + logString;

            queue.Add(newLog);
        }

        // Open panel
        debugPanel.SetActive(true);

        // Update text
        var builder = new StringBuilder();
        foreach (Log log in queue)
        {
            builder.Append(log.text).Append("\n");
        }
        debugLogText.text = builder.ToString();

        // Reset timer
        timer = 0f;
    }
    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}