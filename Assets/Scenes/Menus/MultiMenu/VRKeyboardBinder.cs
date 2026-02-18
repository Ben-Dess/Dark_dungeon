using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class VRKeyboardBinder : MonoBehaviour
{
    UIDocument doc;
    VisualElement root;

    void Awake()
    {
        doc = GetComponent<UIDocument>();
        root = doc.rootVisualElement;

        // Toutes les touches (boutons) qui commencent par "key_"
        var buttons = root.Query<Button>().ToList();
        foreach (var btn in buttons)
        {
            btn.clicked += () => OnButtonClicked(btn);
        }
    }
    void OnButtonClicked(Button btn)
    {
        if (VRKeyboard.Instance == null) return;

        var label = btn.text?.Trim();
        if (string.IsNullOrEmpty(label)) return;

        if (label == "EFFACER")
        {
            VRKeyboard.Instance.Backspace();
            VRKeyboard.Instance.RefocusTarget();
        }
        else if (label == "OK")
        {
            VRKeyboard.Instance.Hide();
        }
        else
        {
            VRKeyboard.Instance.AddCharacter(label);
            VRKeyboard.Instance.RefocusTarget();
        }
    }

}
