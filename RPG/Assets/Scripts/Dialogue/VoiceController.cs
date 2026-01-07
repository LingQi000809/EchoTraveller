using UnityEngine;

public class VoiceController : MonoBehaviour
{
    public static VoiceController instance { get; private set; }

    private ChuckSubInstance chuck;
    private bool isListening = false;

    public enum PitchTrend { None, Ascending, Descending, Stable }
    private PitchTrend lastDetectedTrend = PitchTrend.None;

    [Header("Chuck Setup")]
    [SerializeField] private ChuckMainInstance chuckMain;

    [Header("Debug Logging")]
    [SerializeField] private bool verboseLogging = true;

    private bool chuckIsRunning = false;
    private int lastTrendValue = 0;

    private void Awake()
    {
        if (instance != null)
        {
            Debug.LogError("Found more than one VoiceController in the scene.");
            return;
        }
        instance = this;

        chuck = GetComponent<ChuckSubInstance>();
    }

    private void Start()
    {
        if (chuck != null && chuckMain != null)
        {
            Debug.Log("VoiceController ready. ChucK will start when dialogue begins.");
        }
        else
        {
            Debug.LogError("ChuckSubInstance or ChuckMainInstance not properly set up!");
        }
    }

    private void Update()
    {
        if (isListening && chuckIsRunning)
        {
            PollChuckTrend();
        }
    }

    private void PollChuckTrend()
    {
        chuck.GetInt("trendResult", (long trendValue) =>
        {
            if (trendValue != 0 && trendValue != lastTrendValue)
            {
                lastTrendValue = (int)trendValue;

                switch (trendValue)
                {
                    case 1:
                        lastDetectedTrend = PitchTrend.Ascending;
                        Debug.Log("ASCENDING detected from ChucK!");
                        break;
                    case 2:
                        lastDetectedTrend = PitchTrend.Descending;
                        Debug.Log("DESCENDING detected from ChucK!");
                        break;
                    case 3:
                        lastDetectedTrend = PitchTrend.Stable;
                        Debug.Log("STABLE detected from ChucK!");
                        break;
                }
            }
        });
    }


    // public void StartListening()
    // {
    //     if (!chuckIsRunning && chuck != null)
    //     {
    //         chuck.RunFile("PitchDetector.ck");
    //         chuckIsRunning = true;
    //         Debug.Log("ChucK pitch detector started!");
    //     }

    //     isListening = true;
    //     lastDetectedTrend = PitchTrend.None;
    //     lastTrendValue = 0;
    // }
    public void StartListening()
    {
        if (chuck == null) return;

        if (!chuckIsRunning)
        {
            // Run once per game/session — do not re-run every dialogue unless you need a fresh VM
            if (chuck.RunFile("PitchDetector.ck"))
            {
                chuckIsRunning = true;
                Debug.Log("ChucK pitch detector started (RunFile).");
            }
            else
            {
                Debug.LogError("Failed to start PitchDetector.ck");
            }
        }
        else
        {
            // If it was previously silenced, re-enable its audio processing
            chuck.SetRunning(true);
        }
        // Activate the ChucK script
        if (chuck != null)
        {
            chuck.SetInt("isActive", 1);  // Resume processing
        }


        isListening = true;
        lastDetectedTrend = PitchTrend.None;
        lastTrendValue = 0;
        Debug.Log("Now listening for voice input (hum ascending or descending notes for 1.5s)");
    }

    public void StopListening()
    {
        isListening = false;
        lastDetectedTrend = PitchTrend.None;
        lastTrendValue = 0;
        // Pause the ChucK script
        if (chuck != null && chuckIsRunning)
        {
            chuck.SetInt("isActive", 0);  // Pause processing
            Debug.Log("ChucK pitch detector paused!");
        }
        Debug.Log("Stopped listening for voice input");
    }

    public PitchTrend GetLastDetectedTrend()
    {
        PitchTrend trend = lastDetectedTrend;
        Debug.Log($"GetLastDetectedTrend() returning {trend}, then clearing to None");
        lastDetectedTrend = PitchTrend.None;
        return trend;
    }

    public bool HasDetectedTrend()
    {
        bool hasTrend = lastDetectedTrend != PitchTrend.None;
        if (hasTrend)
        {
            Debug.Log($"HasDetectedTrend() = TRUE, lastDetectedTrend = {lastDetectedTrend}");
        }
        return hasTrend;
    }
}
