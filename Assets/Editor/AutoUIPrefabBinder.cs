using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

public class AutoUIPrefabBinder : EditorWindow
{
    private GameObject prefab;
    private bool bindInteractiveComponents = true;
    private bool bindDisplayComponents = true;
    private string nameKeywords = "";
    private string tagFilter = "";
    private bool enableFilter = false;
    private bool filterDefaultNames = true;
    
    [MenuItem("Tools/Auto UI Prefab Binder")]
    public static void ShowWindow()
    {
        GetWindow<AutoUIPrefabBinder>("UI Prefab Binder");
    }
    
    private void OnGUI()
    {
        GUILayout.Space(10);
        
        // 预制体选择
        GUILayout.Label("选择UGUI预制体:", EditorStyles.boldLabel);
        prefab = (GameObject)EditorGUILayout.ObjectField("预制体", prefab, typeof(GameObject), false);
        
        GUILayout.Space(20);
        
        // 组件绑定选项
        GUILayout.Label("组件绑定类型:", EditorStyles.boldLabel);
        bindInteractiveComponents = EditorGUILayout.ToggleLeft("绑定交互组件（Button/Toggle/Slider/Dropdown/InputField/Scrollbar/ScrollRect）", bindInteractiveComponents);
        bindDisplayComponents = EditorGUILayout.ToggleLeft("绑定展示型组件（Text/TextMeshProUGUI/Image/RawImage）", bindDisplayComponents);
        
        GUILayout.Space(20);
        
        // 过滤设置
        GUILayout.Label("过滤设置:", EditorStyles.boldLabel);
        enableFilter = EditorGUILayout.ToggleLeft("启用过滤（未匹配的组件不绑定）", enableFilter);
        
        EditorGUI.BeginDisabledGroup(!enableFilter);
        {
            GUILayout.Space(10);
            
            // 过滤默认名字组件
            filterDefaultNames = EditorGUILayout.ToggleLeft("过滤默认名字的组件（Button/Text/Image等）", filterDefaultNames);
            EditorGUILayout.HelpBox("跳过名称为Button、Text、Image等默认名称的组件，避免绑定冗余组件", MessageType.Info);
            
            GUILayout.Space(10);
            
            // 名称过滤
            GUILayout.BeginVertical("box");
            GUILayout.Label("名称过滤:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            nameKeywords = EditorGUILayout.TextField("关键词（逗号分隔）", nameKeywords);
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("例如: \"Dynamic,Score\" - 只绑定名称包含Dynamic或Score的物体上的组件", MessageType.Info);
            GUILayout.EndVertical();
            
            GUILayout.Space(10);
            
            // Tag过滤
            GUILayout.BeginVertical("box");
            GUILayout.Label("Tag过滤:", EditorStyles.boldLabel);
            GUILayout.Space(5);
            tagFilter = EditorGUILayout.TagField("Tag（必须完全匹配）", tagFilter);
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("例如: \"UI_Interactive\" - 只绑定Tag为UI_Interactive的物体上的组件", MessageType.Info);
            GUILayout.EndVertical();
            
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("名称过滤和Tag过滤为OR关系，满足任一条件即可绑定", MessageType.Info);
        }
        EditorGUI.EndDisabledGroup();
        
        GUILayout.Space(20);
        
        // 生成按钮
        if (GUILayout.Button("生成绑定脚本", GUILayout.Height(30)))
        {
            if (prefab != null)
            {
                GenerateBindingScript();
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请先选择一个预制体", "确定");
            }
        }
        
        GUILayout.Space(10);
        
        // 使用说明
        GUILayout.Label("使用说明:", EditorStyles.boldLabel);
        GUILayout.Label("1. 拖入UGUI预制体到上方字段");
        GUILayout.Label("2. 选择要绑定的组件类型");
        GUILayout.Label("3. 设置过滤条件（可选）");
        GUILayout.Label("4. 点击生成按钮");
        GUILayout.Label("5. 脚本会自动生成到 Assets/Scripts/UI 目录并挂载到预制体上");
        GUILayout.Label("6. 预制体不会被自动放入场景Hierarchy中");
        
        GUILayout.Space(10);
        
        // 过滤逻辑说明
        GUILayout.Label("过滤逻辑说明:", EditorStyles.boldLabel);
        GUILayout.Label("- 默认名字过滤：跳过Button、Text等默认名称的组件");
        GUILayout.Label("- 名称过滤：GameObject名称包含任一关键词即可");
        GUILayout.Label("- Tag过滤：GameObject的Tag完全匹配即可");
        GUILayout.Label("- 名称和Tag过滤为OR关系（满足任一即可）");
        GUILayout.Label("- 适用于大型UI系统的组件分类管理");
    }
    
    private void GenerateBindingScript()
    {
        // 检查是否是UGUI预制体（含有RectTransform）
        if (!prefab.GetComponent<RectTransform>())
        {
            EditorUtility.DisplayDialog("错误", "请选择一个UGUI预制体（含有RectTransform的物体）", "确定");
            return;
        }
        
        // 分析预制体结构
        List<ComponentInfo> components = AnalyzePrefabStructure(prefab.transform);
        
        if (components.Count == 0)
        {
            EditorUtility.DisplayDialog("提示", "未找到符合条件的组件，请检查过滤设置和组件类型选择。", "确定");
            return;
        }
        
        // 生成脚本内容
        string scriptContent = GenerateScriptContent(prefab.name, components);
        
        // 保存脚本到UI文件夹
        string uiScriptPath = "Assets/Scripts/UI";
        if (!Directory.Exists(uiScriptPath))
        {
            Directory.CreateDirectory(uiScriptPath);
        }
        string scriptPath = Path.Combine(uiScriptPath, prefab.name + "Binder.cs");
        
        // 检查是否覆盖
        if (File.Exists(scriptPath))
        {
            if (!EditorUtility.DisplayDialog("提示", "绑定脚本已存在，是否覆盖？", "是", "否"))
            {
                return;
            }
        }
        
        // 写入脚本文件
        File.WriteAllText(scriptPath, scriptContent, Encoding.UTF8);
        AssetDatabase.Refresh();
        
        // 挂载脚本到预制体
        MountScriptToPrefab(scriptPath);
        
        EditorUtility.DisplayDialog("成功", $"绑定脚本生成并挂载成功！\n共绑定 {components.Count} 个组件。", "确定");
    }
    
    private List<ComponentInfo> AnalyzePrefabStructure(Transform root)
    {
        List<ComponentInfo> components = new List<ComponentInfo>();
        
        // 遍历所有子物体
        foreach (Transform child in root)
        {
            AnalyzeGameObject(child, root, components);
        }
        
        return components;
    }
    
    private void AnalyzeGameObject(Transform transform, Transform root, List<ComponentInfo> components)
    {
        // 检查是否通过过滤
        if (enableFilter && !PassesFilter(transform))
        {
            return;
        }
        
        // 检查交互组件
        if (bindInteractiveComponents)
        {
            CheckAndAddComponent<UnityEngine.UI.Button>(transform, root, components, "btn");
            CheckAndAddComponent<UnityEngine.UI.Toggle>(transform, root, components, "tgl");
            CheckAndAddComponent<UnityEngine.UI.Slider>(transform, root, components, "sld");
            CheckAndAddComponent<UnityEngine.UI.Dropdown>(transform, root, components, "dd");
            CheckAndAddComponent<UnityEngine.UI.InputField>(transform, root, components, "inp");
            CheckAndAddComponent<UnityEngine.UI.Scrollbar>(transform, root, components, "sb");
            CheckAndAddComponent<UnityEngine.UI.ScrollRect>(transform, root, components, "sr");
        }
        
        // 检查展示型组件
        if (bindDisplayComponents)
        {
            CheckAndAddComponent<UnityEngine.UI.Text>(transform, root, components, "txt");
            CheckAndAddComponent<TMPro.TextMeshProUGUI>(transform, root, components, "tmp");
            CheckAndAddComponent<UnityEngine.UI.Image>(transform, root, components, "img");
            CheckAndAddComponent<UnityEngine.UI.RawImage>(transform, root, components, "raw");
        }
        
        // 递归检查子物体
        foreach (Transform child in transform)
        {
            AnalyzeGameObject(child, root, components);
        }
    }
    
    private bool PassesFilter(Transform transform)
    {
        // 检查是否过滤默认名字
        if (filterDefaultNames && IsDefaultName(transform.name))
        {
            return false;
        }
        
        // 检查名称过滤
        bool nameMatches = true;
        if (!string.IsNullOrEmpty(nameKeywords))
        {
            string[] keywords = nameKeywords.Split(new[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
            nameMatches = false;
            foreach (string keyword in keywords)
            {
                if (transform.name.Contains(keyword.Trim()))
                {
                    nameMatches = true;
                    break;
                }
            }
        }
        
        // 检查Tag过滤
        bool tagMatches = true;
        if (!string.IsNullOrEmpty(tagFilter))
        {
            tagMatches = transform.CompareTag(tagFilter);
        }
        
        // 名称过滤和Tag过滤为OR关系（满足任一条件即可）
        // 如果都为空，则通过过滤
        if (string.IsNullOrEmpty(nameKeywords) && string.IsNullOrEmpty(tagFilter))
        {
            return true;
        }
        
        return nameMatches || tagMatches;
    }
    
    private bool IsDefaultName(string name)
    {
        // 常见的默认名称列表
        string[] defaultNames = new string[]
        {
            "Button", "Text", "Image", "RawImage", "Toggle", "Slider", 
            "Dropdown", "InputField", "Scrollbar", "ScrollRect", "Panel",
            "Canvas", "RectTransform", "Background", "Title", "Content"
        };
        
        foreach (string defaultName in defaultNames)
        {
            if (name.Equals(defaultName, System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private void CheckAndAddComponent<T>(Transform transform, Transform root, List<ComponentInfo> components, string abbreviation) where T : Component
    {
        T component = transform.GetComponent<T>();
        if (component != null)
        {
            string path = GetTransformPath(transform, root);
            string componentName = typeof(T).Name;
            
            // 生成字段名：组件缩写_GameObject名称_后缀
            string fieldName = GenerateFieldName(transform.name, abbreviation);
            
            // 处理同名情况
            int suffix = 1;
            string originalFieldName = fieldName;
            while (components.Exists(c => c.FieldName == fieldName))
            {
                fieldName = originalFieldName + "_" + suffix;
                suffix++;
            }
            
            components.Add(new ComponentInfo
            {
                ComponentType = typeof(T),
                Path = path,
                FieldName = fieldName,
                Transform = transform,
                Abbreviation = abbreviation
            });
        }
    }
    
    private string GetTransformPath(Transform current, Transform root)
    {
        if (current == root)
            return "";
        
        string path = current.name;
        Transform parent = current.parent;
        
        while (parent != root && parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        
        return path;
    }
    
    private string GenerateFieldName(string gameObjectName, string abbreviation)
    {
        // 移除特殊字符，只保留字母、数字和下划线
        string name = Regex.Replace(gameObjectName, @"[^a-zA-Z0-9_]", "");
        // 首字母大写
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name.Substring(1);
        }
        // 确保名称不为空
        if (string.IsNullOrEmpty(name))
        {
            name = "Unnamed";
        }
        // 组件缩写 + GameObject名称
        return abbreviation + "_" + name;
    }
    
    private string GenerateScriptContent(string prefabName, List<ComponentInfo> components)
    {
        StringBuilder sb = new StringBuilder();
        
        // 生成文件头注释
        sb.AppendLine("// =============================================");
        sb.AppendLine("// 自动生成的UI绑定脚本");
        sb.AppendLine("// 预制体: " + prefabName);
        sb.AppendLine("// 生成时间: " + System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        sb.AppendLine("// =============================================");
        sb.AppendLine();
        sb.AppendLine("// 过滤逻辑说明：");
        sb.AppendLine("// - 此脚本根据用户设置的过滤条件自动生成");
        sb.AppendLine("// - 名称过滤：只绑定GameObject名称包含指定关键词的组件");
        sb.AppendLine("// - Tag过滤：只绑定GameObject Tag匹配指定值的组件");
        sb.AppendLine("// - 使用场景：适用于大型UI系统，实现组件的精细化管理");
        sb.AppendLine("// - 例如：动态UI元素可设置特定Tag，避免误绑定静态背景组件");
        sb.AppendLine();
        
        // 生成命名空间和类定义
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using TMPro;");
        sb.AppendLine();
        sb.AppendLine("public class " + prefabName + "Binder : MonoBehaviour");
        sb.AppendLine("{");
        
        // 生成字段声明
        sb.AppendLine("    #region 组件引用");
        foreach (var info in components)
        {
            sb.AppendLine("    [SerializeField] private " + info.ComponentType.Name + " " + info.FieldName + ";");
        }
        sb.AppendLine("    #endregion");
        
        sb.AppendLine();
        
        // 生成Awake方法
        sb.AppendLine("    private void Awake()");
        sb.AppendLine("    {");
        sb.AppendLine("        FindAndBindComponents();");
        sb.AppendLine("        BindEvents();");
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // 生成FindAndBindComponents方法
        sb.AppendLine("    #region 组件绑定");
        sb.AppendLine("    private void FindAndBindComponents()");
        sb.AppendLine("    {");
        foreach (var info in components)
        {
            sb.AppendLine("        " + info.FieldName + " = transform.Find(\"" + info.Path + "\").GetComponent<" + info.ComponentType.Name + ">();");
        }
        sb.AppendLine("    }");
        sb.AppendLine("    #endregion");
        sb.AppendLine();
        
        // 生成BindEvents方法
        sb.AppendLine("    #region 事件绑定");
        sb.AppendLine("    private void BindEvents()");
        sb.AppendLine("    {");
        foreach (var info in components)
        {
            if (IsInteractiveComponent(info.ComponentType))
            {
                GenerateEventBinding(sb, info);
            }
        }
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // 生成事件回调方法
        foreach (var info in components)
        {
            if (IsInteractiveComponent(info.ComponentType))
            {
                GenerateEventCallback(sb, info);
            }
        }
        sb.AppendLine("    #endregion");
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private bool IsInteractiveComponent(System.Type componentType)
    {
        return componentType == typeof(UnityEngine.UI.Button) ||
               componentType == typeof(UnityEngine.UI.Toggle) ||
               componentType == typeof(UnityEngine.UI.Slider) ||
               componentType == typeof(UnityEngine.UI.Dropdown) ||
               componentType == typeof(UnityEngine.UI.InputField) ||
               componentType == typeof(UnityEngine.UI.Scrollbar) ||
               componentType == typeof(UnityEngine.UI.ScrollRect);
    }
    
    private void GenerateEventBinding(StringBuilder sb, ComponentInfo info)
    {
        if (info.ComponentType == typeof(UnityEngine.UI.Button))
        {
            sb.AppendLine("        " + info.FieldName + ".onClick.AddListener(On" + info.FieldName + "Click);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Toggle))
        {
            sb.AppendLine("        " + info.FieldName + ".onValueChanged.AddListener(On" + info.FieldName + "Toggle);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Slider))
        {
            sb.AppendLine("        " + info.FieldName + ".onValueChanged.AddListener(On" + info.FieldName + "SliderChange);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Dropdown))
        {
            sb.AppendLine("        " + info.FieldName + ".onValueChanged.AddListener(On" + info.FieldName + "DropdownChange);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.InputField))
        {
            sb.AppendLine("        " + info.FieldName + ".onEndEdit.AddListener(On" + info.FieldName + "InputEnd);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Scrollbar))
        {
            sb.AppendLine("        " + info.FieldName + ".onValueChanged.AddListener(On" + info.FieldName + "ScrollbarChange);");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.ScrollRect))
        {
            sb.AppendLine("        " + info.FieldName + ".onValueChanged.AddListener(On" + info.FieldName + "Scroll);");
        }
    }
    
    private void GenerateEventCallback(StringBuilder sb, ComponentInfo info)
    {
        sb.AppendLine();
        sb.AppendLine("    private void On" + info.FieldName + "Click()");
        sb.AppendLine("    {");
        sb.AppendLine("        // TODO: 处理按钮点击事件");
        sb.AppendLine("    }");
        
        if (info.ComponentType == typeof(UnityEngine.UI.Toggle))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "Toggle(bool isOn)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理开关状态变化事件");
            sb.AppendLine("    }");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Slider))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "SliderChange(float value)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理滑动条值变化事件");
            sb.AppendLine("    }");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Dropdown))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "DropdownChange(int index)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理下拉菜单选择变化事件");
            sb.AppendLine("    }");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.InputField))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "InputEnd(string text)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理输入框输入完成事件");
            sb.AppendLine("    }");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.Scrollbar))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "ScrollbarChange(float value)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理滚动条值变化事件");
            sb.AppendLine("    }");
        }
        else if (info.ComponentType == typeof(UnityEngine.UI.ScrollRect))
        {
            sb.AppendLine();
            sb.AppendLine("    private void On" + info.FieldName + "Scroll(Vector2 position)");
            sb.AppendLine("    {");
            sb.AppendLine("        // TODO: 处理滚动视图滚动事件");
            sb.AppendLine("    }");
        }
    }
    
    private void MountScriptToPrefab(string scriptPath)
    {
        // 获取脚本资产
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        if (script != null)
        {
            // 获取预制体的实例（不会自动放入Hierarchy）
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (prefabInstance != null)
            {
                // 确保实例不在Hierarchy中
                prefabInstance.hideFlags = HideFlags.HideAndDontSave;
                
                // 检查是否已经有相同类型的组件
                System.Type scriptType = script.GetClass();
                if (prefabInstance.GetComponent(scriptType) == null)
                {
                    // 添加组件到预制体根物体
                    prefabInstance.AddComponent(scriptType);
                    
                    // 应用更改到预制体
                    PrefabUtility.SaveAsPrefabAsset(prefabInstance, AssetDatabase.GetAssetPath(prefab));
                }
                
                // 销毁临时实例
                GameObject.DestroyImmediate(prefabInstance);
            }
        }
    }
    
    private class ComponentInfo
    {
        public System.Type ComponentType;
        public string Path;
        public string FieldName;
        public Transform Transform;
        public string Abbreviation;
    }
}