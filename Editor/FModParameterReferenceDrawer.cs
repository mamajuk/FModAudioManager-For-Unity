
#if UNITY_EDITOR
#if FMOD_Event_ENUM
using UnityEditor;
using UnityEngine;
using System;


/**********************************************************************
 *    FModParameterReference�� ���������� �����ϱ� ���� ������ Ȯ��...
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


    /**������ ������ ����...*/
    private const string _EditorSettingsPath = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
    private static FModAudioEditorSettings _EditorSettings;

    /**��Ÿ�� ����...*/
    private static GUIStyle _labelStyle;
    private static GUIStyle _labelStyleLight;

    /**�Ķ���� �� ����...*/
    private float    _min, _max;
    private int      _select = -1;
    private string[] _labels;
    private bool     _init = false;


    //===============================================
    //////          Override methods            /////
    //===============================================

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        /**�ʱ�ȭ�� �����ߴٸ�, ���� ��Ĵ�� ����Ѵ�.*/
        if (GUI_Initialized(property) == false) return;

        /**������ ���¿����� ���� ������� ������ ǥ���Ѵ�....*/
        position.height = GetBaseHeight();

        if (property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, property.displayName))
        {
            position.y += GetBaseHeight();

            /**��� ������Ƽ���� ǥ���Ѵ�...*/
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
         *   ��� ������Ƽ���� �ʱ�ȭ�Ѵ�...
         * ***/
        ParamTypeProperty  = property.FindPropertyRelative("_paramType");
        ParamValueProperty = property.FindPropertyRelative("_paramValue");
        isGlobalProperty   = property.FindPropertyRelative("_isGlobal");

        /**������ ���� �ʱ�ȭ...*/
        if (_EditorSettings == null){

            _EditorSettings = AssetDatabase.LoadAssetAtPath<FModAudioEditorSettings>(_EditorSettingsPath);
        }

        /**��Ÿ�� �ʱ�ȭ...*/
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

        /**������̺� �ʱ�ȭ....*/
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
         *   ������Ƽ �̸��� ����Ѵ�...
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
         *   �Ķ���� Ÿ���� ǥ���Ѵ�...
         * ***/
        Rect rect = header;

        using (var scope = new EditorGUI.ChangeCheckScope()){

            bool isGlobal = isGlobalProperty.boolValue;
            int  value    = ParamTypeProperty.intValue;
            System.Enum result;

            rect.width *= .8f;

            /**�۷ι� �Ķ������ ���...*/
            if (isGlobal)
            {
                FModGlobalParamType global = (FModGlobalParamType)value;
                result = EditorGUI.EnumPopup(rect, "��-ParamType", global);
            }

            /**���� �Ķ������ ���...*/
            else{

                FModLocalParamType local = (FModLocalParamType)value;
                result = EditorGUI.EnumPopup(rect, "��-ParamType", local);
            }

            /**���� ����Ǿ��� ��� �����Ѵ�...*/
            if (scope.changed){
                ParamTypeProperty.intValue = Convert.ToInt32(result);
                GUI_SetParameter(true);
            }
        }

        /*****************************************
         *   �۷ι������� ���� ���θ� ǥ���Ѵ�...
         * ***/
        using (var scope = new EditorGUI.ChangeCheckScope())
        {
            rect.x += rect.width + 5f;
            bool value = EditorGUI.ToggleLeft(rect, "Is Global", isGlobalProperty.boolValue);

            /**���� ����Ǿ��ٸ� �����Ѵ�...*/
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
             *   �Ķ���Ͱ� ���õ��� �ʾ��� ���...
             * ***/
            if (ParamTypeProperty.intValue <= 0)
            {
                EditorGUI.TextField(rect, "��-Param Value", "(No Value)");
                return;
            }

            /******************************************
             *   ���̺��� �������� �ʴ� �Ķ������ ���...
             * ***/
            if (_labels == null)
            {
                float value = EditorGUI.Slider(rect, "��-Param Value", ParamValueProperty.floatValue, _min, _max);

                /**���� ����Ǿ��ٸ� ����.*/
                if (scope.changed){

                    ParamValueProperty.floatValue = value;
                }

                return;
            }


            /******************************************
             *   ���̺��� �����ϴ� �Ķ������ ���...
             * ***/
            int selected = EditorGUI.Popup(rect, "��-Param Value", _select, _labels);

            /**���� ����Ǿ��ٸ� ����...*/
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

        /**���� �������� �������, Ż���Ѵ�...*/
        if (desc.LableCount == 0){

            _labels = null;
            return;
        }



        /*************************************************
         *   �ش� �Ķ������ ���̺��� ������̺��� �����Ѵ�...
         * ***/

        /**���̺��� ���� �ε����� ���Ѵ�...*/
        int labelBegin = 0;
        for (int i = 0; i < index; i++){

            labelBegin += _EditorSettings.ParamDescList[i].LableCount;
        }

        /**�ش� ���̺��� �迭�� �����Ѵ�...*/
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