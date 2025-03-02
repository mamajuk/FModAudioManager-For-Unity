#if UNITY_EDITOR
#if FMOD_Event_ENUM
using UnityEditor;
using UnityEngine;
using FMODUnity;


/*********************************************************
 *     FModEventPlayerEditor�� Ȯ���ϱ� ���� Editor....
 * ******/
[CustomEditor(typeof(FMODEventPresetPlayer))]
public class FMODEventPresetPlayerEditor : Editor
{
    private SerializedProperty _EventDescs;
    private int                _prevArraySize = 0;

    public void OnSceneGUI()
    {
        #region Omit
        /********************************************
         *    �ʱ�ȭ�� �����Ѵ�....
         * *****/
        if ((_EventDescs = serializedObject.FindProperty("EventPresets")) != null)
        {
            _prevArraySize = _EventDescs.arraySize;
        }

        if (_EventDescs == null) return;


        /****************************************************************
         *    ���� Min-Max ������ �����ϰ� �ִ� �������� �ڵ��� ǥ���Ѵ�...
         * ******/
        for (int i = 0; i < _prevArraySize; i++){

            FMODEventPresetPlayer player   = (target as FMODEventPresetPlayer);
            SerializedProperty element     = _EventDescs.GetArrayElementAtIndex(i);
            SerializedProperty position    = element.FindPropertyRelative("EventPos");
            SerializedProperty isOverride  = element.FindPropertyRelative("OverrideDistance");
            SerializedProperty min         = element.FindPropertyRelative("EventMinDistance");
            SerializedProperty max         = element.FindPropertyRelative("EventMaxDistance");
            SerializedProperty instance    = element.FindPropertyRelative("Instance");

            /**�ڵ��� ǥ�õ��� �ʴ°�..?*/
            if (!element.isExpanded || !position.isExpanded || isOverride.boolValue == false)
            {
                continue;
            }


            using (var scope = new EditorGUI.ChangeCheckScope())
            {
                position.vector3Value = Handles.DoPositionHandle(position.vector3Value, Quaternion.identity);

                Handles.color = Color.red;
                min.floatValue = Handles.RadiusHandle(Quaternion.identity, position.vector3Value, min.floatValue);

                Handles.color = Color.yellow;
                max.floatValue = Handles.RadiusHandle(Quaternion.identity, position.vector3Value, max.floatValue);


                /*************************************************************************
                 * 3D Ʈ�������� ����Ǿ���, ������ EventInstance�� ��ȿ�ϸ� ���� �����Ѵ�...
                 * *****/
                if (scope.changed){
                    serializedObject.ApplyModifiedProperties();
                }
            }


            /*************************************************************************
             * ������ EventInstance�� ��ȿ�ϸ� ���� �����Ѵ�...
             * *****/
            if (player!=null){
                FModEventInstance ins = player.GetPresetByIndex(i).Instance;

                if (ins.IsValid)
                {
                    ins.Position3D = position.vector3Value;
                    ins.Set3DDistance(min.floatValue, max.floatValue);
                }
            }
        }
        #endregion
    }

    public override void OnInspectorGUI()
    {
        #region Omit
        /********************************************
         *    �ʱ�ȭ�� �����Ѵ�....
         * *****/
        if ((_EventDescs = serializedObject.FindProperty("EventPresets")) != null){
            _prevArraySize = _EventDescs.arraySize;
        }

        if (_EventDescs == null) return;


        /*********************************************
         *    ��ȭ�� �����Ѵ�....
         * *****/
        EditorGUILayout.HelpBox("You can create presets for FMOD events and play them conveniently by either providing the " +
                                "preset number to the method offered by FModEventPresetPlayer or using the Auto Play option.", MessageType.Info);

        EditorGUILayout.PropertyField(_EventDescs);

        //���ο� Event Desc�� �߰��Ǿ��ٸ� �ʱⰪ�� �ִ´�...
        if (_EventDescs.arraySize > _prevArraySize)
        {
            SerializedProperty lastElement  = _EventDescs.GetArrayElementAtIndex(_EventDescs.arraySize - 1);
            SerializedProperty volume       = lastElement.FindPropertyRelative("Volume");
            SerializedProperty position     = lastElement.FindPropertyRelative("EventPos");
            SerializedProperty min          = lastElement.FindPropertyRelative("EventMinDistance");
            SerializedProperty max          = lastElement.FindPropertyRelative("EventMaxDistance");
            SerializedProperty eventRef     = lastElement.FindPropertyRelative("EventRef");
            SerializedProperty playTiming   = lastElement.FindPropertyRelative("PlayApplyTiming");
            SerializedProperty stopTiming   = lastElement.FindPropertyRelative("StopApplyTiming");
            SerializedProperty IsOneShot    = lastElement.FindPropertyRelative("IsOneShot");
            SerializedProperty Override     = lastElement.FindPropertyRelative("OverrideDistance");
            SerializedProperty minDistance  = lastElement.FindPropertyRelative("EventMinDistance");
            SerializedProperty maxDistance  = lastElement.FindPropertyRelative("EventMaxDistance");
            SerializedProperty timeRatio    = lastElement.FindPropertyRelative("StartTimelinePositionRatio");
            SerializedProperty paramRef     = lastElement.FindPropertyRelative("ParamRef");
            SerializedProperty paramType    = paramRef.FindPropertyRelative("_paramType");
            SerializedProperty paramValue   = paramRef.FindPropertyRelative("_paramValue");
            SerializedProperty paramGlobal  = paramRef.FindPropertyRelative("_isGlobal");
            SerializedProperty methodType   = lastElement.FindPropertyRelative("EventPlayType");

            volume.floatValue       = 1f;
            min.floatValue          = 1f;
            max.floatValue          = 20f;
            playTiming.intValue     = 0;
            stopTiming.intValue     = 0;
            IsOneShot.boolValue     = false;
            Override.boolValue      = false;
            minDistance.floatValue  = 1f;
            maxDistance.floatValue  = 20f;
            timeRatio.floatValue    = 0f;
            paramType.intValue      = 0;
            paramValue.floatValue   = 0f;
            paramGlobal.boolValue   = false;
            position.vector3Value   = ((FMODEventPresetPlayer)target).transform.position;
            methodType.intValue     = 0;
            eventRef.SetEventReference(new FMOD.GUID(), "");

            _prevArraySize = _EventDescs.arraySize;
        }

        serializedObject.ApplyModifiedProperties();
        #endregion
    }
}
#endif
#endif