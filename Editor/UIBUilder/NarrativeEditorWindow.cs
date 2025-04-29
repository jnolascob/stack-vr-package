using System.Xml.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

public class NarrativeEditorWindow : EditorWindow {
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    public static VisualElement templateContainerElement;

    [MenuItem("Window/UI Toolkit/TestEditorWindow")]
    public static void ShowExample() {
        NarrativeEditorWindow wnd = GetWindow<NarrativeEditorWindow>();
        wnd.titleContent = new GUIContent("TestEditorWindow");
    }

    public void CreateGUI() {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // VisualElements objects can contain other VisualElement following a tree hierarchy.
        //VisualElement label = new Label("Hello World! From C#");
        //root.Add(label);
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.singularisvr.stackvr/Editor/UIBUilder/NarrativeEditorWindow.uss");
        rootVisualElement.styleSheets.Add(styleSheet);

        TemplateContainer templateContainer = m_VisualTreeAsset.CloneTree();
        templateContainer.style.display = DisplayStyle.Flex;
        templateContainer.style.position = Position.Absolute;
        templateContainer.style.left = 0;
        templateContainer.style.top = 0;
        templateContainer.style.right = 0;
        templateContainer.style.bottom = 0;


        // Instantiate UXML
        VisualElement labelFromUXML = templateContainer;
        root.Add(labelFromUXML);
        templateContainerElement = templateContainer;
        LoadDocument();




    }


    public void LoadDocument() {
        var root = templateContainerElement;
        var button = root.Q<Button>("TestButton");
        button.RegisterCallback<ClickEvent>((e) => { Debug.Log("You Clicked a button"); });
        //var graphViewExperince = root.Q<GraphVi>()


    }
}
