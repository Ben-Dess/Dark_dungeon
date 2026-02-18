using UnityEngine;
using UnityEngine.UIElements;

public class VRKeyboard : MonoBehaviour
{
    public static VRKeyboard Instance;

    UIDocument document;
    VisualElement root;
    TextField targetField;

    void Awake()
    {
        Instance = this;
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;
        root.style.display = DisplayStyle.None;
    }

    public void Show(TextField field)
    {
        targetField = field;
        root.style.display = DisplayStyle.Flex;
    }
    public void RefocusTarget()
    {
        if (targetField == null) return;
        targetField.Focus();
    }

    public void Hide()
    {
        root.style.display = DisplayStyle.None;
        targetField = null;
    }

    public void AddCharacter(string c)
    {
        if (targetField == null) return;
        targetField.value += c;
    }

    public void Backspace()
    {
        if (targetField == null) return;
        if (targetField.value.Length > 0)
            targetField.value =
                targetField.value.Substring(0, targetField.value.Length - 1);
    }
}
