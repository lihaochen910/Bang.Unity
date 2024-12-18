using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using Bang.Systems;
using Gilzoide.EasyProjectSettings;


namespace Bang.Unity.Editor {
    
	// 实现 IPreprocessBuildWithReport 接口以在构建前执行
    public class BangUnityLinkXmlPreprocessBuild : IPreprocessBuildWithReport
    {
        // 设置执行顺序，数字越小越早执行
        public int callbackOrder => 0;

        // 构建前执行的方法
        public void OnPreprocessBuild(BuildReport report)
        {
            GenerateLinkXml();
        }

        public static void GenerateLinkXml()
        {
            Debug.Log("[Bang.Unity] Generating link.xml before build...");
            
            // 获取 BangAppSettings
            ProjectSettings.TryLoad<BangAppSettings>( out var settings );
            if (settings == null)
            {
                Debug.LogError("Failed to load BangAppSettings");
                return;
            }

            // 收集需要保留的类型
            HashSet<Type> preservedTypes = new HashSet<Type>();
            preservedTypes.Add( typeof( Bang.World ) );
            preservedTypes.Add( typeof( Bang.Entities.Entity ) );
            preservedTypes.Add( typeof( Bang.Components.IComponent ) );
            preservedTypes.Add( typeof( Bang.StateMachines.StateMachine ) );
            preservedTypes.Add( typeof( Bang.StateMachines.IStateMachineComponent ) );
            preservedTypes.Add( typeof( Bang.Unity.UnityWorld ) );
            
            // 遍历所有程序集中的IComponent类型
            try {
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach ( var assembly in assemblies ) {
                    try {
                        var assemblyTypes = assembly.GetTypes();
                        var componentTypes = assemblyTypes.Where( t =>
                                                typeof( Bang.Components.IComponent ).IsAssignableFrom( t ) &&
                                                !t.IsInterface && !t.IsAbstract );
                        foreach ( var type in componentTypes ) {
                            preservedTypes.Add( type );
                            // Debug.Log($"Adding component type to preserve list: {type.FullName}");
                        }
                        
                        var stateMachineTypes = assemblyTypes.Where( t =>
                            typeof( Bang.StateMachines.StateMachine ).IsAssignableFrom( t ) &&
                            !t.IsInterface && !t.IsAbstract );
                        foreach ( var type in stateMachineTypes ) {
                            preservedTypes.Add( type );
                            // Debug.Log($"Adding component type to preserve list: {type.FullName}");
                        }
                        
                        var componentsLookupTypes = assemblyTypes.Where( t =>
                            !t.IsInterface && !t.IsAbstract &&
                            typeof( Bang.ComponentsLookup ).IsAssignableFrom( t ) );
                        foreach ( var type in componentsLookupTypes ) {
                            preservedTypes.Add( type );
                        }
                    }
                    catch ( Exception assemblyEx ) {
                        Debug.LogWarning( $"[Bang.Unity] 无法从程序集 {assembly.FullName} 加载类型: {assemblyEx.Message}" );
                    }
                }
            }
            catch ( Exception e ) {
                Debug.LogError( $"[Bang.Unity] 收集组件类型时出错: {e}" );
            }

            try
            {
                // 遍历 MainFeatures
                if (settings.MainFeatures != null)
                {
                    foreach (var (systemType, isActive) in settings.MainFeatures.FetchAllSystems(enabled: true))
                    {
                        if (systemType != null)
                        {
                            // 检查 System 是否激活
                            // if (isActive)
                            if (true)
                            {
                                preservedTypes.Add(systemType);
                                // Debug.Log($"Adding active system to preserve list: {systemType.FullName}");
                                
                                // 遍历系统类型的特性
                                var attributes = systemType.GetCustomAttributes(true);
                                foreach (var attribute in attributes)
                                {
                                    // 检查 FilterAttribute
                                    if (attribute is FilterAttribute filterAttr)
                                    {
                                        foreach (var type in filterAttr.Types)
                                        {
                                            preservedTypes.Add(type);
                                        }
                                    }
                                    
                                    // 检查 WatchAttribute 
                                    if (attribute is WatchAttribute watchAttr)
                                    {
                                        foreach (var type in watchAttr.Types)
                                        {
                                            preservedTypes.Add(type);
                                        }
                                    }
                                    
                                    // 检查 MessagerAttribute 
                                    if (attribute is MessagerAttribute messagerAttr)
                                    {
                                        foreach (var type in messagerAttr.Types)
                                        {
                                            preservedTypes.Add(type);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bang.Unity] Error while collecting system types: {e}");
                return;
            }

            if (!preservedTypes.Any())
            {
                Debug.LogWarning("[Bang.Unity] No active systems found to preserve!");
                return;
            }

            preservedTypes = preservedTypes.Distinct().ToHashSet();

            // 生成 link.xml 内容
            var xmlContent = new StringBuilder();
            xmlContent.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
            xmlContent.AppendLine("<linker>");

            // 按程序集分组组织类型
            var typesByAssembly = preservedTypes
                .GroupBy(t => t.Assembly.GetName().Name)
                .OrderBy(g => g.Key);

            foreach (var assemblyGroup in typesByAssembly)
            {
                xmlContent.AppendLine($"  <assembly fullname=\"{assemblyGroup.Key}\">");
                
                foreach (var type in assemblyGroup.OrderBy(t => t.FullName))
                {
                    xmlContent.AppendLine($"    <type fullname=\"{type.FullName}\" preserve=\"all\"/>");
                }
                
                xmlContent.AppendLine("  </assembly>");
            }

            xmlContent.AppendLine("</linker>");

            // 保存到 Assets 文件夹
            string filePath = Path.Combine(Application.dataPath, "link.xml");
            try
            {
                File.WriteAllText(filePath, xmlContent.ToString());
                AssetDatabase.Refresh();
                Debug.Log($"[Bang.Unity] Successfully generated link.xml at: {filePath}");
            }
            catch (Exception e)
            {
                Debug.LogError($"[Bang.Unity] Failed to write link.xml: {e.Message}");
                // 在构建过程中遇到错误时抛出异常
                throw new BuildFailedException(e);
            }
        }
    }

    // 保留编辑器工具窗口（可选）
    public class BangUnityLinkXmlGeneratorWindow : EditorWindow
    {
        [MenuItem("Tools/Bang.Unity/Generator link.xml", isValidateFunction: false, priority: 1)]
        public static void ShowWindow()
        {
            // GetWindow<BangUnityLinkXmlGeneratorWindow>("link.xml Generator");
            BangUnityLinkXmlPreprocessBuild.GenerateLinkXml();
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("link.xml Generator", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Generate link.xml"))
            {
                BangUnityLinkXmlPreprocessBuild.GenerateLinkXml();
            }

            EditorGUILayout.HelpBox(
                "This tool generates a link.xml file based on active systems in MainFeatures.\n" +
                "The file will be created in the Assets folder.\n" +
                "Note: link.xml is automatically generated before each build.",
                MessageType.Info
            );
        }
    }

}
