using UnityEngine;
using FMODUnity;
using System;
using System.Runtime.InteropServices;
using System.Collections;

public class MusicManager : MonoBehaviour
{

    public static MusicManager instance;

    [SerializeField]
    private EventReference music;

    public TimeLineInfo timelineInfo = null;
    private GCHandle timelineHandle;

    private FMOD.Studio.EventInstance musicInstance;

    private FMOD.Studio.EVENT_CALLBACK beatCallback;

    public delegate void BeatEventDelegate();
    public static event BeatEventDelegate BeatUpdated;

    public delegate void EvenBeatEventDelegate();
    public static event EvenBeatEventDelegate EvenBeatUpdated;

    public delegate void OddBeatEventDelegate();
    public static event OddBeatEventDelegate OddBeatUpdated;


    public delegate void ClearMarkerListenerDelegate();
    public static event ClearMarkerListenerDelegate clearMarkerUpdated;

    public delegate void ChargeMarkerListenerDelegate();
    public static event ChargeMarkerListenerDelegate chargeMarkerUpdated;


    private static int lastBeat = 0;
    private static string lastMarkerString = null;

    [SerializeField] private PlayerController Player1;
    [SerializeField] private PlayerController Player2;
    [SerializeField] private Camera camera;

    [StructLayout(LayoutKind.Sequential)]
    public class TimeLineInfo
    {
        public int currentBeat = 0;
        public FMOD.StringWrapper lastMarker = new FMOD.StringWrapper();
    }



    private void Awake()
    {
        instance = this;
            musicInstance = RuntimeManager.CreateInstance(music);
            musicInstance.start();
    }

    private void Start()
    {
            timelineInfo = new TimeLineInfo();
            beatCallback = new FMOD.Studio.EVENT_CALLBACK(BeatEventCallback);
            timelineHandle = GCHandle.Alloc(timelineInfo, GCHandleType.Pinned);
            musicInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
            musicInstance.setCallback(beatCallback, FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT | FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
    }

    private void OnDestroy()
    {
        musicInstance.setUserData(IntPtr.Zero);
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        timelineHandle.Free();
    }

    private void Update()
    {

        if (lastMarkerString != timelineInfo.lastMarker)
        {
            lastMarkerString = timelineInfo.lastMarker;

            if(clearMarkerUpdated != null)
            {
                clearMarkerUpdated();
            }
        }

        if(lastBeat != timelineInfo.currentBeat)
        {
            lastBeat = timelineInfo.currentBeat;

            if(BeatUpdated != null)
            {
                BeatUpdated();
            }


            if (lastBeat == 1 || lastBeat == 3)
                StartCoroutine(SimpleLerp());


            if (lastBeat == 2 || lastBeat == 4)
                StartCoroutine(SimpleLerp());

            if (EvenBeatUpdated != null)
            {
                if(timelineInfo.currentBeat % 2 == 0)
                EvenBeatUpdated();
                Debug.Log("ÉvenBeat!");
            }

            else if (OddBeatUpdated != null)
            {
                if (timelineInfo.currentBeat % 2 == 1)
                    OddBeatUpdated();
            //    Invoke(nameof(Player2.ClearBeatActions), 0.1f);
                Debug.Log("OddBeat!");
            }
        }
    }

    IEnumerator SimpleLerp()
    {
        int a = 1;  // start
        float b = 0.5f;  // end
        float x = 0.4688f;  // time frame
        float n = 1;  // lerped value
        for (float f = 0; f <= x; f += Time.unscaledDeltaTime)
        {
            n = Mathf.Lerp(a, b, f / x); // passing in the start + end values, and using our elapsed time 'f' as a portion of the total time 'x'                                // use 'n' .. ?
            Time.timeScale = n;
            yield return null;
        }

        for (float f = 0; f <= x; f += Time.unscaledDeltaTime)
        {
            n = Mathf.Lerp(20, 19.9f, f / x); // passing in the start + end values, and using our elapsed time 'f' as a portion of the total time 'x'                                // use 'n' .. ?
            camera.orthographicSize = n;
            yield return null;
        }

    }

    private void ClearPlayer1()
    {
        Player1.ClearBeatActions();
        Debug.Log("Cleared Player 1");
    }

    private void ClearPlayer2()
    {
        Player2.ClearBeatActions();
        Debug.Log("Cleared Player 1");
    }


    void OnGUI()
    {
        GUILayout.Box($"Current beat = {timelineInfo.currentBeat}, Last Marker = {(string)timelineInfo.lastMarker}");
    }

    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(FMOD.Studio.EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        FMOD.Studio.EventInstance instance = new FMOD.Studio.EventInstance(instancePtr);

        IntPtr timelineInfoPtr;
        FMOD.RESULT result = instance.getUserData(out timelineInfoPtr);

        if(result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback Error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimeLineInfo timelineInfo = (TimeLineInfo)timelineHandle.Target;

            switch (type)
            {
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_BEAT:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_BEAT_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_BEAT_PROPERTIES));
                        timelineInfo.currentBeat = parameter.beat;
                    }
                    break;
                case FMOD.Studio.EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (FMOD.Studio.TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(FMOD.Studio.TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.lastMarker = parameter.name;
                    }
                    break;
            }
        }
        return FMOD.RESULT.OK;
    }
}
