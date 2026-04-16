using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class AutoUIPrefabBinder : EditorWindow
{
    private GameObject prefab;
    private bool autoRegisterEvents = true;
    private bool useTransformFind = true;
    private bool overwriteExistingScript = false;
    
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
        
        // 选项设置
        GUILayout.Label("选项:", EditorStyles.boldLabel);
        autoRegisterEvents = EditorGUILayout.ToggleLeft("自动注册事件并生成空方法", autoRegisterEvents);
        useTransformFind = EditorGUILayout.ToggleLeft("使用Transform.Find (否则直接拖拽引用)", useTransformFind);
        overwriteExistingScript = EditorGUILayout.ToggleLeft("覆盖已存在的绑定脚本", overwriteExistingScript);
        
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
        GUILayout.Label("2. 选择所需选项");
        GUILayout.Label("3. 点击生成按钮");
        GUILayout.Label("4. 脚本会自动生成并挂载到预制体上");
        
        GUILayout.Space(10);
        
        // 使用示例
        GUILayout.Label("使用示例:", EditorStyles.boldLabel);
        GUILayout.Label("- 拖入 Assets/Prefabs/LoginPanel.prefab");
        GUILayout.Label("- 点击按钮后自动生成 LoginPanelBinder.cs");
        GUILayout.Label("- 脚本会自动挂载到预制体上");
        GUILayout.Label("- 所有按钮的点击事件已经绑定到生成的空方法");
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
        
        // 生成脚本内容
        string scriptContent = GenerateScriptContent(prefab.name, components);
        
        // 保存脚本
        string prefabPath = AssetDatabase.GetAssetPath(prefab);
        string scriptPath = Path.Combine(Path.GetDirectoryName(prefabPath), prefab.name + "Binder.cs");
        
        // 检查是否覆盖
        if (File.Exists(scriptPath) && !overwriteExistingScript)
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
        
        EditorUtility.DisplayDialog("成功", "绑定脚本生成并挂载成功！", "确定");
    }
    
    private List<ComponentInfo> AnalyzePrefabStructure(Transform parent)
    {
        List<ComponentInfo> components = new List<ComponentInfo>();
        
        // 遍历所有子物体
        foreach (Transform child in parent)
        {
            // 检查常见UGUI组件
            CheckAndAddComponent<UnityEngine.UI.Button>(child, components);
            CheckAndAddComponent<UnityEngine.UI.Text>(child, components);
            CheckAndAddComponent<TMPro.TextMeshProUGUI>(child, components);
            CheckAndAddComponent<UnityEngine.UI.Image>(child, components);
            CheckAndAddComponent<UnityEngine.UI.Toggle>(child, components);
            CheckAndAddComponent<UnityEngine.UI.Slider>(child, components);
            CheckAndAddComponent<UnityEngine.UI.InputField>(child, components);
            CheckAndAddComponent<UnityEngine.UI.ScrollRect>(child, components);
            
            // 递归检查子物体
            components.AddRange(AnalyzePrefabStructure(child));
        }
        
        return components;
    }
    
    private void CheckAndAddComponent<T>(Transform transform, List<ComponentInfo> components) where T : Component
    {
        T component = transform.GetComponent<T>();
        if (component != null)
        {
            string path = GetTransformPath(transform, prefab.transform);
            string componentName = typeof(T).Name;
            
            // 生成字段名
            string fieldName = GenerateFieldName(path, componentName);
            
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
                Transform = transform
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
    
    private string GenerateFieldName(string path, string componentName)
    {
        // 移除路径分隔符，使用下划线连接
        string name = path.Replace("/", "_");
        // 首字母大写
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name.Substring(1);
        }
        // 添加组件类型前缀
        return componentName + "_" + name;
    }
    
    private string GenerateScriptContent(string prefabName, List<ComponentInfo> components)
    {
        StringBuilder sb = new StringBuilder();
        
        // 生成命名空间和类定义
        sb.AppendLine("using UnityEngine;");
        sb.AppendLine("using UnityEngine.UI;");
        sb.AppendLine("using TMPro;");
        sb.AppendLine();
        sb.AppendLine("public class " + prefabName + "Binder : MonoBehaviour");
        sb.AppendLine("{");
        
        // 生成字段声明
        foreach (var info in components)
        {
            sb.AppendLine("    [SerializeField] private " + info.ComponentType.Name + " " + info.FieldName + ";");
        }
        
        sb.AppendLine();
        
        // 生成Awake方法
        sb.AppendLine("    private void Awake()");
        sb.AppendLine("    {");
        sb.AppendLine("        FindAndBindComponents();");
        if (autoRegisterEvents)
        {
            sb.AppendLine("        BindEvents();");
        }
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // 生成FindAndBindComponents方法
        sb.AppendLine("    private void FindAndBindComponents()");
        sb.AppendLine("    {");
        foreach (var info in components)
        {
            if (useTransformFind)
            {
                sb.AppendLine("        " + info.FieldName + " = transform.Find(\"" + info.Path + "\").GetComponent<" + info.ComponentType.Name + ">();");
            }
        }
        sb.AppendLine("    }");
        sb.AppendLine();
        
        // 生成BindEvents方法
        if (autoRegisterEvents)
        {
            sb.AppendLine("    private void BindEvents()");
            sb.AppendLine("    {");
            foreach (var info in components)
            {
                if (info.ComponentType == typeof(UnityEngine.UI.Button))
                {
                    sb.AppendLine("        " + info.FieldName + ".onClick.AddListener(On" + info.FieldName + ");");
                }
            }
            sb.AppendLine("    }");
            sb.AppendLine();
            
            // 生成按钮点击事件方法
            foreach (var info in components)
            {
                if (info.ComponentType == typeof(UnityEngine.UI.Button))
                {
                    sb.AppendLine("    private void On" + info.FieldName + "()");
                    sb.AppendLine("    {");
                    sb.AppendLine("        // TODO: 实现按钮点击逻辑");
                    sb.AppendLine("    }");
                    sb.AppendLine();
                }
            }
        }
        
        sb.AppendLine("}");
        
        return sb.ToString();
    }
    
    private void MountScriptToPrefab(string scriptPath)
    {
        // 获取脚本资产
        MonoScript script = AssetDatabase.LoadAssetAtPath<MonoScript>(scriptPath);
        if (script != null)
        {
            // 获取预制体的实例
            GameObject prefabInstance = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
            if (prefabInstance != null)
            {
                // 检查是否已经有相同类型的组件
                System.Type scriptType = script.GetClass();
                if (prefabInstance.GetComponent(scriptType) == null)
                {
                    // 添加组件
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
    }
}