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


    private class BottleState
    {
        public Transform t;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scale;

        public Rigidbody rb;
        public XRGrabInteractable grab;

        public Collider[] colliders;
        public Renderer[] renderers;

        public Behaviour[] behavioursToReenable; // ex: PotionHoverName, autres scripts
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
                colliders = t.GetComponentsInChildren<Collider>(true),
                renderers = t.GetComponentsInChildren<Renderer>(true)
            };

            // On r�active au reset certains scripts typiques (hover label etc.)
            // Si tu veux en ajouter, mets-les ici.
            var listBehaviours = new List<Behaviour>();
            var hover = t.GetComponent<PotionHoverName>();
            if (hover != null) listBehaviours.Add(hover);

            st.behavioursToReenable = listBehaviours.ToArray();

            _states.Add(st);
        }

        Debug.Log($"[ResetManager] Cached {_states.Count} bottles initial states.");
    }

    public void DoReset()
    {
        // 1) reset chaudron
        if (cauldron != null)
            cauldron.ResetCauldron();

        // 2) reset bouteilles
        foreach (var st in _states)
        {
            if (st.t == null) continue;


            if (st.grab != null && st.grab.isSelected)
            {
                st.grab.enabled = false;
                st.grab.enabled = true;
            }


            if (st.colliders != null)
            {
                foreach (var c in st.colliders)
                    if (c != null) c.enabled = false;
            }

            // C) R�activer les renderers (IMPORTANT : sinon bouteille invisible)
            if (st.renderers != null)
            {
                foreach (var r in st.renderers)
                    if (r != null) r.enabled = true;
            }

            // D) R�activer Grab + scripts (IMPORTANT : sinon non-grabbable / pas de hover)
            if (st.grab != null) st.grab.enabled = true;

            if (st.behavioursToReenable != null)
            {
                foreach (var b in st.behavioursToReenable)
                    if (b != null) b.enabled = true;
            }

            // E) Remettre transform (position/rotation/scale)
            st.t.position = st.pos;
            st.t.rotation = st.rot;
            st.t.localScale = st.scale;

            // F) Remettre Rigidbody normal
            if (st.rb != null)
            {

                st.rb.isKinematic = false;
                st.rb.useGravity = true;

                st.rb.velocity = Vector3.zero;

                st.rb.angularVelocity = Vector3.zero;

                st.rb.Sleep();
                st.rb.WakeUp();
            }


            if (st.colliders != null)
            {
                foreach (var c in st.colliders)
                    if (c != null) c.enabled = true;
            }
        }

        Debug.Log("[ResetManager] Reset done (bottles + cauldron).");
    }
}
