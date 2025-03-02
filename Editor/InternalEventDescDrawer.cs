
#if UNITY_EDITOR
#if FMOD_Event_ENUM
using FMODUnity;
using UnityEditor;
using UnityEngine;
using static FMODEventPresetPlayer;


/**********************************************************************************
 *    FMODEventPresetPlayer의 InternalEventDesc를 커스텀 하기 위한 PropertyDrawer.
 * ******/
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(FMODEventPresetPlayer.InternalEventDesc))]
public class InternalEventDescDrawer : PropertyDrawer
{
    //=================================================
    ///////             Fields..                ///////
    ///================================================
    private Rect _initRect;

    private SerializedProperty _EventRefProperty;
    private SerializedProperty _ParamRefProperty;

    private SerializedProperty _PlayApplyTimingProperty;
    private SerializedProperty _StopApplyTimingProperty;

    private SerializedProperty _IsOneShotProperty;
    private SerializedProperty _EventPlayType;
    private SerializedProperty _VolumeProperty;
    private SerializedProperty _StartTimelinePositionProperty;

    private SerializedProperty _PositionProperty;
    private SerializedProperty _EventMinDistanceProperty;
    private SerializedProperty _EventMaxDistanceProperty;
    private SerializedProperty _OverrideDistanceProperty;

    private static readonly string[] _methodTable = new string[]
    {
       "Instance.Play()", "PlayOneShotSFX()", "PlayBGM()"
    };



    //====================================================
    ///////           Override methods..           ///////
    ///===================================================
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        /********************************************************
         *    해당 객체가 펼쳐진 상태에서만 내용을 표시한다...
         * ******/
        if (GUI_Initialized(property) == false)
        {
            return;
        }

        _initRect = position;
        position.height = GetBaseHeight();

        EventReference selectedEventRef = _EventRefProperty.GetEventReference();
        string[] eventNames = selectedEventRef.Path.Split('/');
        string   eventName  = eventNames[eventNames.Length - 1];

        if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, $"Event Presets ({property.displayName.Replace("Element ", "")}) - {eventName}"))
        {
            GUI_ShowEventDesc(ref position, property, label);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        #region Omit
        if (GUI_Initialized(property) == false) return 0f;

        float eventRefHeight = EditorGUI.GetPropertyHeight(_EventRefProperty) + 10f;
        float paramRefHeight = EditorGUI.GetPropertyHeight(_ParamRefProperty) + 10f;
        float playHeight     = GetBaseHeight() * (_IsOneShotProperty.isExpanded ? 7f : 1f) + 10f;
        float timingHeight   = GetBaseHeight() * (_PlayApplyTimingProperty.isExpanded ? 4f : 1f) + 10f;
        float dddHeight      = GetBaseHeight() * (_PositionProperty.isExpanded ? 8f : 1f) + 10f;
        float total          = (eventRefHeight + paramRefHeight + timingHeight + dddHeight + playHeight + 10f);

        return GetBaseHeight() + (property.isExpanded ? total : 0f);
        #endregion
    }



    //===============================================
    ///////          GUI methods...           ///////
    //===============================================
    private bool GUI_Initialized(SerializedProperty parentProperty)
    {
        #region Omit
        if (parentProperty == null) return false;


        /************************************************
         *    필요한 값들을 모두 초기화한다....
         * *******/
        _EventRefProperty        = parentProperty.FindPropertyRelative("EventRef");
        _ParamRefProperty        = parentProperty.FindPropertyRelative("ParamRef");
        _PlayApplyTimingProperty = parentProperty.FindPropertyRelative("PlayApplyTiming");

        _StopApplyTimingProperty       = parentProperty.FindPropertyRelative("StopApplyTiming");
        _IsOneShotProperty             = parentProperty.FindPropertyRelative("IsOneShot");
        _VolumeProperty                = parentProperty.FindPropertyRelative("Volume");
        _StartTimelinePositionProperty = parentProperty.FindPropertyRelative("StartTimelinePositionRatio");
        _EventPlayType = parentProperty.FindPropertyRelative("EventPlayType");

        _PositionProperty         = parentProperty.FindPropertyRelative("EventPos");
        _EventMaxDistanceProperty = parentProperty.FindPropertyRelative("EventMaxDistance");
        _EventMinDistanceProperty = parentProperty.FindPropertyRelative("EventMinDistance");
        _OverrideDistanceProperty = parentProperty.FindPropertyRelative("OverrideDistance");


        return (_EventRefProperty != null && _ParamRefProperty != null) &&
               (_PlayApplyTimingProperty != null && _StopApplyTimingProperty != null) &&
               (_IsOneShotProperty != null && _VolumeProperty != null && _StartTimelinePositionProperty != null && _EventPlayType != null) &&
               (_PositionProperty != null && _EventMinDistanceProperty != null && _EventMaxDistanceProperty != null && _OverrideDistanceProperty != null);
        #endregion
    }

    private void GUI_ShowEventDesc(ref Rect position, SerializedProperty property, GUIContent label)
    {
        #region Omit
        /***************************************************
         *    프로퍼티 필드의 크기 및 위치를 초기화한다....
         * *****/
        position.x += 25f;
        position.y += GetBaseHeight();
        position.width -= 25f;


        /******************************************************
         *    EventReference에 대한 프로퍼티 필드를 표시하고, 
         *    관련 정보를 불러온다....
         * *****/
        EditorGUI.PropertyField(position, _EventRefProperty);
        position.y += EditorGUI.GetPropertyHeight(_EventRefProperty);
        GUI_DrawLine(ref position);

        EventReference eventRef = _EventRefProperty.GetEventReference();
        EditorEventRef eEventRef = EventManager.EventFromGUID(eventRef.Guid);


        /********************************************************
         *    ParamReference에 대한 프로퍼티를 필드를 표시한다...
         * *******/
        EditorGUI.PropertyField(position, _ParamRefProperty);
        position.y += EditorGUI.GetPropertyHeight(_ParamRefProperty);
        GUI_DrawLine(ref position);


        /*************************************************
         *    Play에 관련된 설정창을 표시한다....
         * *****/
        if (_IsOneShotProperty.isExpanded = EditorGUI.Foldout(position, _IsOneShotProperty.isExpanded, "Play Settings"))
        {
            position.x += 25f;
            position.y += 25f;
            position.width -= 25f;

            //Volume...
            _VolumeProperty.floatValue = EditorGUI.Slider(position, "Volume", _VolumeProperty.floatValue, 0f, 10f);
            position.y += 25f;

            //TimelinePositionRatio...
            _StartTimelinePositionProperty.floatValue = EditorGUI.Slider(position, "TimelinePosition Ratio", _StartTimelinePositionProperty.floatValue, 0f, 1f);
            position.y += 25f;

            //Play Timing...
            _PlayApplyTimingProperty.intValue = (int)(FModEventApplyTiming)EditorGUI.EnumFlagsField(position, "Auto Play Timing", (FModEventApplyTiming)_PlayApplyTimingProperty.intValue);
            position.y += 25f;

            //Play Type...
            _EventPlayType.intValue = EditorGUI.Popup(position, "Auto Play Method", _EventPlayType.intValue, _methodTable);
            position.x -= 25f;
            position.width += 25f;
        }

        position.y += 25f;
        GUI_DrawLine(ref position);


        /********************************************
         *    Stop에 관련된 설정창을 표시한다...
         * *****/
        using (var disableScope = new EditorGUI.DisabledGroupScope(_EventPlayType.intValue == (int)FModEventPlayMethodType.PlayOneShotSFX))
        {
            if (_PlayApplyTimingProperty.isExpanded = EditorGUI.Foldout(position, _PlayApplyTimingProperty.isExpanded, "Stop Settings"))
            {
                position.x += 25f;
                position.y += 25f;
                position.width -= 25f;

                //IsOneShot...
                _IsOneShotProperty.boolValue = EditorGUI.Toggle(position, "Auto DestroyAtStop", _IsOneShotProperty.boolValue);
                position.y += 25f;

                //Stop Timing...
                _StopApplyTimingProperty.intValue = (int)(FModEventApplyTiming)EditorGUI.EnumFlagsField(position, "Auto Stop Timing", (FModEventApplyTiming)_StopApplyTimingProperty.intValue);
                position.x -= 25f;
                position.width += 25f;
            }
        }

        position.y += 25f;
        GUI_DrawLine(ref position);


        /*************************************************
         *   3D Event일 경우, 3D 관련 설정창을 표시한다...
         * *****/
        using (var disableScope = new EditorGUI.DisabledGroupScope(!(eEventRef != null && eEventRef.Is3D)))
        {
            /**3D....**/
            if (_PositionProperty.isExpanded = EditorGUI.Foldout(position, _PositionProperty.isExpanded, "3D"))
            {
                position.x += 25f;
                position.y += 25f;
                position.width -= 25f;

                //Position...
                _PositionProperty.vector3Value = EditorGUI.Vector3Field(position, "position", _PositionProperty.vector3Value);
                position.y += 25f;

                //Override Distance
                _OverrideDistanceProperty.boolValue = EditorGUI.Toggle(position, "Override Distance", _OverrideDistanceProperty.boolValue);
                position.y += 25f;

                //Min/Max Distance
                using (var scope = new EditorGUI.DisabledGroupScope(!_OverrideDistanceProperty.boolValue))
                {
                    position.x += 25f;
                    position.width -= 25f;

                    _EventMinDistanceProperty.floatValue = EditorGUI.FloatField(position, "Min", _EventMinDistanceProperty.floatValue);
                    position.y += 25f;

                    _EventMaxDistanceProperty.floatValue = EditorGUI.FloatField(position, "Max", _EventMaxDistanceProperty.floatValue);
                }

                position.width += 50f;
                position.x -= 50f;
            }
        }
        #endregion
    }

    private void GUI_DrawLine(ref Rect position, float space = 5f, float subOffset = 0f)
    {
        #region Omit
        position.y += space;

        Handles.color = Color.gray;
        Handles.DrawLine(
            new Vector2(position.x - subOffset, position.y),
            new Vector2(_initRect.width + 30f - subOffset, position.y)
        );

        position.y += space;
        #endregion
    }


    //=====================================================
    ///////           Utility methods..             ///////
    /////==================================================
    private float GetBaseHeight()
    {
        return GUI.skin.label.CalcSize(GUIContent.none).y;
    }
}
#endif
#endif