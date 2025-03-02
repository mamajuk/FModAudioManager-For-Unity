
#if UNITY_EDITOR
#if FMOD_Event_ENUM
using UnityEditor;
using UnityEngine;
using System;


/**********************************************************************
 *    FModParameterReference를 직관적으로 수정하기 위한 에디터 확장...
 * ******/
[CanEditMultipleObjects]
[CustomPropertyDrawer(typeof(FModParameterReference))]
public sealed class FModParameterReferenceDrawer : PropertyDrawer
{
    //=====================================
    /////           Fields            /////
    //=====================================
    private SerializedProperty ParamTypeProperty;
    private SerializedProperty ParamValueProperty;
    private SerializedProperty isGlobalProperty;


    /**에디터 데이터 관련...*/
    private const string _EditorSettingsPath = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
    private static FModAudioEditorSettings _EditorSettings;

    /**스타일 관련...*/
    private static GUIStyle _labelStyle;
    private static GUIStyle _labelStyleLight;

    /**파라미터 값 관련...*/
    private float    _min, _max;
    private int      _select = -1;
    private string[] _labels;
    private bool     _init = false;


    //===============================================
    //////          Override methods            /////
    //===============================================

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        /**초기화에 실패했다면, 원래 방식대로 출력한다.*/
        if (GUI_Initialized(property) == false) return;

        /**펼쳐진 상태에서만 하위 내용들을 모조리 표시한다....*/
        position.height = GetBaseHeight();

        if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
        {
            position.y += GetBaseHeight();

            /**모든 프로퍼티들을 표시한다...*/
            GUI_ShowParamType(ref position);

            GUI_ShowParamValue(ref position);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + (property.isExpanded ? 50f : 0f);
    }



    //======================================
    /////         GUI methods          /////
    //======================================
    private bool GUI_Initialized(SerializedProperty property)
    {
        #region Omit
        /******************************************
         *   모든 프로퍼티들을 초기화한다...
         * ***/
        ParamTypeProperty  = property.FindPropertyRelative("_paramType");
        ParamValueProperty = property.FindPropertyRelative("_paramValue");
        isGlobalProperty   = property.FindPropertyRelative("_isGlobal");

        /**에디터 세팅 초기화...*/
        if (_EditorSettings == null){

            _EditorSettings = AssetDatabase.LoadAssetAtPath<FModAudioEditorSettings>(_EditorSettingsPath);
        }

        /**스타일 초기화...*/
        if (_labelStyle == null){

            _labelStyle = new GUIStyle(EditorStyles.foldout);
            _labelStyle.normal.textColor = Color.white;
            _labelStyle.fontStyle = FontStyle.Bold;
            _labelStyle.fontSize = 12;
        }

        if (_labelStyleLight == null){

            _labelStyleLight = new GUIStyle(EditorStyles.boldLabel);
            _labelStyleLight.normal.textColor = Color.black;
            _labelStyleLight.fontSize = 12;
        }

        /**룩업테이블 초기화....*/
        if(_init==false){
            GUI_SetParameter();
            _init = true;
        }

        return (_EditorSettings != null && ParamTypeProperty != null && ParamValueProperty != null && isGlobalProperty != null);
        #endregion
    }

    private void GUI_ShowPropertyRect(ref Rect header, SerializedProperty property, float space = 3f)
    {
        #region Omit
        bool isBlack = EditorGUIUtility.isProSkin;
        GUIStyle style = (isBlack ? _labelStyle : _labelStyleLight);
        Color color = (isBlack ? new Color(.3f, .3f, .3f) : new Color(0.7254f, 0.7254f, 0.7254f));


        /************************************
         *   프로퍼티 이름을 출력한다...
         * ***/
        EditorGUI.LabelField(header, property.displayName, style);

        header.x += 10f;
        header.y += 20f;
        header.width -= 40f;

        #endregion
    }

    private void GUI_ShowParamType(ref Rect header, float space = 3f)
    {
        #region Omit
#if FMOD_Event_ENUM
        /**************************************
         *   파라미터 타입을 표시한다...
         * ***/
        Rect rect = header;

        using (var scope = new EditorGUI.ChangeCheckScope()){

            bool isGlobal = isGlobalProperty.boolValue;
            int  value    = ParamTypeProperty.intValue;
            System.Enum result;

            rect.width *= .8f;

            /**글로벌 파라미터일 경우...*/
            if (isGlobal)
            {
                FModGlobalParamType global = (FModGlobalParamType)value;
                result = EditorGUI.EnumPopup(rect, "●-ParamType", global);
            }

            /**로컬 파라미터일 경우...*/
            else{

                FModLocalParamType local = (FModLocalParamType)value;
                result = EditorGUI.EnumPopup(rect, "●-ParamType", local);
            }

            /**값이 변경되었을 경우 갱신한다...*/
            if (scope.changed){
                ParamTypeProperty.intValue = Convert.ToInt32(result);
                GUI_SetParameter(true);
            }
        }

        /*****************************************
         *   글로벌인지에 대한 여부를 표시한다...
         * ***/
        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            rect.x += rect.width + 5f;
            bool value = EditorGUI.ToggleLeft(rect, "Is Global", isGlobalProperty.boolValue);

            /**값이 변경되었다면 갱신한다...*/
            if (scope.changed){

                isGlobalProperty.boolValue = value;
                ParamTypeProperty.intValue = 0;
                ParamValueProperty.floatValue = 0f;
            }
        }


        header.y += (20f + space);
#endif
        #endregion
    }

    private void GUI_ShowParamValue(ref Rect header, float space = 3f)
    {
        #region Omit
        Rect rect = header;
        //rect.x += 20f;

        using (var scope = new EditorGUI.ChangeCheckScope()){

            /*******************************************
             *   파라미터가 선택되지 않았을 경우...
             * ***/
            if (ParamTypeProperty.intValue <= 0)
            {
                EditorGUI.TextField(rect, "●-Param Value", "(No Value)");
                return;
            }

            /******************************************
             *   레이블이 존재하지 않는 파라미터일 경우...
             * ***/
            if (_labels == null)
            {
                float value = EditorGUI.Slider(rect, "●-Param Value", ParamValueProperty.floatValue, _min, _max);

                /**값이 변경되었다면 갱신.*/
                if (scope.changed){

                    ParamValueProperty.floatValue = value;
                }

                return;
            }


            /******************************************
             *   레이블이 존재하는 파라미터일 경우...
             * ***/
            int selected = EditorGUI.Popup(rect, "●-Param Value", _select, _labels);

            /**값이 변경되었다면 갱신...*/
            if (_select != selected)
            {
                _select = selected;
                ParamValueProperty.floatValue = (float)selected;
            }
        }


        #endregion
    }

    private void GUI_SetParameter(bool forceApply=false)
    {
        #region Omit
        int index = ParamTypeProperty.intValue;
        if (index <= 0) return;

        index -= 1;
        FModParamDesc desc = _EditorSettings.ParamDescList[index];
        _min = desc.Min;
        _max = desc.Max;
        _select = 0;

        /**라벨이 존재하지 않을경우, 탈출한다...*/
        if (desc.LableCount == 0){

            _labels = null;
            return;
        }



        /*************************************************
         *   해당 파라미터의 레이블의 룩업테이블을 생성한다...
         * ***/

        /**레이블의 시작 인덱스를 구한다...*/
        int labelBegin = 0;
        for (int i = 0; i < index; i++){

            labelBegin += _EditorSettings.ParamDescList[i].LableCount;
        }

        /**해당 레이블의 배열을 생성한다...*/
        if (_labels == null || _labels.Length != desc.LableCount){

            _labels = new string[desc.LableCount];
        }

        for (int i = 0; i < desc.LableCount; i++){
            _labels[i] = _EditorSettings.ParamLableList[labelBegin + i];
        }
        _select = (int)ParamValueProperty.floatValue;
        #endregion
    }



    //===============================================
    /////            Utility Methods             ////
    ///==============================================
    private float GetBaseHeight()
    {
        return GUI.skin.textField.CalcSize(GUIContent.none).y;
    }

}
#endif
#endif