#if UNITY_EDITOR
using FMODUnity;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml;
using UnityEditor.AnimatedValues;
using UnityEditor;
using UnityEngine.Events;
using UnityEngine;


/*******************************************************************************
 *    FModAudioManager 에서 사용할 열거형과 구조체를 생성하는 EditorWindow....
 * *****/
public sealed class FModAudioManagerWindow : EditorWindow
{
    private struct ParamFolderInfo
    {
        public string name;
        public string id;
        public string output;
    }

    private enum BankLoadType
    {
        Load_At_Initialized,
        Not_Load
    }

    //=====================================
    ////            Fields           ///// 
    //====================================

    /*************************************
     *   Editor Data Path String...
     * **/
    private const string _DataScriptPath         = "Assets/Plugins/FMOD/src/FMODAudioManagerDefine.cs";
    private const string _EditorSettingsPath     = "Assets/Plugins/FMOD/Resources/FModAudioEditorSettings.asset";
    private const string _StudioSettingsPath     = "Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset";
    private const string _GroupFolderPath        = "Metadata/Group";
    private const string _PresetFolderPath       = "Metadata/ParameterPreset";
    private const string _PresetFolderFolderPath = "Metadata/ParameterPresetFolder";
    private const string _ScriptDefine           = "FMOD_Event_ENUM";
    private const string _EditorVersion          = "v1.250302";

    private const string _EventRootPath = "event:/";
    private const string _BusRootPath   = "bus:/";
    private const string _BankRootPath  = "bank:/";
    private const string _ParamRootPath = "param:/";

    /*************************************
     *  Texture Path and reference
     ***/
    private static Texture _BannerWhiteTex;
    private const string   _BannerWhitePath = "Assets/Plugins/FMOD/images/FMODLogoWhite.png";

    private static Texture _BannerBlackTex;
    private const string   _BannerBlackPath = "Assets/Plugins/FMOD/images/FMODLogoBlack.png";

    private static Texture _StudioIconTex;
    private const string   _StudioIconPath = "Assets/Plugins/FMOD/images/StudioIcon.png";

    private static Texture _SearchIconTex;
    private const string   _SearchIconPath = "Assets/Plugins/FMOD/images/SearchIconBlack.png";

    private static Texture _BankIconTex;
    private const string   _BankIconPath = "Assets/Plugins/FMOD/images/BankIcon.png";

    private static Texture _AddIconTex;
    private const string   _AddIconPath = "Assets/Plugins/FMOD/images/AddIcon.png";

    private static Texture _XYellowIconTex;
    private const string   _XYellowIconPath = "Assets/Plugins/FMOD/images/CrossYellow.png";

    private static Texture _DeleteIconTex;
    private const string   _DeleteIconPath = "Assets/Plugins/FMOD/images/Delete.png";

    private static Texture _NotFoundIconTex;
    private const string   _NotFoundIconPath = "Assets/Plugins/FMOD/images/NotFound.png";

    private static Texture _ParamLabelIconTex;
    private const string   _ParamLabelIconPath = "Assets/Plugins/FMOD/images/LabeledParameterIcon.png";


    /****************************************
     *   Editor ScriptableObject Fields...
     ***/
    private static FModAudioEditorSettings _EditorSettings;
    private static FMODUnity.Settings      _StudioSettings;


    /******************************************
     *   Editor GUI Fields...
     * **/
    private Regex         _regex     = new Regex(@"[^a-zA-Z0-9_]");
    private StringBuilder _builder   = new StringBuilder();
    private bool          _refresh   = false;
    private Vector2       _Scrollpos = Vector2.zero;

    /** Categorys... *************************/
    private static readonly string[] _EventGroups = new string[] { "BGM", "SFX", "NoGroup" };
    private static readonly string[] _ParamGroups = new string[] { "Global Parameters", "Local Parameters" };
    private int _EventGroupSelected = 0;
    private int _ParamGroupSelected = 0;

    /** Styles... ****************************/
    private static GUIStyle _BoldTxtStyle;
    private static GUIStyle _BoldErrorTxtStyle;
    private static GUIStyle _FoldoutTxtStyle;
    private static GUIStyle _ButtonStyle;
    private static GUIStyle _TxtFieldStyle;
    private static GUIStyle _TxtFieldErrorStyle;
    private static GUIStyle _CategoryTxtStyle;
    private static GUIStyle _ContentTxtStyle;
    private string _CountColorStyle   = "#8DFF9E";
    private string _ContentColorStyle = "#6487AA";
    private string _FoldoutColorStyle = "black";

    /** GUI Boolean ****************************/
    private static AnimBool _StudioPathSettingsFade;
    private static AnimBool _BusSettingsFade;
    private static AnimBool _BankSettingsFade;
    private static AnimBool _ParamSettingsFade;
    private static AnimBool _EventSettingsFade;

    private static bool[] _GroupIsValids = new bool[3] { true, true, true };


    /** SerializedProperty **********************/
    private SerializedObject   _StudioSettingsObj;
    private SerializedProperty _StudioPathProperty;



    //=======================================
    ////          Magic Methods           ////
    //=======================================
    [MenuItem("FMOD/FMODAudio Settings")]
    public static void OpenWindow()
    {
        EditorWindow.GetWindow(typeof(FModAudioManagerWindow), false, "FMODAudio Settings");
    }

    private void OnEnable()
    {
        /**AnimBool 갱신...***************************/
        FadeAnimBoolInit();
    }

    private void OnFocus()
    {
        /** Banks 갱신... ****************************/
        try { FMODUnity.EventManager.RefreshBanks(); } catch { /*TODO:...*/ }
    }

    private void OnGUI()
    {
        GUI_InitEditor();

        //이벤트들이 유효한지 사전 판단.
        EventGroupIsValid(_GroupIsValids);

        //에디터 스킨에 따른 색상 변화
        bool isBlack = (EditorGUIUtility.isProSkin == false);
        _BoldTxtStyle.normal.textColor = (isBlack ? Color.black : Color.white);

        _FoldoutTxtStyle.normal.textColor   = (isBlack ? Color.black : Color.white);
        _FoldoutTxtStyle.onNormal.textColor = (isBlack ? Color.black : Color.white);
        _FoldoutColorStyle = (isBlack ? "black" : "white");

        _CategoryTxtStyle.normal.textColor   = (isBlack ? Color.black : Color.white);
        _CategoryTxtStyle.onNormal.textColor = (isBlack ? Color.black : Color.white);

        //-----------------------------------------------------
        using (var view = new EditorGUILayout.ScrollViewScope(_Scrollpos, false, false, GUILayout.Height(position.height)))
        {
            /** 스크롤 뷰 시작. **************************/
            _Scrollpos = view.scrollPosition;
            GUI_ShowLogo();
            GUI_DrawLine();

            GUI_StudioPathSettings();
            GUI_DrawLine();

            using (var scope = new EditorGUI.DisabledGroupScope(!StudioPathIsValid()))
            {
                GUI_BankSettings();
                GUI_DrawLine();

                GUI_BusSettings();
                GUI_DrawLine();

                GUI_ParamSettings();
                GUI_DrawLine();

                GUI_EventSettings();

                scope.Dispose();
            }
            /** 스크롤 뷰 끝. ***************************/

            view.Dispose();
        }
        //--------------------------------------------------------
    }



    //========================================
    ////        GUI Content Methods       ////
    //========================================
    private void GUI_DrawLine(float space = 5f, float subOffset = 0f)
    {
        #region Omit
        EditorGUILayout.Space(15f);
        var rect = EditorGUILayout.BeginHorizontal();
        Handles.color = Color.gray;
        Handles.DrawLine(new Vector2(rect.x - 15 + subOffset, rect.y), new Vector2(rect.width + 15 - subOffset * 2, rect.y));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(10f);
        #endregion
    }

    private void GUI_InitEditor()
    {
        #region Omit
        /*************************************
         *   에디터에서 사용할 에셋 초기화.
         * **/
        if (_EditorSettings == null) _EditorSettings = AssetDatabase.LoadAssetAtPath<FModAudioEditorSettings>(_EditorSettingsPath);
        if (_StudioSettings == null) _StudioSettings = AssetDatabase.LoadAssetAtPath<FMODUnity.Settings>(_StudioSettingsPath);

        //EditorSettings이 없다면 새로 생성한다.
        if (_EditorSettings == null){

            _EditorSettings = new FModAudioEditorSettings();
            AssetDatabase.CreateAsset(_EditorSettings, _EditorSettingsPath);
        }

        /**************************************
         *  페이드 Anim들을 초기화한다.
         * **/
        if (_StudioPathSettingsFade == null){

            _StudioPathSettingsFade = new AnimBool(true);
            _StudioPathSettingsFade.speed = 3f;
            _StudioPathSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
        }

        if (_BusSettingsFade == null){

            _BusSettingsFade = new AnimBool(false);
            _BusSettingsFade.speed = 3f;
            _BusSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
        }

        if (_BankSettingsFade == null){

            _BankSettingsFade = new AnimBool(false);
            _BankSettingsFade.speed = 3f;
            _BankSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
        }

        if (_ParamSettingsFade == null){

            _ParamSettingsFade = new AnimBool(false);
            _ParamSettingsFade.speed = 3f;
            _ParamSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
        }

        if (_EventSettingsFade == null){

            _EventSettingsFade = new AnimBool(false);
            _EventSettingsFade.speed = 3f;
            _EventSettingsFade.valueChanged.AddListener(new UnityAction(base.Repaint));
        }

        /*************************************
         *  모든 텍스쳐들을 초기화한다.
         * **/
        if (_ParamLabelIconTex == null){

            _ParamLabelIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_ParamLabelIconPath, typeof(Texture));
        }

        if (_BankIconTex == null){

            _BankIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_BankIconPath, typeof(Texture));
        }

        if (_BannerBlackTex == null){

            _BannerBlackTex = (Texture)AssetDatabase.LoadAssetAtPath(_BannerBlackPath, typeof(Texture));
        }

        if (_BannerWhiteTex == null){

            _BannerWhiteTex = (Texture)AssetDatabase.LoadAssetAtPath(_BannerWhitePath, typeof(Texture));
        }

        if (_StudioIconTex == null){

            _StudioIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_StudioIconPath, typeof(Texture));
        }

        if (_SearchIconTex == null){

            _SearchIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_SearchIconPath, typeof(Texture));
        }

        if (_AddIconTex == null){

            _AddIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_AddIconPath, typeof(Texture));
        }

        if (_XYellowIconTex == null){

            _XYellowIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_XYellowIconPath, typeof(Texture));
        }

        if (_DeleteIconTex == null){

            _DeleteIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_DeleteIconPath, typeof(Texture));
        }

        if (_NotFoundIconTex == null){

            _NotFoundIconTex = (Texture)AssetDatabase.LoadAssetAtPath(_NotFoundIconPath, typeof(Texture));
        }

        /*********************************
         *  텍스트 스타일 초기화
         ***/
        if (_BoldTxtStyle == null){

            _BoldTxtStyle = new GUIStyle();
            _BoldTxtStyle.normal.textColor = Color.white;
            _BoldTxtStyle.fontStyle = FontStyle.Bold;
            _BoldTxtStyle.richText = true;
        }

        if (_FoldoutTxtStyle == null){

            _FoldoutTxtStyle = new GUIStyle(EditorStyles.foldout);
            _FoldoutTxtStyle.normal.textColor = Color.white;
            _FoldoutTxtStyle.fontStyle = FontStyle.Bold;
            _FoldoutTxtStyle.richText = true;
            _FoldoutTxtStyle.fontSize = 14;
        }

        if (_CategoryTxtStyle == null){

            _CategoryTxtStyle = new GUIStyle(EditorStyles.foldout);
            _CategoryTxtStyle.normal.textColor = Color.white;
            _CategoryTxtStyle.richText = true;
            _CategoryTxtStyle.fontStyle = FontStyle.Bold;
            _CategoryTxtStyle.fontSize = 12;
        }

        if (_ContentTxtStyle == null){

            _ContentTxtStyle = new GUIStyle(EditorStyles.label);
            _ContentTxtStyle.richText = true;
            _ContentTxtStyle.fontStyle = FontStyle.Bold;
            _ContentTxtStyle.fontSize = 12;
        }

        if (_BoldErrorTxtStyle == null){

            _BoldErrorTxtStyle = new GUIStyle();
            _BoldErrorTxtStyle.normal.textColor = Color.red;
            _BoldErrorTxtStyle.onNormal.textColor = Color.red;
            _BoldErrorTxtStyle.fontStyle = FontStyle.Bold;
            _BoldErrorTxtStyle.fontSize = 14;
            _BoldErrorTxtStyle.richText = true;
        }

        if (_ButtonStyle == null){

            _ButtonStyle = new GUIStyle(GUI.skin.button);
            _ButtonStyle.padding.top = 1;
            _ButtonStyle.padding.bottom = 1;
        }

        if (_TxtFieldStyle == null){

            _TxtFieldStyle = new GUIStyle(EditorStyles.textField);
            _TxtFieldStyle.richText = true;
        }

        if (_TxtFieldErrorStyle == null){

            _TxtFieldErrorStyle = new GUIStyle(EditorStyles.textField);
            _TxtFieldErrorStyle.richText = true;
            _TxtFieldErrorStyle.normal.textColor = Color.red;
            _TxtFieldErrorStyle.fontStyle = FontStyle.Bold;
            _TxtFieldErrorStyle.onNormal.textColor = Color.red;
        }


        /**************************************
         *  SerializedProperty 초기화
         ****/
        if (_StudioSettingsObj == null) _StudioSettingsObj = new SerializedObject(_StudioSettings);
        if (_StudioPathProperty == null) _StudioPathProperty = _StudioSettingsObj.FindProperty("sourceProjectPath");

        bool isBlackSkin = (EditorGUIUtility.isProSkin);
        _CountColorStyle = (isBlackSkin ? "#8DFF9E" : "#107C05");
        _ContentColorStyle = (isBlackSkin ? "#6487AA" : "#104C87");
        #endregion
    }

    private void GUI_ShowLogo()
    {
        #region Omit
        bool isBlack      = (EditorGUIUtility.isProSkin == false);
        Texture useBanner = (isBlack ? _BannerBlackTex : _BannerWhiteTex);

        GUILayout.Box(useBanner, GUILayout.Width(position.width), GUILayout.Height(100f));

        /**Editor Version을 띄운다.*/
        using (var scope = new GUILayout.AreaScope(new Rect(position.width * .5f - 95f, 100f - 20, 300, 30))){

            GUILayout.Label($"FModAudio Settings Editor {_EditorVersion}", _BoldTxtStyle);
            scope.Dispose();
        }

        #endregion
    }

    private void GUI_StudioPathSettings()
    {
        #region Omit

        EditorGUI.indentLevel++;
        _StudioPathSettingsFade.target = EditorGUILayout.Foldout(_StudioPathSettingsFade.target, "FMod Studio Path Setting", _FoldoutTxtStyle);
        EditorGUILayout.Space(3f);


        using (var fadeScope = new EditorGUILayout.FadeGroupScope(_StudioPathSettingsFade.faded))
        {
            bool EventIsValid = (_GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
            bool PathIsValid  = StudioPathIsValid();

            if (fadeScope.visible)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginHorizontal();
                /*************************************/
                float buttonWidth = 25f;
                float pathWidth = (position.width - buttonWidth * 4f);
                string prevPath = _StudioSettings.SourceProjectPath;

                GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
                GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
                GUILayoutOption pathWidthOption = GUILayout.Width(pathWidth);
                GUILayoutOption pathHeightOption = GUILayout.Height(buttonWidth);

                //경로 표시
                using (var scope = new EditorGUI.ChangeCheckScope())
                {
                    GUIStyle usedStyle = (PathIsValid ? _TxtFieldStyle : _TxtFieldErrorStyle);
                    string newPath = EditorGUILayout.TextField("Studio Project Path: ", _StudioPathProperty.stringValue, usedStyle, pathWidthOption, pathHeightOption);

                    //경로가 변경되었을 경우
                    if (scope.changed && newPath.EndsWith(".fspro")){

                        _StudioPathProperty.stringValue = newPath;
                        _StudioSettings.HasSourceProject = true;
                        ResetEditorSettings(true);
                    }

                    scope.Dispose();
                }

                //돋보기 버튼을 눌렀을 경우
                if (GUILayout.Button(_SearchIconTex, _ButtonStyle, buttonWidthOption, buttonHeightOption)){

                    try
                    {
                        if (FMODUnity.SettingsEditor.BrowseForSourceProjectPath(_StudioSettingsObj) && !prevPath.Equals(_StudioSettings.SourceProjectPath)){
                            ResetEditorSettings(true);
                        }
                    }
                    catch
                    {
                        if (!prevPath.Equals(_StudioSettings.SourceProjectPath)){
                            ResetEditorSettings(true);
                        }
                    }

                    _refresh = false;
                }

                //스튜디오 바로가기 버튼
                if (GUILayout.Button(_StudioIconTex, _ButtonStyle, buttonWidthOption, buttonHeightOption)){

                    string projPath = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;

                    if (StudioPathIsValid()){

                        System.Diagnostics.Process.Start(projPath);
                    }
                }
                EditorGUI.indentLevel--;

                /**************************************/
                EditorGUILayout.EndHorizontal();

                GUI_CreateEnumAndRefreshData();
                if (!PathIsValid) EditorGUILayout.HelpBox("The FMod Studio Project for that path does not exist.", MessageType.Error);
                else if (!EventIsValid) EditorGUILayout.HelpBox("Among the events currently loaded into the Editor, an invalid event exists. Press the Loaded Studio Settings button to reload.", MessageType.Error);
            }

            fadeScope.Dispose();
        }

        EditorGUI.indentLevel--;

        #endregion
    }

    private void GUI_CreateEnumAndRefreshData()
    {
        #region Omit
        using (var scope = new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.Space(10f);

            //옵션 저장 및 갱신
            bool allGroupInValid   = !(_GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
            bool studioPathInValid = !StudioPathIsValid();

            using (var disableScope = new EditorGUI.DisabledGroupScope(allGroupInValid || studioPathInValid))
            {
                if (GUILayout.Button("Save Settings and Create Enums", GUILayout.Width(position.width * .5f))){

                    CreateEnumScript();
                    ApplyBankLoadInfo();
                    if (_EditorSettings != null && _StudioSettings != null)
                    {
                        EditorUtility.SetDirty(_StudioSettings);
                        EditorUtility.SetDirty(_EditorSettings);
                    }

                    PlayerSettings.SetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone, _ScriptDefine);
                    AssetDatabase.Refresh();
                }
            }

            //FMod Studio 데이터 불러오기
            if (GUILayout.Button("Load Studio Settings", GUILayout.Width(position.width * .5f)))
            {
                if (_EditorSettings != null){

                    //현재 올바르게 빌드된 것들이 없다면 스킵한다....
                    try { FMODUnity.EventManager.RefreshBanks(); } catch { _refresh = true; return; }
                    GetBusList(_EditorSettings.BusList);
                    GetBankList(_EditorSettings.BankList);
                    GetParamList(_EditorSettings.ParamDescList, _EditorSettings.ParamLableList);
                    GetEventList(_EditorSettings.CategoryDescList, _EditorSettings.EventRefs, _EditorSettings.EventGroups);
                    _refresh = true;
                }
            }

            scope.Dispose();
        }

        /**데이터를 갱신했는데 이벤트가 없다면 빌드관련 경고를 띄운다..*/
        if (_refresh && _EditorSettings.EventRefs.Count == 0)
        {
            EditorGUILayout.HelpBox("Did the event not load even though you added it in FMod Studio? Make sure you've build the event in FMod Studio", MessageType.Info);
        }

        #endregion
    }

    private void GUI_BusSettings()
    {
        #region Omit
        List<NPData> busList = _EditorSettings.BusList;
        int          Count   = busList.Count;

        EditorGUI.indentLevel++;
        _BusSettingsFade.target = EditorGUILayout.Foldout(_BusSettingsFade.target, $"FMod Bus<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
        EditorGUILayout.Space(3f);


        using (var fadeScope = new EditorGUILayout.FadeGroupScope(_BusSettingsFade.faded))
        {
            if (fadeScope.visible)
            {
                EditorGUILayout.BeginVertical();
                /*******************************************/
                float buttonWidth = 25f;
                float pathWidth = (position.width - buttonWidth * 8f);

                GUILayoutOption buttonWidthOption = GUILayout.Width(buttonWidth);
                GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
                GUILayoutOption pathWidthOption = GUILayout.Width(pathWidth);
                GUILayoutOption pathHeightOption = GUILayout.Height(buttonWidth);

                if (Count > 0) EditorGUILayout.HelpBox("An FModBusType enum is created based on the information shown below.", MessageType.Info);

                //모든 버스 목록들을 보여준다.
                EditorGUI.indentLevel++;
                EditorGUI.BeginDisabledGroup(true);

                using (var horizontal = new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField($"<color={_ContentColorStyle}>Master</color>", _ContentTxtStyle, GUILayout.Width(150));
                    EditorGUILayout.TextArea("bus:/", _TxtFieldStyle, pathWidthOption, pathHeightOption);
                    horizontal.Dispose();
                }
                EditorGUI.EndDisabledGroup();

                for (int i = 0; i < Count; i++){

                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{busList[i].Name}</color>", _ContentTxtStyle, GUILayout.Width(150));
                        EditorGUILayout.TextArea(busList[i].Path, _TxtFieldStyle, pathWidthOption, pathHeightOption);
                        horizontal.Dispose();
                    }
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                /*******************************************/
            }

            fadeScope.Dispose();
        }

        EditorGUI.indentLevel--;
        #endregion
    }

    private void GUI_BankSettings()
    {
        #region Omit
        List<NPData> bankList = _EditorSettings.BankList;
        int          Count    = bankList.Count;

        EditorGUI.indentLevel++;
        _BankSettingsFade.target = EditorGUILayout.Foldout(_BankSettingsFade.target, $"FMod Banks<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
        EditorGUILayout.Space(3f);

        using (var fadeScope = new EditorGUILayout.FadeGroupScope(_BankSettingsFade.faded))
        {
            if (fadeScope.visible)
            {
                EditorGUILayout.BeginVertical();

                /*******************************************/
                float buttonWidth  = 150f;
                float buttonHeight = 25f;
                float pathWidth    = (position.width - buttonWidth - 40f);

                GUILayoutOption buttonWidthOption  = GUILayout.Width(buttonWidth);
                GUILayoutOption buttonHeightOption = GUILayout.Height(buttonHeight);
                GUILayoutOption pathWidthOption    = GUILayout.Width(pathWidth);
                GUILayoutOption pathHeightOption   = GUILayout.Height(buttonHeight);

                if (Count > 0) EditorGUILayout.HelpBox("An FModBankType enum is created based on the information shown below.", MessageType.Info);

                //모든 뱅크 목록들을 보여준다.
                EditorGUI.indentLevel++;
                for (int i = 0; i < Count; i++){

                    NPData bank = bankList[i];
                    string rawName = bank.Name.Replace("_", ".");

                    EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{bank.Name}</color>", _ContentTxtStyle, buttonWidthOption);
                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.TextArea(bank.Path, _TxtFieldStyle, pathWidthOption, pathHeightOption);

                        //초기화시 뱅크가 로드되도록 설정이 되어있다면....
                        bool isLoaded = bank.Extra;
                        BankLoadType loadType = (isLoaded ? BankLoadType.Load_At_Initialized : BankLoadType.Not_Load);

                        /***시작시 로드 필드를 변경했을 경우....**/
                        using (var scope = new EditorGUI.ChangeCheckScope())
                        {
                            loadType = (BankLoadType)EditorGUILayout.EnumPopup(loadType, GUILayout.Width(170f), buttonHeightOption);
                            if (scope.changed)
                            {
                                bank.Extra = (loadType == BankLoadType.Load_At_Initialized);
                                bankList[i] = bank;
                            }
                        }

                        horizontal.Dispose();
                    }

                    EditorGUILayout.Space(5f);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.EndVertical();
                /*******************************************/
            }

            fadeScope.Dispose();
        }

        EditorGUI.indentLevel--;
        #endregion
    }

    private void GUI_MenuSelected()
    {
        //TODO: 
    }

    private void GUI_ParamSettings()
    {
        #region Omit
        List<FModParamDesc> descs  = _EditorSettings.ParamDescList;
        List<string>        labels = _EditorSettings.ParamLableList;

        int Count           = descs.Count;
        int labelStartIndex = 0;
        float buttonWidth   = 25f;
        float pathWidth     = (position.width - buttonWidth * 10f);

        if (pathWidth <= 0f) pathWidth = 0f;

        GUILayoutOption buttonWidthOption  = GUILayout.Width(buttonWidth);
        GUILayoutOption buttonHeightOption = GUILayout.Height(buttonWidth);
        GUILayoutOption pathWidthOption    = GUILayout.Width(pathWidth);
        GUILayoutOption pathHeightOption   = GUILayout.Height(buttonWidth);

        EditorGUI.indentLevel++;
        _ParamSettingsFade.target = EditorGUILayout.Foldout(_ParamSettingsFade.target, $"FMod Parameters<color={_CountColorStyle}>({Count})</color>", _FoldoutTxtStyle);
        EditorGUILayout.Space(3f);

        using (var fadeScope = new EditorGUILayout.FadeGroupScope(_ParamSettingsFade.faded))
        {
            if (fadeScope.visible)
            {
                if (Count > 0) EditorGUILayout.HelpBox("An FModGlobalParameter/FModLocalParameter enum is created based on the information shown below.", MessageType.Info);

                _ParamGroups[0] = $"Global Parameters({_EditorSettings.ParamCountList[0]})";
                _ParamGroups[1] = $"Local Parameters({_EditorSettings.ParamCountList[1]})";
                _ParamGroupSelected = GUILayout.Toolbar(_ParamGroupSelected, _ParamGroups, GUILayout.Height(40f));

                //모든 파라미터 목록들을 보여준다.
                EditorGUI.indentLevel++;
                for (int i = 0; i < Count; i++)
                {
                    FModParamDesc desc = descs[i];

                    using (var horizontal = new EditorGUILayout.HorizontalScope())
                    {
                        if (descs[i].isGlobal && _ParamGroupSelected != 0
                            || !descs[i].isGlobal && _ParamGroupSelected != 1)
                        {

                            labelStartIndex += desc.LableCount;
                            continue;
                        }

                        GUILayout.Space(5f);
                        EditorGUILayout.LabelField($"<color={_ContentColorStyle}>{desc.ParamName}</color>", _ContentTxtStyle, GUILayout.Width(140));
                        EditorGUILayout.TextArea($"( <color=red>Min:</color> {desc.Min}~ <color=red>Max:</color> {desc.Max} )", _TxtFieldStyle, pathWidthOption, pathHeightOption);

                        //레이블 확인버튼
                        using (var disable = new EditorGUI.DisabledGroupScope(desc.LableCount <= 0))
                        {
                            //레이블이 존재한다면 버튼을 누를 수 있도록 한다.
                            if (EditorGUILayout.DropdownButton(new GUIContent("Labeld"), FocusType.Passive, GUILayout.Width(70f))){

                                GenericMenu menu = new GenericMenu();
                                for (int j = 0; j < desc.LableCount; j++){

                                    menu.AddItem(new GUIContent(labels[labelStartIndex + j]), true, GUI_MenuSelected);
                                }

                                menu.ShowAsContext();
                            }
                        }

                        horizontal.Dispose();
                        labelStartIndex += desc.LableCount;
                    }

                    /**파라미터 경로를 표시한다....**/
                    using (var vertical = new EditorGUILayout.VerticalScope())
                    {
                        using (var horizontal = new EditorGUILayout.HorizontalScope())
                        {
                            EditorGUILayout.LabelField($"<color=#6487AA00>{desc.ParamName}</color>", _ContentTxtStyle, GUILayout.Width(140));
                            EditorGUILayout.TextArea(desc.Path, _TxtFieldStyle, pathWidthOption);
                        }
                    }

                    GUILayout.Space(5f);

                }
                EditorGUI.indentLevel--;

            }

            fadeScope.Dispose();
        }

        EditorGUI.indentLevel--;
        #endregion
    }

    private void GUI_EventSettings()
    {
        #region Omit
        FModGroupInfo[]             descs     = _EditorSettings.EventGroups;
        List<FModEventCategoryDesc> categorys = _EditorSettings.CategoryDescList;
        List<FModEventInfo>         infos     = _EditorSettings.EventRefs;
        int infoCount     = infos.Count;
        int descCount     = descs.Length;
        int categoryCount = categorys.Count;

        EditorGUI.indentLevel++;
        bool   allGroupIsValid    = (_GroupIsValids[0] && _GroupIsValids[1] && _GroupIsValids[2]);
        string eventSettingsColor = (allGroupIsValid ? _FoldoutColorStyle : "red");
        _EventSettingsFade.target = EditorGUILayout.Foldout(_EventSettingsFade.target, $"<color={eventSettingsColor}>FMod Events</color><color={_CountColorStyle}>({infoCount})</color>", _FoldoutTxtStyle);
        EditorGUILayout.Space(3f);


        using (var fadeScope = new EditorGUILayout.FadeGroupScope(_EventSettingsFade.faded))
        {
            ////////////////////////////////////////////////////
            if (fadeScope.visible)
            {
                EditorGUI.indentLevel--;
                if (categoryCount > 0) EditorGUILayout.HelpBox("An FModBGMEventType/FModSFXEventType/FModNoGroupEventType enum is created based on the information shown below.", MessageType.Info);
                EditorGUI.indentLevel++;

                //루트폴더 세팅...
                if (_EditorSettings.RootFolderFoldout = EditorGUILayout.Foldout(_EditorSettings.RootFolderFoldout, "Event Group RootFolder Settings"))
                {
                    EditorGUILayout.HelpBox("Events are categorized into BGM and SFX groups based on the name of the root folder they belong to. If an event does not belong to either of these two groups, it is categorized into the NoGroup group.", MessageType.Info);

                    using (var scope = new EditorGUILayout.HorizontalScope()){

                        _EditorSettings.EventGroups[0].RootFolderName = EditorGUILayout.TextField("BGM RootFolder", _EditorSettings.EventGroups[0].RootFolderName, GUILayout.Width(position.width * .5f));
                        _EditorSettings.EventGroups[1].RootFolderName = EditorGUILayout.TextField("SFX RootFolder", _EditorSettings.EventGroups[1].RootFolderName, GUILayout.Width(position.width * .5f));
                        scope.Dispose();
                    }
                }


                _EventGroups[0] = $"BGM({descs[0].TotalEvent})";
                _EventGroups[1] = $"SFX({descs[1].TotalEvent})";
                _EventGroups[2] = $"NoGroups({descs[2].TotalEvent})";
                _EventGroupSelected = GUILayout.Toolbar(_EventGroupSelected, _EventGroups, GUILayout.Height(40f));


                //모든 카테고리를 순회한다.
                EditorGUI.indentLevel += 1;

                int startIndex = 0;
                for (int i = 0; i < categoryCount; i++)
                {
                    FModEventCategoryDesc category = categorys[i];

                    //현재 선택된 그룹의 카테고리가 아니라면 스킵.
                    if (category.GroupIndex != _EventGroupSelected){

                        startIndex += category.EventCount;
                        continue;
                    }

                    //해당 카테고리의 폴드아웃 효과 구현.
                    using (var change = new EditorGUI.ChangeCheckScope())
                    {
                        string groupColor = (category.EventIsValid ? _ContentColorStyle : "red");
                        category.foldout = EditorGUILayout.Foldout(category.foldout, $"<color={groupColor}>{category.CategoryName}</color>" + $"<color={_CountColorStyle}>({category.EventCount})</color>", _CategoryTxtStyle);

                        //foldout값이 바뀌었을 경우
                        if (change.changed){

                            categorys[i] = category;
                        }

                        change.Dispose();
                    }

                    //카테고리가 펼쳐진 상태라면 해당 카테고리에 속해있는 모든 이벤트를 출력한다.
                    if (category.foldout)
                    {
                        int EventCount = categorys[i].EventCount;
                        EditorGUI.indentLevel++;
                        for (int j = 0; j < EventCount; j++){

                            int realIndex  = (startIndex + j);
                            int groupIndex = (category.GroupIndex);
                            bool isValid   = CheckEventIsValid(realIndex, infos);

                            FModEventInfo info    = infos[realIndex];
                            GUILayoutOption width = GUILayout.Width(position.width - 25f);

                            EditorGUILayout.BeginVertical();
                            {
                                EditorGUILayout.LabelField(info.Name, width);
                                EditorGUILayout.TextField(info.Path, width);
                            }
                            EditorGUILayout.EndVertical();

                        }
                        EditorGUI.indentLevel--;
                    }

                    GUI_DrawLine(3f, 40f);
                    startIndex += category.EventCount;
                }

                EditorGUI.indentLevel -= 1;
            }

            fadeScope.Dispose();
            /////////////////////////////////////////////////////////
        }

        EditorGUI.indentLevel--;
        #endregion
    }



    //========================================
    ////          Utility Methods         ////
    //========================================
    private void ApplyBankLoadInfo()
    {
        #region Omit
        /***************************************************
         *    뱅크들의 로드 정보를 최종 적용한다....
         * *****/
        int count = _EditorSettings.BankList.Count;
        FMODUnity.BankLoadType loadType = FMODUnity.BankLoadType.All;

        _StudioSettings.BanksToLoad.Clear();
        for (int i = 0; i < count; i++)
        {
            NPData bank = _EditorSettings.BankList[i];

            /**FMod 초기화 단계에서 해당 뱅크를 로드하는가?**/
            if (bank.Extra == false)
            {
                loadType = FMODUnity.BankLoadType.Specified;
            }

            /**해당 뱅크가 실제로 존재할 경우만....**/
            else _StudioSettings.BanksToLoad.Add(bank.Name.Replace("_", "."));
        }

        /**모든 뱅크를 로드하는 것이라면, 특정 뱅크들의 로드 정보를 초기화한다....**/
        if (loadType == FMODUnity.BankLoadType.All)
        {
            _StudioSettings.BanksToLoad.Clear();
        }

        /**모든 뱅크를 로드하지 않는가?**/
        else if (loadType == FMODUnity.BankLoadType.Specified && _StudioSettings.BanksToLoad.Count == 0)
        {
            loadType = FMODUnity.BankLoadType.None;
        }

        _StudioSettings.BankLoadType = loadType;
        #endregion
    }

    private void ResetEditorSettings(bool setDirty = false)
    {
        #region Omit
        if (_EditorSettings == null || _StudioSettings == null) return;

        //Editor Settings Reset
        _EditorSettings.FoldoutBooleans = 0;
        _EditorSettings.BankList.Clear();
        _EditorSettings.BusList.Clear();
        _EditorSettings.ParamCountList[0] = 0;
        _EditorSettings.ParamCountList[1] = 0;
        _EditorSettings.ParamDescList.Clear();
        _EditorSettings.ParamLableList.Clear();
        _EditorSettings.CategoryDescList.Clear();
        _EditorSettings.EventRefs.Clear();
        _EditorSettings.EventGroups[0].TotalEvent = 0;
        _EditorSettings.EventGroups[1].TotalEvent = 0;
        _EditorSettings.EventGroups[2].TotalEvent = 0;
        EditorUtility.SetDirty(_EditorSettings);

        //Studio Settings Reset
        _StudioSettings.BankLoadType = FMODUnity.BankLoadType.All;
        _StudioSettings.BanksToLoad.Clear();

        if (setDirty)
        {
            EditorUtility.SetDirty(_StudioSettings);
            EditorUtility.SetDirty(_EditorSettings);
        }
        #endregion
    }

    private void CreateEnumScript()
    {
        #region Omit
        if (_EditorSettings == null) return;

        _builder.Clear();
        _builder.AppendLine("using UnityEngine;");
        _builder.AppendLine("");


        /**************************************
         * Bus Enum 정의.
         *****/
        _builder.AppendLine("public enum FModBusType");
        _builder.AppendLine("{");
        _builder.AppendLine("   Master=0,");

        List<NPData> busLists = _EditorSettings.BusList;
        int Count = busLists.Count;
        for (int i = 0; i < Count; i++)
        {
            string busName = RemoveUnnecessaryChar(busLists[i].Path, _BusRootPath);
            string comma = (i == Count - 1 ? "" : ",");
            _builder.AppendLine($"   {busName}={i + 1}{comma}");
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /********************************************
         *  Bank Enum 정의
         ***/
        _builder.AppendLine("public enum FModBankType");
        _builder.AppendLine("{");

        List<EditorBankRef> bankList = FMODUnity.EventManager.Banks;

        Count = bankList.Count;
        for (int i = 0; i < Count; i++)
        {
            EditorBankRef bank = bankList[i];
            string bankName = RemoveInValidChar(bank.Name);
            string comma = (i == Count - 1 ? "" : ",");
            _builder.AppendLine($"   {bankName}={i}{comma}");
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /*******************************************
         *  Global Parameter Enum 정의
         * ***/
        _builder.AppendLine("public enum FModGlobalParamType");
        _builder.AppendLine("{");
        _builder.AppendLine("   None_Parameter =0,");

        List<FModParamDesc> paramDescs = _EditorSettings.ParamDescList;

        Count = paramDescs.Count;
        for (int i = 0; i < Count; i++)
        {
            FModParamDesc desc = paramDescs[i];
            if (desc.isGlobal == false) continue;

            string comma = (i == Count - 1 ? "" : ",");
            string enumName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);
            _builder.AppendLine($"   {enumName}={i+1}{comma}");
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /*******************************************
         *  Local Parameter Enum 정의
         * ***/
        _builder.AppendLine("public enum FModLocalParamType");
        _builder.AppendLine("{");
        _builder.AppendLine($"   None_Parameter =0,");

        Count = paramDescs.Count;
        for (int i = 0; i < Count; i++)
        {
            FModParamDesc desc = paramDescs[i];
            if (desc.isGlobal) continue;

            string comma = (i == Count - 1 ? "" : ",");
            string enumName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);
            _builder.AppendLine($"   {enumName}={i+1}{comma}");
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /**********************************************
         *   Param Lable Struct 정의
         * *****/
        _builder.AppendLine("public struct FModParamLabel");
        _builder.AppendLine("{");

        Count = paramDescs.Count;
        for (int i = 0; i < Count; i++){

            FModParamDesc desc = paramDescs[i];
            if (desc.LableCount <= 0) continue;

            string structName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);

            _builder.AppendLine($"    public struct {structName}");
            _builder.AppendLine("    {");

            AddParamLabelListScript(_builder, i);

            _builder.AppendLine("    }");
        }


        _builder.AppendLine("}");
        _builder.AppendLine("");


        /**********************************************
         *   Param Range Struct 정의
         * *****/
        _builder.AppendLine("public struct FModParamValueRange");
        _builder.AppendLine("{");

        Count = paramDescs.Count;
        for (int i = 0; i < Count; i++){

            FModParamDesc desc = paramDescs[i];
            string structName = RemoveUnnecessaryChar(desc.Path, _ParamRootPath);

            _builder.AppendLine($"    public struct {structName}");
            _builder.AppendLine("    {");

            AddParamRangeListScript(_builder, i);

            _builder.AppendLine("    }");
        }


        _builder.AppendLine("}");
        _builder.AppendLine("");


        /**************************************
         * BGM Events Enum 정의
         ***/
        int total = 0;
        float writeEventCount = 0;
        List<FModEventCategoryDesc> categoryDescs = _EditorSettings.CategoryDescList;
        List<FModEventInfo> infos = _EditorSettings.EventRefs;
        Count = _EditorSettings.CategoryDescList.Count;

        _builder.AppendLine("public enum FModBGMEventType");
        _builder.AppendLine("{");

        for (int i = 0; i < Count; i++)
        {
            FModEventCategoryDesc desc = categoryDescs[i];

            //BGM 그룹이 아니라면 스킵.
            if (desc.GroupIndex != 0){

                total += desc.EventCount;
                continue;
            }

            //해당 카테고리의 모든 이벤트를 추가한다.
            for (int j = 0; j < desc.EventCount; j++){

                int realIndex = (total + j);
                string comma = (++writeEventCount == _EditorSettings.EventGroups[0].TotalEvent ? "" : ",");
                string path = infos[realIndex].Path;
                string enumName = RemoveUnnecessaryChar(path, _EventRootPath, _EditorSettings.EventGroups[0].RootFolderName, "/");

                if (CheckEventIsValid(realIndex, infos) == false) continue;
                _builder.AppendLine($"   {enumName}={realIndex}{comma}");
            }

            total += desc.EventCount;
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /**************************************
         *   SFX Events Enum 정의
         * ****/
        _builder.AppendLine("public enum FModSFXEventType");
        _builder.AppendLine("{");

        total = 0;
        writeEventCount = 0;
        for (int i = 0; i < Count; i++)
        {
            FModEventCategoryDesc desc = categoryDescs[i];

            //SFX 그룹이 아니라면 스킵.
            if (desc.GroupIndex != 1)
            {

                total += desc.EventCount;
                continue;
            }

            //해당 카테고리의 모든 이벤트를 추가한다.
            for (int j = 0; j < desc.EventCount; j++)
            {
                int realIndex = (total + j);
                string comma = (++writeEventCount == _EditorSettings.EventGroups[1].TotalEvent ? "" : ",");
                string path = infos[realIndex].Path;
                string enumName = RemoveUnnecessaryChar(path, _EventRootPath, _EditorSettings.EventGroups[1].RootFolderName, "/");

                if (CheckEventIsValid(realIndex, infos) == false) continue;
                _builder.AppendLine($"   {enumName}={realIndex}{comma}");
            }

            total += desc.EventCount;
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /**************************************
         *   NoGroups Events Enum 정의
         * ****/
        _builder.AppendLine("public enum FModNoGroupEventType");
        _builder.AppendLine("{");

        total = 0;
        writeEventCount = 0;
        for (int i = 0; i < Count; i++)
        {
            FModEventCategoryDesc desc = categoryDescs[i];

            //NoGroup 그룹이 아니라면 스킵.
            if (desc.GroupIndex != 2)
            {

                total += desc.EventCount;
                continue;
            }

            //해당 카테고리의 모든 이벤트를 추가한다.
            for (int j = 0; j < desc.EventCount; j++)
            {
                int realIndex = (total + j);
                string comma = (++writeEventCount == _EditorSettings.EventGroups[2].TotalEvent ? "" : ",");
                string path = infos[realIndex].Path;
                string enumName = RemoveUnnecessaryChar(path, _EventRootPath);

                if (CheckEventIsValid(realIndex, infos) == false) continue;
                _builder.AppendLine($"   {enumName}={realIndex}{comma}");
            }

            total += desc.EventCount;
        }

        _builder.AppendLine("}");
        _builder.AppendLine("");


        /***************************************
         * Event Reference List class 정의
         ***/
        _builder.AppendLine("public sealed class FModReferenceList");
        _builder.AppendLine("{");
        _builder.AppendLine("    public static readonly FMOD.GUID[] Events = new FMOD.GUID[]");
        _builder.AppendLine("    {");
        AddEventListScript(_builder, _EditorSettings.EventRefs);
        _builder.AppendLine("    };");
        _builder.AppendLine("");

        _builder.AppendLine("    public static readonly string[] Banks = new string[]");
        _builder.AppendLine("    {");
        AddBankListScript(_builder);
        _builder.AppendLine("    };");
        _builder.AppendLine("");


        _builder.AppendLine("    public static readonly string[] Params = new string[]");
        _builder.AppendLine("    {");
        AddParamListScript(_builder);
        _builder.AppendLine("    };");
        _builder.AppendLine("");

        _builder.AppendLine("    public static readonly string[] BusPaths = new string[]");
        _builder.AppendLine("    {");
        AddBusPathListScript(_builder);
        _builder.AppendLine("    };");
        _builder.AppendLine("");

        _builder.AppendLine("}");
        _builder.AppendLine("");


        //생성 및 새로고침
        File.WriteAllText(_DataScriptPath, _builder.ToString());

        #endregion
    }

    private void AddBusPathListScript(StringBuilder builder)
    {
        #region Omit
        List<NPData> list = _EditorSettings.BusList;
        int Count = list.Count;

        builder.AppendLine($"        \"{_BusRootPath}\"{(Count == 0 ? "" : ",")}");
        for (int i = 0; i < Count; i++)
        {
            string comma = (i == Count - 1 ? "" : ",");
            builder.AppendLine($"        \"{list[i].Path}\"" + comma);
        }
        #endregion
    }

    private void AddParamListScript(StringBuilder builder)
    {
        #region Omit
        List<FModParamDesc> list = _EditorSettings.ParamDescList;
        int Count = list.Count;

        builder.AppendLine($"        \"\"" + (Count==0? "":","));

        for (int i = 0; i < Count; i++)
        {
            string comma = (i == Count - 1 ? "" : ",");
            builder.AppendLine($"        \"{list[i].ParamName}\"" + comma);
        }
        #endregion
    }

    private void AddParamRangeListScript(StringBuilder builder, int descIndex)
    {
        #region Omit
        List<FModParamDesc> descs = _EditorSettings.ParamDescList;
        FModParamDesc desc = descs[descIndex];

        builder.AppendLine($"       public const float Min={desc.Min};");
        builder.AppendLine($"       public const float Max={desc.Max};");
        #endregion
    }

    private void AddParamLabelListScript(StringBuilder builder, int descIndex)
    {
        #region Omit
        List<FModParamDesc> descs = _EditorSettings.ParamDescList;
        List<string> labels = _EditorSettings.ParamLableList;

        FModParamDesc desc = descs[descIndex];
        if (desc.LableCount <= 0) return;

        int startIndex = GetParamLabelStartIndex(descIndex);
        for (int i = 0; i < desc.LableCount; i++)
        {
            int realIndex = (startIndex + i);
            string labelName = RemoveInValidChar(labels[realIndex]);
            builder.AppendLine($"       public const float {labelName}  ={i}f;");
        }
        #endregion
    }

    private void AddBankListScript(StringBuilder builder)
    {
        #region Omit
        List<FMODUnity.EditorBankRef> list = FMODUnity.EventManager.Banks;
        int Count = list.Count;
        for (int i = 0; i < Count; i++)
        {
            string comma = (i == Count - 1 ? "" : ",");
            builder.AppendLine($"        \"{list[i].Name}\"" + comma);
        }
        #endregion
    }

    private void AddEventListScript(StringBuilder builder, List<FModEventInfo> list, bool lastWork = false)
    {
        #region Omit
        int Count = list.Count;
        for (int i = 0; i < Count; i++)
        {
            string comma = (i == Count - 1 && lastWork ? "" : ",");
            string guidValue = $"Data1={list[i].GUID.Data1}, Data2={list[i].GUID.Data2}, Data3={list[i].GUID.Data3}, Data4={list[i].GUID.Data4}";
            if (CheckEventIsValid(i, list) == false) continue;
            builder.AppendLine("        new FMOD.GUID{ " + guidValue + " }" + comma);
        }
        #endregion
    }

    private void FadeAnimBoolInit()
    {
        #region Omit
        UnityAction repaintEvent = new UnityAction(base.Repaint);

        _StudioPathSettingsFade?.valueChanged.RemoveAllListeners();
        _StudioPathSettingsFade?.valueChanged.AddListener(repaintEvent);

        _BusSettingsFade?.valueChanged.RemoveAllListeners();
        _BusSettingsFade?.valueChanged.AddListener(repaintEvent);

        _ParamSettingsFade?.valueChanged.RemoveAllListeners();
        _ParamSettingsFade?.valueChanged.AddListener(repaintEvent);

        _EventSettingsFade?.valueChanged.RemoveAllListeners();
        _EventSettingsFade?.valueChanged.AddListener(repaintEvent);

        _BusSettingsFade?.valueChanged.RemoveAllListeners();
        _BusSettingsFade?.valueChanged.AddListener(repaintEvent);

        #endregion
    }

    private bool StudioPathIsValid()
    {
        string projPath = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
        return File.Exists(projPath);
    }

    private string RemoveInValidChar(string inputString)
    {
        #region Omit
        //시작 숫자 제거
        int removeCount = Regex.Match(inputString, @"^\d*").Length;
        inputString = inputString.Substring(removeCount, inputString.Length - removeCount);

        //사용하지 못하는 특수문자 제거
        inputString = _regex.Replace(inputString, "_");

        //공백문자 제거.
        inputString = inputString.Replace(" ", "_");

        return inputString;
        #endregion
    }

    private string RemoveUnnecessaryChar(string inputString, params string[] removeFirstString)
    {
        #region Omit
        //문자열 앞부분에서 불필요한 부분이 발견된다면 제거한다....
        int len = removeFirstString.Length;
        for (int i = 0; i < len; i++)
        {
            string cur = removeFirstString[i];

            if (inputString.IndexOf(cur) == 0)
            {
                int curLen = cur.Length;
                inputString = inputString.Substring(curLen, inputString.Length - curLen);
            }
        }

        return RemoveInValidChar(inputString);
        #endregion
    }

    private void GetBankList(List<NPData> lists)
    {
        #region Omit
        if (lists == null) return;
        lists.Clear();

        List<FMODUnity.EditorBankRef> banks = FMODUnity.EventManager.Banks;

        int Count = banks.Count;
        for (int i = 0; i < Count; i++){

            EditorBankRef bank = banks[i];
            string bankName = RemoveInValidChar(bank.Name);
            string bankPath = bank.Path;

            NPData info = new NPData()
            {
                Name = bankName,
                Path = bankPath,
                Extra = true
            };

            lists.Add(info);
        }
        #endregion
    }

    private void GetBusList(List<NPData> lists)
    {
        #region Omit
        if (lists == null || StudioPathIsValid() == false) return;
        lists.Clear();

        string   studiopath = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
        string[] pathSplit  = studiopath.Split('/');
        string   busPath    = studiopath.Replace(pathSplit[pathSplit.Length - 1], "") + _GroupFolderPath;

        Dictionary<string, NPData> busMap = new Dictionary<string, NPData>();
        DirectoryInfo dPath    = new DirectoryInfo(busPath);
        XmlDocument   document = new XmlDocument();

        try
        {
            //Studio folder의 모든 bus 데이터들을 읽어들인후 기록.
            foreach (FileInfo file in dPath.GetFiles())
            {
                if (!file.Exists) continue;

                try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                string idNode = document.SelectSingleNode("//object/@id")?.InnerText;
                string nameNode = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                string outputNode = document.SelectSingleNode("//object/relationship[@name='output']/destination")?.InnerText;

                if (idNode != null && nameNode != null && outputNode != null)
                {

                    busMap.Add(idNode, new NPData { Name = nameNode, Path = outputNode });
                    lists.Add(new NPData { Name = nameNode, Path = outputNode });
                }
            }
        }
        catch
        {

            //파일을 찾는데 실패하면 스킵한다.
            return;
        }

        //불러온 모든 busData들의 경로를 제대로 기록한다.
        int Count = lists.Count;
        for (int i = 0; i < Count; i++)
        {
            string busName = lists[i].Name;
            string parentBus = lists[i].Path;
            string finalPath = busName;

            while (busMap.ContainsKey(parentBus)){

                finalPath = busMap[parentBus].Name + "/" + finalPath;
                parentBus = busMap[parentBus].Path;
            }

            //마무리 작업
            lists[i] = new NPData { Name = busName, Path = _BusRootPath + finalPath };
        }

        #endregion
    }

    private void GetParamList(List<FModParamDesc> descList, List<string> labelList)
    {
        #region Omit
        if (descList == null || labelList == null) return;

        /**********************************************************
         *    파라미터 목록을 얻어오기 위한 초기화 과정을 진행한다...
         * ******/
        descList.Clear();
        labelList.Clear();
        _EditorSettings.ParamCountList[0] = _EditorSettings.ParamCountList[1] = 0;

        string   studiopath       = Application.dataPath.Replace("Assets", "") + _StudioPathProperty.stringValue;
        string[] pathSplit        = studiopath.Split('/');
        string   studioRootPath   = studiopath.Replace(pathSplit[pathSplit.Length - 1], "");
        string   presetPath       = studioRootPath + _PresetFolderPath;
        string   presetFolderPath = studioRootPath + _PresetFolderFolderPath;

        DirectoryInfo pPath  = new DirectoryInfo(presetPath);
        DirectoryInfo pfPath = new DirectoryInfo(presetFolderPath);
        XmlDocument document = new XmlDocument();
        Dictionary<string, ParamFolderInfo> folderMap = new Dictionary<string, ParamFolderInfo>();



        /************************************************************
         *     파라미터들의 정보와 부모 폴더 이름을 기록한다...
         * ******/
        try
        {
            foreach (FileInfo file in pPath.GetFiles())
            {
                if (!file.Exists) continue;

                try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                string nameNode = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                string outputNode = document.SelectSingleNode("//object/relationship[@name='folder']/destination")?.InnerText;
                string typeNode = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='parameterType']/value")?.InnerText;
                string minNode = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='minimum']/value")?.InnerText; //null이라면 0.
                string maxNode = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='maximum']/value")?.InnerText; //null이라면 0.
                string isGlobalNode = document.SelectSingleNode("//object[@class='GameParameter']/property[@name='isGlobal']/value")?.InnerText; //null이라면 false.
                XmlNodeList labelNodes = document.SelectNodes("//object[@class='GameParameter']/property[@name='enumerationLabels']/value");


                /**파라미터 값을 기록한다...*/
                FModParamDesc newDesc = new FModParamDesc()
                {
                    ParamName = (nameNode == null ? "" : nameNode),
                    Path = "",
                    ParentFolderName = (outputNode == null ? "" : outputNode),
                    isGlobal = (isGlobalNode != null),
                    LableCount = labelNodes.Count,
                    Min = (minNode == null ? 0f : float.Parse(minNode)),
                    Max = (maxNode == null ? 1f : float.Parse(maxNode) - 1f),
                };

                for (int i = 0; i < newDesc.LableCount; i++)
                {
                    labelList.Add(labelNodes[i].InnerText);
                }

                descList.Add(newDesc);
                _EditorSettings.ParamCountList[(newDesc.isGlobal ? 0 : 1)]++;
            }
        }
        catch
        {

            /**파일을 찾을 수 없었다면 스킵한다...**/
            return;
        }



        /*****************************************************
         *     파라미터들이 담긴 폴더 정보들을 모두 기록한다...
         * *******/
        try
        {
            foreach (FileInfo file in pfPath.GetFiles())
            {
                if (!file.Exists) continue;

                try { document.LoadXml(File.ReadAllText(file.FullName)); } catch { continue; }
                string idNode = document.SelectSingleNode("//object/@id")?.InnerText;
                string nameNode = document.SelectSingleNode("//object/property[@name='name']/value")?.InnerText;
                string outputNode = document.SelectSingleNode("//object/relationship[@name='folder']/destination")?.InnerText;

                /**폴더 정보들을 모두 기록한다...**/
                ParamFolderInfo newInfo = new ParamFolderInfo()
                {
                    id = idNode,
                    name = nameNode,
                    output = outputNode,
                };

                /**루트폴더는 추가하지 않는다...**/
                if (outputNode == null) continue;
                folderMap.Add(idNode, newInfo);
            }


        }
        catch
        {
            /**파일 목록이 없다면 스킵한다....**/
            return;
        }



        /********************************************************
         *    파라미터 폴더들의 위치를 역추적해 경로를 기록한다...
         * ******/
        int paramCount = descList.Count;
        for (int i = 0; i < paramCount; i++){

            FModParamDesc cur = descList[i];
            string parent = cur.ParentFolderName;
            string finalPath = "";


            /**부모폴더가 존재하지 않을 때까지 거슬러 올라간다...*/
            while (folderMap.ContainsKey(parent))
            {
                finalPath = folderMap[parent].name + "/" + cur.Path;
                parent = folderMap[parent].output;
            }

            /**마무리 작업...*/
            cur.Path = (_ParamRootPath + finalPath + cur.ParamName);
            descList[i] = cur;
        }

        #endregion
    }

    private bool GetParamIsAlreadyContain(string checkParamName, List<FModParamDesc> descs)
    {
        #region Omit
        int Count = descs.Count;
        for (int i = 0; i < Count; i++)
        {
            if (descs[i].ParamName == checkParamName) return true;
        }

        return false;
        #endregion
    }

    private int GetCategoryIndex(string categoryName, List<FModEventCategoryDesc> descs)
    {
        #region Omit
        int Count = descs.Count;
        for (int i = 0; i < Count; i++)
        {
            if (descs[i].CategoryName.Equals(categoryName)) return i;
        }

        return -1;
        #endregion
    }

    private int GetCategoryEventStartIndex(string categoryName, List<FModEventCategoryDesc> descs)
    {
        #region Omit
        int total = 0;
        int Count = descs.Count;
        for (int i = 0; i < Count; i++)
        {
            if (descs[i].CategoryName == categoryName) return total;
            total += descs[i].EventCount;
        }

        return total;
        #endregion
    }

    private void GetEventList(List<FModEventCategoryDesc> categoryList, List<FModEventInfo> refList, FModGroupInfo[] groupList)
    {
        #region Omit
        if (categoryList == null || refList == null || groupList == null) return;
        categoryList.Clear();
        refList.Clear();

        groupList[0].TotalEvent = groupList[1].TotalEvent = groupList[2].TotalEvent = 0;

        List<EditorEventRef> eventRefs = FMODUnity.EventManager.Events;
        int eventCount = eventRefs.Count;

        //모든 이벤트들을 그룹별로 분류한다.
        for (int i = 0; i < eventCount; i++)
        {
            EditorEventRef eventRef = eventRefs[i];
            string Path = eventRef.Path;
            string[] PathSplit = Path.Split('/');
            string Name = RemoveInValidChar(PathSplit[PathSplit.Length - 1]);
            string CategoryName = Path.Replace("event:/", "").Replace("/" + PathSplit[PathSplit.Length - 1], "");
            int GroupIndex = GetEventGroupIndex(PathSplit[1]);
            int CategoryIndex = GetCategoryIndex(CategoryName, categoryList);

            //카테고리가 없으면 카테고리를 새로 만든다.
            if (CategoryName.Equals(PathSplit[PathSplit.Length - 1])){

                CategoryName = "Root";
                CategoryIndex = GetCategoryIndex(CategoryName, categoryList);
            }

            if (CategoryIndex == -1){

                categoryList.Add(new FModEventCategoryDesc { CategoryName = CategoryName, EventCount = 1, GroupIndex = GroupIndex });
                CategoryIndex = (categoryList.Count - 1);
            }
            else categoryList[CategoryIndex] = new FModEventCategoryDesc(categoryList[CategoryIndex], 1);
            groupList[GroupIndex].TotalEvent++;

            int eventStartIndex = GetCategoryEventStartIndex(CategoryName, categoryList);
            refList.Insert(eventStartIndex, new FModEventInfo { Name = Name, Path = Path, GUID = eventRef.Guid });
        }
        #endregion
    }

    private int GetEventGroupIndex(string RootFolder)
    {
        if (_EditorSettings.EventGroups[0].RootFolderName.Equals(RootFolder)) return 0;
        else if (_EditorSettings.EventGroups[1].RootFolderName.Equals(RootFolder)) return 1;
        else return 2;
    }

    private int GetParamLabelStartIndex(int descIndex)
    {
        #region Omit
        int total = 0;
        List<FModParamDesc> descs = _EditorSettings.ParamDescList;

        for (int i = 0; i < descIndex; i++)
        {
            total += descs[i].LableCount;
        }

        return total;
        #endregion
    }

    private void EventGroupIsValid(bool[] groupEventBooleans)
    {
        #region Omit
        if (_EditorSettings == null) return;

        FModGroupInfo[]             groups    = _EditorSettings.EventGroups;
        List<FModEventCategoryDesc> categorys = _EditorSettings.CategoryDescList;
        List<FModEventInfo>         infos     = _EditorSettings.EventRefs;

        groupEventBooleans[0] = groupEventBooleans[1] = groupEventBooleans[2] = true;

        int total         = 0;
        int CategoryCount = categorys.Count;

        /************************************
         *   모든 카테고리를 순회한다.
         * **/
        for (int i = 0; i < CategoryCount; i++){

            FModEventCategoryDesc desc = categorys[i];
            desc.EventIsValid = true;

            for (int j = 0; j < desc.EventCount; j++)
            {
                int realIndex = (total + j);
                bool eventIsValid = CheckEventIsValid(realIndex, infos);

                //유효하지 않은 인덱스를 가지고 있다면....
                if (eventIsValid == false)
                {

                    groupEventBooleans[desc.GroupIndex] = false;
                    desc.EventIsValid = false;
                }
            }

            categorys[i] = desc;
            total += desc.EventCount;
        }
        #endregion
    }

    private bool CheckEventIsValid(int index, List<FModEventInfo> lists)
    {
        #region Omit
        if (Settings.Instance.EventLinkage == EventLinkage.Path && !lists[index].GUID.IsNull)
        {
            EditorEventRef eventRef = EventManager.EventFromGUID(lists[index].GUID);

            if (eventRef == null || eventRef != null && (eventRef.Path != lists[index].Path))
            {
                return false;
            }
        }
        return true;

        #endregion
    }

}
#endif