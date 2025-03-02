using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/**************************************************************
 *   FMOD Event�� ���� �������� �ۼ��ϰ�, ������ ��ȣ�� �����ϰ�
 *   Event�� ����� �� �ֵ��� �����ִ� ������Ʈ�Դϴ�...
 * ***/
#if FMOD_Event_ENUM
[AddComponentMenu("FMOD Studio/FMODEventPresetPlayer")]
public sealed class FMODEventPresetPlayer : MonoBehaviour
{
    #region Define
    public enum FModEventApplyTiming
    {
        None            = 0,
        Start           = 1,
        CollisionEnter  = 2, 
        CollisionExit   = 4,
        TriggerEnter    = 8, 
        TriggerExit     = 16,
        Destroy         = 32,
        ALL             = (Start|CollisionEnter|CollisionExit|TriggerEnter|TriggerExit)
    }

    public enum FModEventPlayMethodType
    {
        Instance_Play,
        PlayOneShotSFX,
        PlayBGM
    }

    [System.Serializable]
    public struct InternalEventDesc
    {
        public FMODUnity.EventReference EventRef;
        public FModParameterReference   ParamRef;
        public FModEventApplyTiming     PlayApplyTiming;
        public FModEventApplyTiming     StopApplyTiming;
        public FModEventPlayMethodType  EventPlayType;
        public Vector3                  EventPos;
        public bool                     IsOneShot;
        public bool                     OverrideDistance;
        public float                    EventMinDistance;
        public float                    EventMaxDistance;
        public float                    Volume;
        public float                    StartTimelinePositionRatio;

        [System.NonSerialized]
        public FModEventInstance        Instance;
    }
    #endregion

    //=================================================
    /////          Property and Fields             ////
    //=================================================
    [SerializeField] public InternalEventDesc[] EventPresets = new InternalEventDesc[0];



    //===========================================================
    /////               Magic and Core methods             //////
    //===========================================================
    private void Start()
    {
        PlayStop_Internal(FModEventApplyTiming.Start);
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayStop_Internal(FModEventApplyTiming.TriggerEnter);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayStop_Internal(FModEventApplyTiming.TriggerExit);
    }

    private void OnCollisionEnter(Collision collision)
    {
        PlayStop_Internal(FModEventApplyTiming.CollisionEnter);
    }

    private void OnCollisionExit(Collision collision)
    {
        PlayStop_Internal(FModEventApplyTiming.CollisionExit);
    }

    private void OnDestroy()
    {
        PlayStop_Internal(FModEventApplyTiming.Destroy);
    }

    private void PlayStop_Internal(FModEventApplyTiming timing, int filter = -1)
    {
        #region Omit
        int descCount = EventPresets.Length;

        for (int i=0; i<descCount; i++)
        {
            //Ư�� �ν��Ͻ��� ����� ���, ���Ϳ� ��ġ���� �ʴ� �ε����� ��ŵ�Ѵ�...
            if(filter>=0 && i!=filter){
                return;
            }

            ref InternalEventDesc desc = ref EventPresets[i];

            bool ContainPlayTiming = ((int)desc.PlayApplyTiming & (int)timing)>0;
            bool ContainStopTiming = ((int)desc.StopApplyTiming & (int)timing)>0;


            /**********************************************
             *    ���� Ÿ�ְ̹� ��ġ�ϴ°�?..
             * ****/
            if(ContainStopTiming){

                /**PlayBGM() �޼ҵ带 ����ϴ°�?**/
                if(desc.EventPlayType == FModEventPlayMethodType.PlayBGM)
                {
                    FModAudioManager.StopBGM();
                }

                /**Instance.Play() �޼ҵ带 ����ϴ°�?**/
                if(desc.EventPlayType==FModEventPlayMethodType.Instance_Play && desc.Instance.IsValid)
                {
                    desc.Instance.Stop();
                }
            }


            /**********************************************
             *    ���� Ÿ�ְ̹� ��ġ�ϴ°�?..
             * ****/
            if (ContainPlayTiming){

                /*PlayOneShotSFX() �޼ҵ带 ����ϴ°�?**/
                if (desc.EventPlayType == FModEventPlayMethodType.PlayOneShotSFX)
                {
                    FModAudioManager.PlayOneShotSFX(desc.EventRef, desc.EventPos, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventMinDistance, desc.EventMaxDistance);
                }

                /**PlayBGM() �޼ҵ带 ����ϴ°�?*/
                else if (desc.EventPlayType == FModEventPlayMethodType.PlayBGM)
                {
                    FModAudioManager.PlayBGM(desc.EventRef, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventPos);
                }
                else Play_Progress_Internal(ref desc);
            }

            //�ı��Ǵ� Ÿ�̹��̶�� ��ȿ�� �ν��Ͻ��� �ı��Ѵ�...
            if(timing==FModEventApplyTiming.Destroy && desc.Instance.IsValid)
            {
                desc.Instance.Destroy();
            }
        }

        #endregion
    }

    private void Play_Progress_Internal(ref InternalEventDesc desc)
    {
        #region Omit
        if (desc.Instance.IsValid) desc.Instance.Destroy();

        /*********************************************
         *    ��ȿ�� EventInstance�� ���ٸ� �����Ѵ�...
         * *****/
        desc.Instance = FModAudioManager.CreateInstance(desc.EventRef);
        desc.Instance.Position3D = desc.EventPos;
        desc.Instance.Volume = desc.Volume;
        desc.Instance.TimelinePositionRatio = desc.StartTimelinePositionRatio;

        //Min-Max Distance�� �����°�?
        if (desc.OverrideDistance){

            desc.Instance.Set3DDistance(desc.EventMinDistance, desc.EventMaxDistance);
        }

        //�Ķ���͸� �����Ѵ�....
        if (desc.ParamRef.IsValid){

            desc.Instance.SetParameter(desc.ParamRef);
        }

        desc.Instance.Play();

        //���尡 �����Ǹ� �ڵ����� �ı��Ǵ°�?
        if (desc.IsOneShot){

            desc.Instance.Destroy(true);
        }
        #endregion
    }



    //=======================================
    /////        Public methods         /////
    //=======================================
    public void PlayOneShotSFX(int preset_index)
    {
        #region Omit
        if (EventPresets == null || preset_index<0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        FModAudioManager.PlayOneShotSFX( desc.EventRef, desc.EventPos, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventMinDistance, desc.EventMaxDistance );
        #endregion
    }

    public void PlayBGM( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        FModAudioManager.PlayBGM(desc.EventRef, desc.ParamRef, desc.Volume, desc.StartTimelinePositionRatio, desc.EventPos);
        #endregion
    }

    public void PlayInstance( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;
        Play_Progress_Internal(ref EventPresets[preset_index]);
        #endregion
    }

    public void StopInstance( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        if(desc.Instance.IsValid){

            desc.Instance.Stop();
        }
        #endregion
    }

    public void ResumeInstance( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        if (desc.Instance.IsValid){
            desc.Instance.Resume();
        }
        #endregion
    }

    public void PauseInstance( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        if (desc.Instance.IsValid){
            desc.Instance.Pause();
        }
        #endregion
    }

    public void DestroyInstance( int preset_index )
    {
        #region Omit
        if (EventPresets == null || preset_index < 0 || EventPresets.Length <= preset_index) return;

        ref InternalEventDesc desc = ref EventPresets[preset_index];
        if (desc.Instance.IsValid){

            desc.Instance.Destroy();
        }
        #endregion
    }
}
#endif
