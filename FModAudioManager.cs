using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using FMOD;


#region Define
[System.Serializable]
public struct FModParameterReference
{
    //===============================================
    /////          Property and Fields           ////
    //===============================================
    public int   ParamType  { get { return _paramType; } }
    public float ParamValue { get { return _paramValue; } }
    public bool  IsValid    { get { return (_paramType > 0); } }
    public bool  IsGlobal   { get { return _isGlobal; } }

    [SerializeField] private int   _paramType;
    [SerializeField] private float _paramValue;
    [SerializeField] private bool  _isGlobal;



    //======================================
    /////        Public methods        /////
    //======================================
#if FMOD_Event_ENUM
    public FModParameterReference(FModLocalParamType paramType, float value = 0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = false;
        #endregion
    }

    public FModParameterReference(FModGlobalParamType paramType, float value = 0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal = false;
        #endregion
    }

    public void SetParameter(FModLocalParamType paramType, float value=0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = false;
        #endregion
    }

    public void SetParameter(FModGlobalParamType paramType, float value=0f)
    {
        #region Omit
        _paramType = (int)paramType;
        _paramValue = value;
        _isGlobal   = true;
        #endregion
    }
#endif

    public void ClearParameter()
    {
        #region Omit
        _paramType = -1;
        _paramValue = 0f;
        _isGlobal   = false;
        #endregion
    }
}

public struct FModEventInstance
{
    //==================================
    ////     Property And Fields   ///// 
    //==================================
    public FMOD.GUID     GUID
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            FMOD.GUID guid;
            desc.getID(out guid);

            return guid;
        }

    }
    public bool          IsPaused 
    { 
        get {

            bool ret;
            Ins.getPaused(out ret);
            return ret;
        } 
    }
    public bool          IsLoop
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            bool isOneShot;
            desc.isOneshot(out isOneShot);
            return isOneShot;
        }
    }
    public bool          Is3DEvent
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            bool is3D;
            desc.is3D(out is3D);
            return is3D;
        }

    }
    public Vector3       Position3D
    {
        get
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            FMOD.VECTOR pos = attributes.position;
            return new Vector3( pos.x, pos.y, pos.z );
        }

        set
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);


            attributes.position = new FMOD.VECTOR() { 
                x = value.x, 
                y = value.y,
                z = value.z
            };

            Ins.set3DAttributes(attributes);
        }
    }
    public Vector3       Forward3D
    {
        get
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            FMOD.VECTOR dir = attributes.forward;
            return new Vector3(dir.x, dir.y, dir.z);
        }

        set
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            attributes.forward = new FMOD.VECTOR()
            {
                x = value.x,
                y = value.y,
                z = value.z
            };

            Ins.set3DAttributes(attributes);
        }
    }
    public Vector3       Up3D
    {
        get
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            FMOD.VECTOR dir = attributes.up;
            return new Vector3(dir.x, dir.y, dir.z);
        }

        set
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);


            attributes.up = new FMOD.VECTOR()
            {
                x = value.x,
                y = value.y,
                z = value.z
            };

            Ins.set3DAttributes(attributes);
        }
    }
    public Vector3       Velocity3D
    {
        get
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);

            FMOD.VECTOR velocity = attributes.velocity;
            return new Vector3(velocity.x, velocity.y, velocity.z);
        }

        set
        {
            FMOD.ATTRIBUTES_3D attributes;
            Ins.get3DAttributes(out attributes);


            attributes.velocity = new FMOD.VECTOR()
            {
                x = value.x,
                y = value.y,
                z = value.z
            };

            Ins.set3DAttributes(attributes);
        }
    }
    public bool          IsValid{ get{ return Ins.isValid(); } }
    public float         Volume
    {
        get
        {

            float volume;
            Ins.getVolume(out volume);
            return volume;
        }

        set { Ins.setVolume((value < 0 ? 0f : value)); }
    }
    public float         Pitch
    {
        get
        {
            float pitch;
            Ins.getPitch(out pitch);
            return pitch;
        }

        set { Ins.setPitch(value); }
    }
    public bool          IsPlaying
    {
        get
        {
            FMOD.Studio.PLAYBACK_STATE state;
            Ins.getPlaybackState(out state);
            return (state == FMOD.Studio.PLAYBACK_STATE.PLAYING);
        }

    }
    public int           TimelinePosition
    {
        get
        {
            int position;
            Ins.getTimelinePosition(out position);
            return position;
        }

        set{  Ins.setTimelinePosition(value); }
    }
    public float         TimelinePositionRatio
    {
        get
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);

            int position;
            Ins.getTimelinePosition(out position);

            return ((float)position/(float)length);
        }

        set
        {
            EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);

            float ratio = Mathf.Clamp(value, 0.0f, 1.0f);
            Ins.setTimelinePosition( Mathf.RoundToInt(length*ratio) );
        }
    }
    public int           Length
    {
        get
        {
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);

            int length;
            desc.getLength(out length);
            return length;
        }
    }
    public float         Min3DDistance
    {
        get
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                float distance;
                Ins.getProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, out distance);
                return distance;
            }

            return 0f;
        }

        set
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, value);
            }
        }
    }
    public float         Max3DDistance
    {
        get
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                float distance;
                Ins.getProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, out distance);
                return distance;
            }

            return 0f;
        }

        set
        {
            bool is3D;
            FMOD.Studio.EventDescription desc;
            Ins.getDescription(out desc);
            desc.is3D(out is3D);
            if (is3D)
            {
                Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, value);
            }
        }
    }

    private EventInstance Ins;

    //==================================
    ////        Public Methods     ///// 
    //==================================
    public FModEventInstance(EventInstance instance, Vector3 position=default) 
    { 
        Ins = instance;

        bool is3D;
        FMOD.Studio.EventDescription desc;
        Ins.getDescription(out desc);
        desc.is3D(out is3D);
        if (is3D)
        {
            Ins.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        }
    }

    public static explicit operator EventInstance(FModEventInstance ins) { 
        return ins.Ins; 
    }

    public void Play(float volume = -1f, float startTimelinePositionRatio = -1f, string paramName = "", float paramValue = 0f)
    {
        if(volume>=0)     Ins.setVolume(volume);
        if(paramName!="") Ins.setParameterByName(paramName, paramValue);
        if(startTimelinePositionRatio>=0f) TimelinePositionRatio = startTimelinePositionRatio;
        Ins.start();
    }

    public void Pause()
    {
        Ins.setPaused(true);
    }

    public void Resume()
    {
        Ins.setPaused(false);
    }

    public void Stop()
    {
        Ins.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void Destroy(bool destroyAtStop=false)
    {
        if (destroyAtStop)
        {
            Ins.release();
            return;
        }

        Ins.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
        Ins.release();
        Ins.clearHandle();
    }

#if FMOD_Event_ENUM
    public void SetParameter(FModGlobalParamType paramType, float paramValue)
    {
        string paramName = FModReferenceList.Params[(int)paramType];

        FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
        system.setParameterByName(paramName, paramValue);
    }

    public void SetParameter(FModLocalParamType paramType, float paramValue)
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        Ins.setParameterByName(paramName, paramValue);
    }
#endif

    public void SetParameter(FModParameterReference paramRef)
    {
#if FMOD_Event_ENUM
        if (paramRef.IsValid == false) return;
        string paramName = FModReferenceList.Params[paramRef.ParamType];

        /**글로벌 파라미터일 경우...*/
        if(paramRef.IsGlobal)
        {
            FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
            system.setParameterByName(paramName, paramRef.ParamValue);
            return;
        }

        /**로컬 파라미터일 경우...*/
        Ins.setParameterByName( paramName, paramRef.ParamValue);
#endif
    }

    public void SetParameter(string paramName, float value)
    {
        Ins.setParameterByName(paramName, value);
    }

#if FMOD_Event_ENUM
    public float GetParameter(FModGlobalParamType paramType)
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        float value;
        FMOD.Studio.System system = FMODUnity.RuntimeManager.StudioSystem;
        system.getParameterByName(paramName, out value);

        return value;
    }

    public float GetParameter(FModLocalParamType paramType) 
    {
        string paramName = FModReferenceList.Params[(int)paramType];
        float value;
        Ins.getParameterByName(paramName, out value);

        return value;
    }
#endif

    public float GetParameter(string paramName) 
    {
        float value;
        Ins.getParameterByName(paramName, out value);

        return value;
    }

    public void Set3DDistance(float minDistance, float maxDistance )
    {
        bool is3D;
        FMOD.Studio.EventDescription desc;
        Ins.getDescription(out desc);
        desc.is3D(out is3D);
        if (is3D)
        {
            Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MINIMUM_DISTANCE, minDistance);
            Ins.setProperty(FMOD.Studio.EVENT_PROPERTY.MAXIMUM_DISTANCE, maxDistance);
        }
    }
}

[StructLayout(LayoutKind.Sequential)]
public struct TIMELINE_MARKER_PROPERTIESEX
{
    public string MarkerName;
    public int    TimelinePosition;
    public float  TimelinePositionRatio;
}

#endregion

public interface IFModEventFadeComplete { void OnFModEventComplete(int fadeID, float goalVolume); }
public interface IFModEventCallBack     { void OnFModEventCallBack(FMOD.Studio.EVENT_CALLBACK_TYPE eventType, FModEventInstance eventTarget, int paramKey); }
public delegate void FModEventFadeCompleteNotify( int fadeID, float goalVolume );
public delegate void FModEventCallBack( FMOD.Studio.EVENT_CALLBACK_TYPE eventType, FModEventInstance eventTarget, int paramKey );

[AddComponentMenu("FMOD Studio/FModAudioManager")]
public sealed class FModAudioManager : MonoBehaviour
{
    #region Define
    private struct FadeInfo
    {
#if FMOD_Event_ENUM
        public FModEventInstance TargetIns;
#endif
        public int   TargetBusIdx;
        public int   FadeID;
        public float duration;
        public float durationDiv;
        public float startVolume;
        public float distance;
        public bool  destroyAtCompleted;
        public bool  pendingKill;
    }

    private struct CallBackInfo
    {
        public FModEventInstance   EventKey;
        public EVENT_CALLBACK_TYPE EventType;
        public int                 ParamKey;
    }

    private struct CallBackRegisterInfo
    {
        public FModEventCallBack Func;
        public bool              UsedDestroyEvent;
    }

    private struct ParamDesc
    {
        public int StartIdx;
        public int Length;
    }

    private enum FadeState
    {
        None,
        PendingKill_Ready,
        PendingKill
    }
    #endregion

    //=======================================
    /////            Property           /////
    ///======================================
    public static bool  UsedBGMAutoFade     { get; set; } = false;
    public static bool  IsInitialized       { get { return _Instance != null; } }
    public static float BGMAutoFadeDuration { get; set; } = 3f;
    public const  int   BGMAutoFadeID = -9324;
    public static FModEventFadeCompleteNotify OnEventFadeComplete;



    //=======================================
    /////            Fields            /////
    ///======================================
    private static FModAudioManager _Instance;
#if FMOD_Event_ENUM
    private FModEventInstance       _BGMIns;
#endif

    private FMOD.Studio.Bus[] _BusList;
 
    /**Fade fields....**/
    private FadeInfo[]   _fadeInfos = new FadeInfo[10];
    private int          _fadeCount = 0;
    private FadeState    _fadeState = FadeState.None;


    /**event callback fields...**/
    private static EVENT_CALLBACK _cbDelegate = new EVENT_CALLBACK(Callback_Internal);
    private static Dictionary<FModEventInstance, CallBackRegisterInfo> _callBackTargets = new Dictionary<FModEventInstance, CallBackRegisterInfo>();

    private static CallBackInfo[] _callbackInfos = new CallBackInfo[10];
    private static int            _callbackCount = 0;

    private static List<ParamDesc>  _paramDescs = new List<ParamDesc>();
    private static byte[]           _paramBytes = new byte[10];
    private static int              _usedBytes  = 0;


    /**Audo bgm fields...**/
    private int     _NextBGMEvent         = -1;
    private float   _NextBGMVolume        = 0f;
    private float   _NextBGMStartPosRatio = 0;
    private int     _NextBGMParam         = -1;
    private float   _NextBGMParamValue    = 0f;
    private Vector3 _NextBGMPosition      = Vector3.zero;



    //=======================================
    /////         Core Methods          /////
    ///======================================
    
    private static bool InstanceIsValid()
    {
        #region Omit
        if (_Instance==null)
        {
#if UNITY_EDITOR
            UnityEngine.Debug.LogError("To use the methods provided by FModAudioManager, there must be at least one GameObject in the scene with the FModAudioManager component attached.");
            UnityEngine.Debug.LogWarning("Alternatively, this error might occur if you try to use the methods before FModAudioManager has finished initializing.\nIf this error appears when calling a function in the Awake() magic method, try moving the code to the Start() magic method instead.");
#endif
            return false;
        }

        return true;
        #endregion
    }


    /*************************************************
     *      Callback Methods....
     * *******/
    [AOT.MonoPInvokeCallback(typeof(FMOD.Studio.EVENT_CALLBACK))]
    private static FMOD.RESULT Callback_Internal(EVENT_CALLBACK_TYPE eventType, IntPtr instance, IntPtr ptrParams)
    {
        #region Omit

        /****************************************************************
         *    스레드 세이프를 위해, 콜백 정보를 Update에서 처리하도록 한다...
         * *****/
        lock(_cbDelegate)
        {
            bool isDestroyedEvent     = (eventType == EVENT_CALLBACK_TYPE.DESTROYED);
            FModEventInstance target  = new FModEventInstance(new EventInstance(instance));
            CallBackRegisterInfo info = _callBackTargets[target];


            /*********************************************************
             *    이벤트에 따른 파라미터 구조체 정보를 기록한다...
             * *****/

            #region PARAMETERS
            /**MARKER 이벤트일 경우.....**/
            if (eventType == EVENT_CALLBACK_TYPE.TIMELINE_MARKER)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<TIMELINE_MARKER_PROPERTIES>(ptrParams);
                TIMELINE_MARKER_PROPERTIESEX newEx = new TIMELINE_MARKER_PROPERTIESEX()
                {
                    MarkerName       = parameters.name,
                    TimelinePosition = parameters.position
                };

                PasteStructByte(newEx);
            }

            /**TIMELINE BEAT 이벤트일 경우...**/
            else if (eventType == EVENT_CALLBACK_TYPE.TIMELINE_BEAT)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<TIMELINE_BEAT_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**PROGRAMMER SOUND 관련 이벤트일 경우...*/
            else if (eventType == EVENT_CALLBACK_TYPE.CREATE_PROGRAMMER_SOUND || eventType == EVENT_CALLBACK_TYPE.DESTROY_PROGRAMMER_SOUND)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<PROGRAMMER_SOUND_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**PLUGIN INSTANCE 관련 이벤트일 경우...**/
            else if (eventType == EVENT_CALLBACK_TYPE.PLUGIN_CREATED || eventType == EVENT_CALLBACK_TYPE.PLUGIN_DESTROYED)
            {
                var parameters = System.Runtime.InteropServices.Marshal.PtrToStructure<PLUGIN_INSTANCE_PROPERTIES>(ptrParams);
                PasteStructByte(parameters);
            }

            /**SOUND 관련 이벤트일 경우...**/
            else if (eventType == EVENT_CALLBACK_TYPE.SOUND_PLAYED || eventType == EVENT_CALLBACK_TYPE.SOUND_STOPPED)
            {
                var parameter = System.Runtime.InteropServices.Marshal.PtrToStructure<IntPtr>(ptrParams);
                PasteStructByte(parameter);
            }

            /**시작 관련 이벤트일 경우...**/
            else if (eventType == EVENT_CALLBACK_TYPE.START_EVENT_COMMAND)
            {
                var parameter = System.Runtime.InteropServices.Marshal.PtrToStructure<IntPtr>(ptrParams);
                PasteStructByte(parameter);
            }

            #endregion


            /**********************************************************
             *    이벤트 호출을 예약한다....
             * *****/
            CallBackInfo newInfo = new CallBackInfo()
            {
                EventKey = target,
                ParamKey = _callbackCount,
                EventType = eventType
            };

            /**콜백 정보를 담을 배열이 가득찼다면 배로 할당한다...*/
            int len = _callbackInfos.Length;
            if (len <= _callbackCount)
            {
                CallBackInfo[] newArr = new CallBackInfo[len * 2];
                Array.Copy(_callbackInfos, newArr, len);
                _callbackInfos = newArr;
            }

            _callbackInfos[_callbackCount++] = newInfo;
        }

        return FMOD.RESULT.OK;
        #endregion
    }

    private void CallbackProgress_internal()
    {
        #region Omit
        /**********************************************
         *   모든 콜백 정보들을 처리한다...
         * *******/
        lock (_cbDelegate)
        {
            try
            {
                for (int i = 0; i < _callbackCount; i++){

                    ref CallBackInfo     info         = ref _callbackInfos[i];
                    EventInstance        ins          = (EventInstance)info.EventKey;
                    CallBackRegisterInfo registerInfo = _callBackTargets[info.EventKey];

                    /**이벤트가 파괴될 경우, 관리 대상에서 제외시킨다...**/
                    if (info.EventType == EVENT_CALLBACK_TYPE.DESTROYED)
                    {
                        _callBackTargets.Remove(info.EventKey);
                        if (registerInfo.UsedDestroyEvent == false) continue;
                    }

                    registerInfo.Func?.Invoke(info.EventType, info.EventKey, info.ParamKey);
                }
            }
            catch { }

            _callbackCount = 0;
            _usedBytes     = 0;
            _paramDescs.Clear();
        }

        #endregion
    }

    private static void PasteStructByte<T>(T copyTarget)
    {
        #region Omit
        int size       = Marshal.SizeOf(typeof(T));
        int Targetlen  = _paramBytes.Length;

        /***********************************************
         *    할당할 공간이 부족하면 확장한다...
         * *****/
        if(Targetlen < (_usedBytes + size))
        {
            byte[] newArr = new byte[(Targetlen*2)+size];
            Array.Copy(_paramBytes, newArr, _usedBytes);
            _paramBytes = newArr;
        }


        /************************************************
         *    byte 배열에 구조체를 할당한다...
         * ******/
        IntPtr ptr = Marshal.AllocHGlobal(size);

        Marshal.StructureToPtr(copyTarget, ptr, false);
        Marshal.Copy(ptr, _paramBytes, _usedBytes, size);
        Marshal.FreeHGlobal(ptr);

        _paramDescs.Add(new ParamDesc()
        {
             Length   = size,
             StartIdx = _usedBytes
        });
        _usedBytes += size;
        #endregion
    }

    private static bool ParamKeyIsValid(int paramKey)
    {
        return (paramKey >=0 || paramKey < _paramDescs.Count);
    }

    private static T GetCallbackParam_internal<T>(int paramKey) where T : struct
    {
        #region Omit

        /**paramKey가 유효하지 않다면 탈출한다....**/
        if(!ParamKeyIsValid(paramKey)){
            return new T();
        }

        /***************************************************************
         *    byte 배열에서 구조체에 대한 메모리를 복사해 구조체를 만든다...
         * ******/
        ParamDesc desc = _paramDescs[paramKey];
        IntPtr    ptr  = Marshal.AllocHGlobal(desc.Length);
        T         ret;

        Marshal.Copy(_paramBytes, desc.StartIdx, ptr, desc.Length);
        ret = Marshal.PtrToStructure<T>(ptr);

        Marshal.FreeHGlobal(ptr);
        return ret;

        #endregion
    }

    public static void SetEventCallback(FModEventInstance eventTarget, EVENT_CALLBACK_TYPE eventTypeMask, FModEventCallBack callbackFunc)
    {
        #region Omit
        if (!InstanceIsValid() || !eventTarget.IsValid) return;

        bool usedDestroyEvent  = ((int)(eventTypeMask & EVENT_CALLBACK_TYPE.DESTROYED))>0;
        EventInstance ins      = (EventInstance)eventTarget;
        eventTypeMask         |= EVENT_CALLBACK_TYPE.DESTROYED;

        /**등록할 콜백정보를 초기화한다....*/
        CallBackRegisterInfo newInfo = new CallBackRegisterInfo()
        {
            Func             = callbackFunc,
            UsedDestroyEvent = usedDestroyEvent
        };

        /**키가 이미 존재한다면 값을 변경한다....**/
        if (_callBackTargets.ContainsKey(eventTarget))
        {
            _callBackTargets[eventTarget] = newInfo;
        }

        /**키가 존재하지 않다면 새롭게 추가한다...*/
        else
        {
            _callBackTargets.Add(eventTarget, newInfo);
        }

        ins.setCallback(_cbDelegate, eventTypeMask);
        #endregion
    }

    public static void SetBGMEventCallback(EVENT_CALLBACK_TYPE eventTypeMask, FModEventCallBack callbackFunc)
    {
#if FMOD_Event_ENUM
        if (!InstanceIsValid()) return;
        SetEventCallback(_Instance._BGMIns, eventTypeMask, callbackFunc);
#endif
    }

    public static void ClearEventCallback(FModEventInstance eventTarget)
    {
        #region Omit
        if (!InstanceIsValid() || !eventTarget.IsValid) return;

        _callBackTargets.Remove(eventTarget);

        EventInstance ins = (EventInstance)eventTarget;
        ins.setCallback(null);
        #endregion
    }

    public static void ClearBGMEventCallback()
    {
#if FMOD_Event_ENUM
        if (!InstanceIsValid()) return;
        ClearEventCallback(_Instance._BGMIns);
#endif
    }

    public static TIMELINE_MARKER_PROPERTIESEX GetCallbackParams_Marker(int parameterKey)
    {
        TIMELINE_MARKER_PROPERTIESEX ret = GetCallbackParam_internal<TIMELINE_MARKER_PROPERTIESEX>(parameterKey);

        if(ParamKeyIsValid(parameterKey)){
            ret.TimelinePositionRatio = ((float)ret.TimelinePosition / (float)_callbackInfos[parameterKey].EventKey.Length);
        }

        return ret;
    }

    public static TIMELINE_BEAT_PROPERTIES GetCallbackParams_Beat(int parameterKey)
    {
        return GetCallbackParam_internal<TIMELINE_BEAT_PROPERTIES>(parameterKey);
    }

    public static PROGRAMMER_SOUND_PROPERTIES GetCallbackParams_ProgrammerSound(int parameterKey)
    {
        return GetCallbackParam_internal<PROGRAMMER_SOUND_PROPERTIES>(parameterKey);
    }

    public static PLUGIN_INSTANCE_PROPERTIES GetCallbackParams_PluginInstance(int parameterKey)
    {
        return GetCallbackParam_internal<PLUGIN_INSTANCE_PROPERTIES>(parameterKey);
    }

    public static Sound GetCallbackParams_Sound(int parameterKey)
    {
        return GetCallbackParam_internal<Sound>(parameterKey);
    }

    public static FModEventInstance GetCallbackParams_StartEventCommand(int parameterKey)
    {
        return new FModEventInstance(GetCallbackParam_internal<EventInstance>(parameterKey));
    }

    public static TIMELINE_NESTED_BEAT_PROPERTIES GetCallbackParams_NestedBeat(int parameterKey)
    {
        return GetCallbackParam_internal<TIMELINE_NESTED_BEAT_PROPERTIES>(parameterKey);
    }



#if FMOD_Event_ENUM
    /*****************************************
     *   Bus Methods
     * ***/
    public static void SetBusVolume(FModBusType busType, float newVolume)
     {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.setVolume(newVolume);
        #endregion
    }

    public static float GetBusVolume(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return 0;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return 0;

        float volume;
        bus.getVolume(out volume);
        return volume;
        #endregion
    }

    public static void StopBusAllEvents(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;
        bus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        #endregion
    }

    public static void SetBusMute(FModBusType busType, bool isMute)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return;

        bus.setMute(isMute);
        #endregion
    }

    public static bool GetBusMute(FModBusType busType)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        int index = (int)busType;
        FMOD.Studio.Bus bus = _Instance._BusList[index];

        if (bus.isValid() == false) return false;

        bool isMute;
        bus.getMute(out isMute);
        return isMute;
        #endregion
    }

    public static void SetAllBusMute(bool isMute)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        int Count = _Instance._BusList.Length;
        for(int i=0; i<Count; i++)
        {
            if (!_Instance._BusList[i].isValid()) continue;
            _Instance._BusList[i].setMute(isMute);
        }
        #endregion
    }


    /****************************************
     *   Bank Methods
     * ****/
    public static void LoadBank(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string bankName = FModReferenceList.Banks[(int)bankType];
        try
        {
            FMODUnity.RuntimeManager.LoadBank(bankName);
        }
        catch 
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.LoadBank(...)!!");
            #endif
        }
        #endregion
    }

    public static void UnloadBank(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            string bankName = FModReferenceList.Banks[(int)bankType];
            FMODUnity.RuntimeManager.UnloadBank(bankName);
        }
        catch
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.UnLoadBank(...)!!");
            #endif
        }
        #endregion
    }

    public static bool BankIsLoaded(FModBankType bankType)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        string bankName = FModReferenceList.Banks[(int)bankType];
        return FMODUnity.RuntimeManager.HasBankLoaded(bankName);
        #endregion
    }

    public static void LoadAllBank()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //모든 뱅크를 로드한다.
        for(int i=0; i<Count; i++){

            if (FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            try { FMODUnity.RuntimeManager.LoadBank(bankLists[i]); } catch { continue; }
        }
        #endregion
    }

    public static void UnLoadAllBank()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        string[] bankLists = FModReferenceList.Banks;
        int Count = bankLists.Length;

        //모든 뱅크를 언로드한다.
        for (int i = 0; i < Count; i++)
        {
            if (!FMODUnity.RuntimeManager.HasBankLoaded(bankLists[i])){

                continue;
            }

            try { FMODUnity.RuntimeManager.UnloadBank(bankLists[i]); } catch { continue; }
        }
        #endregion
    }


    /******************************************
     *   FModEventInstance Methods
     * **/
    public static FModEventInstance CreateInstance(FModBGMEventType eventType, Vector3 position=default)
    {
        #region Omit
        if (!InstanceIsValid()) return new FModEventInstance();

        try
        {
            FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid), position);
            return newInstance;
        }
        catch {

#if UNITY_EDITOR
            UnityEngine.Debug.Log("FModAudioManager.CreateInstance() failed! Please check if the Bank with the event you want to use has been loaded.");
#endif
            return new FModEventInstance(); 
        }
        #endregion
    }

    public static FModEventInstance CreateInstance(FModSFXEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static FModEventInstance CreateInstance(FModNoGroupEventType eventType, Vector3 position=default)
    {
        return CreateInstance((FModBGMEventType)eventType, position);
    }

    public static FModEventInstance CreateInstance(FMODUnity.EventReference eventRef, Vector3 position = default)
    {
        #region Omit
        if (!InstanceIsValid()) return new FModEventInstance();

        try
        {
            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(eventRef.Guid), position);
            return newInstance;
        }
        catch { return new FModEventInstance(); }
        #endregion
    }

    public static void StopAllInstance()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        FMOD.Studio.Bus[] busLists = _Instance._BusList;
        int Count = busLists.Length;

        for(int i=0; i<Count; i++){

            busLists[i].stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
        }
        #endregion
    }


    /*********************************************
     *  PlayOneShot Methods
     * ***/
    private static void PlayOneShotSFX_internal(FModSFXEventType eventType, Vector3 position, float volume, float startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, float minDistance, float maxDistance)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            FMOD.GUID guid = FModReferenceList.Events[(int)eventType];
            bool volumeIsChanged = (volume >= 0f);
            bool paramIsChanged  = (paramType > 0);

            FModEventInstance newInstance = new FModEventInstance(FMODUnity.RuntimeManager.CreateInstance(guid));
            newInstance.Set3DDistance(minDistance, maxDistance);
            newInstance.Position3D            = position;
            newInstance.TimelinePositionRatio = startTimelinePositionRatio;
            if (volumeIsChanged) newInstance.Volume = volume;
            if (paramIsChanged)
            {
                if (isGlobal) newInstance.SetParameter((FModGlobalParamType)paramType, paramValue);
                else newInstance.SetParameter((FModLocalParamType)paramType, paramValue);
            }
            newInstance.Play();
            newInstance.Destroy(true);
        }
        catch 
        {
            #if UNITY_EDITOR
            UnityEngine.Debug.LogWarning("failed FModAudioManager.PlayOneShotSFX(...)!!");
            #endif
        }
        #endregion
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModGlobalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal(eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModSFXEventType eventType, FModParameterReference paramRef, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**글로벌 파라미터일 경우...*/
        if (paramRef.IsGlobal){

            PlayOneShotSFX( eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance,maxDistance);
            return;
        }

        /**로컬 파라미터일 경우...*/
        PlayOneShotSFX(eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
        #endregion
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModParameterReference paramRef, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**글로벌 파라미터일 경우...*/
        if (paramRef.IsGlobal)
        {

            PlayOneShotSFX((FModSFXEventType)eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
            return;
        }

        /**로컬 파라미터일 경우...*/
        PlayOneShotSFX((FModSFXEventType)eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, position, volume, startTimelinePositionRatio, minDistance, maxDistance);
        #endregion
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f,  float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, -1, 0, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, Vector3 position = default, float volume = -1f, float startTimelinePositionRatio = 0f, float minDistance = 1f, float maxDistance = 20f)
    {
        PlayOneShotSFX_internal((FModSFXEventType)eventType, position, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, minDistance, maxDistance);
    }

    public static void PlayOneShotSFX(FMODUnity.EventReference eventRef, Vector3 position, FModParameterReference paramRef =default, float volume=-1f, float startTimelinePositionRatio = 0f, float minDistance=1f, float maxDistance=20f)
    {
        #region Omit
        /**************************************
         *   해당 GUID에 대한 인덱스값을 얻는다...
         * ***/
        int Count = FModReferenceList.Events.Length;
        int       index = -1;
        FMOD.GUID guid  = eventRef.Guid;
        for(int i=0; i<Count; i++)
        {
            if (FModReferenceList.Events[i].Equals(guid)){

                index = i;
                break;
            }
        }

        /**존재하지 않는 이벤트는 실행할 수 없다...*/
        if (index == -1) return;

        /**********************************************
         *   파라미터값에 따라 적절히 오버로딩을 호출한다...
         * ***/
        PlayOneShotSFX_internal((FModSFXEventType)index, position, volume, startTimelinePositionRatio, paramRef.IsGlobal, paramRef.ParamType, paramRef.ParamValue, minDistance, maxDistance);
        #endregion
    }


    /*********************************************
     *   BGM Methods
     * ***/
    private static void PlayBGM_internal(FModBGMEventType eventType, float volume, float startTimelinePositionRatio, bool isGlobal, int paramType, float paramValue, Vector3 position)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        try
        {
            bool volumeIsChanged = (volume >= 0f);
            bool paramIsChanged = (paramType > 0);

            //기존에 BGM 인스턴스가 존재할 경우
            if (_Instance._BGMIns.IsValid)
            {
                if (UsedBGMAutoFade)
                {
                    _Instance._NextBGMEvent         = (int)eventType;
                    _Instance._NextBGMVolume        = volume;
                    _Instance._NextBGMStartPosRatio = startTimelinePositionRatio;
                    _Instance._NextBGMParam         = (int)paramType;
                    _Instance._NextBGMParamValue    = paramValue;
                    _Instance._NextBGMPosition      = position;

                    StopFade(BGMAutoFadeID);
                    ApplyBGMFade(0f, BGMAutoFadeDuration * .5f, BGMAutoFadeID, true);
                    return;
                }
                else _Instance._BGMIns.Destroy();
            }

            _Instance._BGMIns                       = CreateInstance(eventType, position);
            _Instance._BGMIns.Position3D            = position;
            _Instance._BGMIns.TimelinePositionRatio = startTimelinePositionRatio;
            if (paramIsChanged)
            {
                if (isGlobal) _Instance._BGMIns.SetParameter((FModGlobalParamType)paramType, paramValue);
                else _Instance._BGMIns.SetParameter((FModLocalParamType)paramType, paramValue);
            }

            //페이드를 적용하면서 시작할 경우
            if (UsedBGMAutoFade)
            {
                float newVolume = (volumeIsChanged ? volume : _Instance._BGMIns.Volume);
                _Instance._NextBGMEvent = -1;
                _Instance._BGMIns.Volume = 0f;
                ApplyBGMFade(newVolume, BGMAutoFadeDuration, BGMAutoFadeID);
            }
            else if (volumeIsChanged) _Instance._BGMIns.Volume = volume;

            _Instance._BGMIns.Play();
        }
        catch
        {
           #if UNITY_EDITOR
           UnityEngine.Debug.LogWarning("failed FModAudioManager.PlayBGM(...)!!");
           #endif
        }
        #endregion
    }

    public static void PlayBGM(FModBGMEventType eventType, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, false,  (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModGlobalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal(eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModBGMEventType eventType, FModParameterReference paramRef, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**글로벌 파라미터일 경우...*/
        if(paramRef.IsGlobal)
        {
            PlayBGM(eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        }

        /**로컬 파라미터일 경우...*/
        PlayBGM(eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        #endregion
    }

    public static void PlayBGM(FModNoGroupEventType eventType, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, -1, 0, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModLocalParamType paramType, float paramValue = 0f, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, false, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModGlobalParamType paramType, float paramValue = 0f,float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        PlayBGM_internal((FModBGMEventType)eventType, volume, startTimelinePositionRatio, true, (int)paramType, paramValue, position);
    }

    public static void PlayBGM(FModNoGroupEventType eventType, FModParameterReference paramRef, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        if (paramRef.IsValid == false) return;

        /**글로벌 파라미터일 경우...*/
        if (paramRef.IsGlobal)
        {
            PlayBGM((FModBGMEventType)eventType, (FModGlobalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        }

        /**로컬 파라미터일 경우...*/
        PlayBGM((FModBGMEventType)eventType, (FModLocalParamType)paramRef.ParamType, paramRef.ParamValue, volume, startTimelinePositionRatio, position);
        #endregion
    }

    public static void PlayBGM(FMODUnity.EventReference eventRef, FModParameterReference paramRef=default, float volume = -1f, float startTimelinePositionRatio = 0f, Vector3 position = default)
    {
        #region Omit
        /**************************************
         *   해당 GUID에 대한 인덱스값을 얻는다...
         * ***/
        int Count = FModReferenceList.Events.Length;
        int index = -1;
        FMOD.GUID guid = eventRef.Guid;
        for (int i = 0; i < Count; i++)
        {
            if (FModReferenceList.Events[i].Equals(guid)){

                index = i;
                break;
            }
        }

        /**존재하지 않는 이벤트는 실행할 수 없다...*/
        if (index == -1) return;

        PlayBGM_internal((FModBGMEventType)index, volume, startTimelinePositionRatio, paramRef.IsGlobal, paramRef.ParamType, paramRef.ParamValue, position);
        #endregion
    }

    public static void StopBGM()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;

        if(UsedBGMAutoFade)
        {
            _Instance._NextBGMEvent = -1;

            StopFade(BGMAutoFadeID);
            ApplyBGMFade(0f, BGMAutoFadeDuration, BGMAutoFadeID, true);
            return;
        }

        _Instance._BGMIns.Stop();
        #endregion
    }

    public static void DestroyBGM(bool destroyAtStop = false)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Destroy(destroyAtStop);
        #endregion
    }

    public static void SetBGMPause()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Pause();
        #endregion
    }

    public static void SetBGMResume()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.Resume();
        #endregion
    }

    public static void SetBGMVolume(float newVolume)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;
        _Instance._BGMIns.Volume = newVolume;
        #endregion
    }

    public static float GetBGMVolume()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.Volume;
        #endregion
    }

    public static void SetBGMParameter(FModGlobalParamType paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(FModLocalParamType paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(FModParameterReference paramRef, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || paramRef.IsValid == false) return;

        /**글로벌 파라미터일 경우....*/
        if(paramRef.IsGlobal)
        {
            SetBGMParameter( (FModGlobalParamType)paramRef.ParamType, paramValue);
            return;
        }

        /**로컬 파라미터일 경우...*/
        SetBGMParameter( (FModLocalParamType)paramRef.ParamType, paramValue);
        #endregion
    }

    public static void SetBGMParameter(string paramType, float paramValue)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.SetParameter(paramType, paramValue);
        #endregion
    }

    public static float GetBGMParameter(FModGlobalParamType paramType) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
        #endregion
    }

    public static float GetBGMParameter(FModLocalParamType paramType) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramType);
        #endregion
    }

    public static float GetBGMParameter(string paramName) 
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return -1f;
        return _Instance._BGMIns.GetParameter(paramName);
        #endregion
    }

    public static void SetBGMTimelinePosition(int timelinePositionMillieSeconds)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePosition= timelinePositionMillieSeconds;
        #endregion
    }

    public static void SetBGMTimelinePosition(float timelinePositionRatio)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return;
        _Instance._BGMIns.TimelinePositionRatio = timelinePositionRatio;
        #endregion
    }

    public static int GetBGMTimelinePosition()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0;
        return _Instance._BGMIns.TimelinePosition;
        #endregion
    }

    public static float GetBGMTimelinePositionRatio()
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid == false) return 0f;
        return _Instance._BGMIns.TimelinePositionRatio;
        #endregion
    }


    /********************************************
     *   Fade Methods
     * ***/
    public static void ApplyBusFade(FModBusType busType, float goalVolume, float fadeTime, int fadeID=0)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        _Instance.AddFadeInfo(new FModEventInstance(), goalVolume, fadeTime, fadeID, false, (int)busType);
        #endregion
    }

    public static void ApplyInstanceFade(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        #region Omit
        if (!InstanceIsValid() || Instance.IsValid==false) return;

        _Instance.AddFadeInfo(Instance, goalVolume, fadeTime, fadeID, completeDestroy);
        #endregion
    }

    public static void ApplyBGMFade(float goalVolume, float fadeTime, int fadeID = 0, bool completeDestroy = false)
    {
        #region Omit
        if (!InstanceIsValid() || _Instance._BGMIns.IsValid==false) return;

        _Instance.AddFadeInfo(_Instance._BGMIns, goalVolume, fadeTime, fadeID, completeDestroy);
        #endregion
    }

    public static bool FadeIsPlaying(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return false;

        for(int i=0; i<_Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) return true;
        }

        return false;
        #endregion
    }

    public static int GetFadeCount(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return 0;

        int total = 0;
        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID) total++;
        }

        return total;
        #endregion
    }

    public static void StopFade(int FadeID)
    {
        #region Omit
        if (!InstanceIsValid()) return;

        for (int i = 0; i < _Instance._fadeCount; i++){

            if (_Instance._fadeInfos[i].FadeID == FadeID)
            {
                _Instance.RemoveFadeInfo(i);
                if(_Instance._fadeState==FadeState.None) i--;
            }
        }
        #endregion
    }

    public static void StopAllFade()
    {
        #region Omit
        if (!InstanceIsValid()) return;

        _Instance._fadeCount = 0;
        #endregion
    }

    private void AddFadeInfo(FModEventInstance Instance, float goalVolume, float fadeTime, int fadeID, bool completeDestroy, int busType=-1)
    {
        #region Omit
        //FadeInfo 배열이 존재하지 않는다면 스킵.
        if (_fadeInfos == null) return;


        //새로운 페이드 정보가 들어갈 공간이 부족하다면 배로 할당.
        int containerCount = _fadeInfos.Length;
        if( (_fadeCount) >= containerCount)
        {
            FadeInfo[] temp = _fadeInfos;
            _fadeInfos = new FadeInfo[containerCount*2];
            Array.Copy(temp, 0, _fadeInfos, 0, containerCount);
        }

        //새로운 페이드 정보를 추가한다.
        float startVolume = (busType >= 0 ? GetBusVolume((FModBusType)busType) : Instance.Volume);

        _fadeInfos[_fadeCount++] = new FadeInfo()
        {
            startVolume  = startVolume,
            distance     = ( goalVolume-startVolume),
            duration     = fadeTime,
            durationDiv  = (1f / fadeTime),
            FadeID       = fadeID,
            TargetIns    = Instance,
            pendingKill  = false,
            TargetBusIdx = busType,
            destroyAtCompleted = completeDestroy,
        };

        #endregion
    }

    private void RemoveFadeInfo(int index)
    {
        #region Omit
        if (_fadeInfos == null) return;

        //PendingKill Ready라면, PendingKill상태로 전환.
        if(_fadeState!=FadeState.None){

            _fadeState= FadeState.PendingKill;
            _fadeInfos[index].pendingKill = true;
            return;
        }

        _fadeInfos[index] = _fadeInfos[_fadeCount - 1];
        _fadeCount--;
        #endregion
    }

    private void FadeProgress_internal()
    {
        #region Omit
        float DeltaTime = Time.deltaTime;
        int fadeCount   = _fadeCount;

        /****************************************
         *    모든 페이드 정보를 업데이트한다...
         * *****/
        _fadeState = FadeState.PendingKill_Ready;
        for (int i = 0; i < fadeCount; i++)
        {
            ref FadeInfo info = ref _fadeInfos[i];

            //페이드 대상 인스턴스가 유효하지 않을 경우, PendingKill상태를 적용.
            if (info.TargetIns.IsValid == false && info.TargetBusIdx<0){

                info.pendingKill = true;
                _fadeState = FadeState.PendingKill;
                continue;
            }

            info.duration      -= DeltaTime;
            float fadeTimeRatio = Mathf.Clamp(1f - (info.duration * info.durationDiv), 0f, 1f);
            float finalVolume   = info.startVolume + (fadeTimeRatio * info.distance);



            /*********************************************
             *    볼륨 페이드 대상에 따라서 적절히 적용한다..
             * ******/

            /**버스일 경우...*/
            if (info.TargetBusIdx>0){
                SetBusVolume((FModBusType)info.TargetBusIdx, finalVolume);
            }

            /**인스턴스일 경우...*/
            else info.TargetIns.Volume = finalVolume;



            /*********************************************
             *   페이드 마무리 단계...
             * ******/
            if (fadeTimeRatio < 1f) continue;
            OnEventFadeComplete?.Invoke(info.FadeID, finalVolume);

            /***마무리 단계 파괴 적용.***/
            if (info.destroyAtCompleted && info.TargetBusIdx<0){

                info.TargetIns.Destroy();
            }

            info.pendingKill = true;
            _fadeState = FadeState.PendingKill;
        }



        /************************************************
         *   PendingKill 상태를 처리한다.
         * ****/
        if (_fadeState == FadeState.PendingKill)
        {
            _fadeState = FadeState.None;

            for (int i = 0; i < fadeCount; i++){

                if (!_fadeInfos[i].pendingKill) continue;

                RemoveFadeInfo(i);
                fadeCount--;
                i--;
            }
        }

        #endregion
    }

    private void BGMFadeComplete(int fadeID, float goalVolume)
    {
        #region Omit
        if (fadeID != BGMAutoFadeID ) return;
        if (goalVolume <= 0f) _BGMIns.Destroy();
        if(_NextBGMEvent >=0) PlayBGM((FModBGMEventType)_NextBGMEvent, (FModLocalParamType)_NextBGMParam, _NextBGMParamValue, _NextBGMVolume, _NextBGMStartPosRatio, _NextBGMPosition);
        #endregion
    }



    //=======================================
    /////        Magic Methods           /////
    ///======================================
    private void Awake()
    {
        #region Omit
        if (_Instance == null)
        {
            /*******************************************
             *     초기화 과정을 적용한다....
             * *****/
            _Instance = this;
            DontDestroyOnLoad(gameObject);

            _callbackCount = 0;
            _usedBytes     = 0;
            _callbackInfos = new CallBackInfo[10];
            _callBackTargets.Clear();
            _paramDescs.Clear();

            #if FMOD_Event_ENUM
            OnEventFadeComplete += BGMFadeComplete;


            /*******************************************
             *    모든 버스 목록들을 얻어온다.....
             * *****/
            int busCount = FModReferenceList.BusPaths.Length;
            _BusList = new Bus[busCount];

            for (int i = 0; i < busCount; i++){

                string busName = FModReferenceList.BusPaths[i];
                try { _BusList[i] = FMODUnity.RuntimeManager.GetBus(busName); } catch { continue; }
            }
            #endif

            return;
        }

        Destroy(gameObject);
        #endregion
    }

    private void OnDestroy()
    {
        #region Omit
        //Destroy...
        if (_Instance==this){

            _Instance = null;
            OnEventFadeComplete = null;
        }
        #endregion
    }

    private void Update()
    {
        #region Omit
        /****************************************
         *   처리할 페이드 정보가 있을 경우...
         * *****/
        if (_fadeCount > 0) { 
            FadeProgress_internal();
        }


        /***************************************
         *   처리할 콜백 정보가 있을 경우....
         * *****/
        if (_callbackCount > 0){
            CallbackProgress_internal();
        }
        #endregion
    }

#endif

}