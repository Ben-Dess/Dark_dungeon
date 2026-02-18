using System.Collections.Generic;
using UnityEngine;

using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class ResetManager : MonoBehaviour
{
    [Header("What to reset")]
    [Tooltip("Glisse ici toutes les bouteilles (instances dans la sc�ne).")]
    public List<Transform> bottles = new();

    [Tooltip("Optionnel : r�f�rence au chaudron pour le reset.")]
    public CauldronMixer cauldron;

    // �tat initial
    private class BottleState
    {
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;
        public Rigidbody rb;
        public XRGrabInteractable grab;
        public Collider[] colliders;
    }

    private readonly List<BottleState> _states = new();

    void Awake()
    {
        CacheInitialStates();
    }

    public void CacheInitialStates()
    {
        _states.Clear();

        foreach (var t in bottles)
        {
            if (t == null) continue;

            var st = new BottleState
            {
                t = t,
                pos = t.position,
                rot = t.rotation,
                scale = t.localScale,
                rb = t.GetComponent<Rigidbody>(),
                grab = t.GetComponent<XRGrabInteractable>(),
                colliders = t.GetComponentsInChildren<Collider>(true)
            };

            _states.Add(st);
        }

        Debug.Log($"[ResetManager] Cached {_states.Count} bottles initial states.");
    }

    public void DoReset()
    {
        // 1) Reset chaudron
        if (cauldron != null)
            cauldron.ResetCauldron();

        // 2) Reset bouteilles
        foreach (var st in _states)
        {
            if (st.t == null) continue;

            // A) Forcer le "drop" si l'objet est grab
            // (m�thode compatible: d�sactiver/r�activer l'interactable)
            if (st.grab != null && st.grab.isSelected)
            {
                st.grab.enabled = false;
                st.grab.enabled = true;
            }

            // B) D�sactiver colliders 1 frame (�vite explosions de physique)
            if (st.colliders != null)
            {
                foreach (var c in st.colliders)
                    if (c != null) c.enabled = false;
            }

            // C) Remettre transform
            st.t.position = st.pos;
            st.t.rotation = st.rot;
            st.t.localScale = st.scale;

            // D) Reset Rigidbody
            if (st.rb != null)
            {
                st.rb.linearVelocity = Vector3.zero;
                st.rb.angularVelocity = Vector3.zero;

                // Certaines versions Unity recommandent linearVelocity, mais velocity marche encore.
                // Si tu veux, je te le remplace.
                st.rb.Sleep();
                st.rb.WakeUp();
            }

            // E) R�activer colliders
            if (st.colliders != null)
            {
                foreach (var c in st.colliders)
                    if (c != null) c.enabled = true;
            }
        }

        Debug.Log("[ResetManager] Reset done.");
    }
}
