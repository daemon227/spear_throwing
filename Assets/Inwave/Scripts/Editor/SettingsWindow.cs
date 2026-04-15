using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.UIElements;

public class SettingsWindow : EditorWindow
{
    [MenuItem("Inwave/Production SDK/Settings")]
    public static void ShowSettings()
    {
        SettingsWindow wnd = GetWindow<SettingsWindow>();
        wnd.titleContent = new GUIContent("Settings");
    }

    public struct DefineSymbolData
    {
        public string Symbol;
        public string Description;

        public DefineSymbolData(string symbol,string description)
        {
            Symbol = symbol;
            Description = description;
        }
    }

    private static List<DefineSymbolData> _symbols = new List<DefineSymbolData>()
    {
        new DefineSymbolData("ADJUST", "Enable Adjust SDK"),
        new DefineSymbolData("FIREBASE_SDK", "Enable Firebase SDK"),
        new DefineSymbolData("MAX_ADS", "Enable MAX SDK"),
        new DefineSymbolData("IRONSOURCE_ADS", "Enable Ironsource SDK"),
        new DefineSymbolData("ADMOB", "Enable Admob SDK(7.2 only)"),
        new DefineSymbolData("DEBUG_ADS", "Enable Log Debug Ads"),
        new DefineSymbolData("DEBUG_ANALYTIC", "Enable Log Debug Firebase"),
        new DefineSymbolData("BANNER_COLLAPSIBLE","Enable Banner Collapsible")

    };

    public static void AddNewDefineSymbol(string symbol, string description)
    {
        bool isContain = false;
        for (int i = 0; i < _symbols.Count; i++)
        {
            if (_symbols[i].Symbol == symbol)
            {
                isContain = true;
                break;
            }
        }

        if (!isContain)
        {
            _symbols.Add(new DefineSymbolData(symbol,description));
        }
    }
    private List<Toggle> _toggles = new List<Toggle>();
    public void CreateGUI()
    {
        VisualElement container = rootVisualElement;

        CreateDefineSymbolToggles(container);

        Button applyButton = new Button
        {
            text = "Apply"
        };
        applyButton.clicked += ApplyDefineSymbols;

        container.Add(applyButton);

        InitToggles();
    }

    void InitToggles()
    {
        if (_toggles.Count != _symbols.Count)
        {
            return;
        }
        var namedBuildTarget = GetNamedBuildTarget();
        string[] symbols;
        PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out symbols);

        for (int i = 0; i < _toggles.Count; i++)
        {
            foreach (var symbol in symbols)
            {
                if (_symbols[i].Symbol == symbol)
                {
                    _toggles[i].value = true;
                    break;
                }
            }
        }

    }

    private static NamedBuildTarget GetNamedBuildTarget()
    {
        BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup targetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);
        NamedBuildTarget namedBuildTarget = NamedBuildTarget.FromBuildTargetGroup(targetGroup);
        return namedBuildTarget;
    }

    void CreateDefineSymbolToggles(VisualElement container)
    {
        foreach (var symbolData in _symbols)
        {
            Toggle toggle = new Toggle(symbolData.Description);
            container.Add(toggle);
            _toggles.Add(toggle);
        }
    }

    void ApplyDefineSymbols()
    {
        if (_symbols.Count != _toggles.Count)
        {
            return;
        }
        var namedBuildTarget = GetNamedBuildTarget();
        string[] symbols;
        PlayerSettings.GetScriptingDefineSymbols(namedBuildTarget, out symbols);

        List<string> applySymbolList = new List<string>(symbols);
        for (int i = 0; i < _toggles.Count; i++)
        {
            string sb = _symbols[i].Symbol;
            if (_toggles[i].value)
            {
                if (!symbols.Contains(sb))
                {
                    applySymbolList.Add(sb);
                }
            }
            else
            {
                if (symbols.Contains(sb))
                {
                    applySymbolList.Remove(sb);
                }
            }
        }

        PlayerSettings.SetScriptingDefineSymbols(namedBuildTarget,applySymbolList.ToArray());
    }
}