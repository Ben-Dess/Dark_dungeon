using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Collider))]
public class ScepterSocket : MonoBehaviour
{
    [Header("Bind")]
    public Scepter scepter;
    public Transform attachPoint;

    [Header("Collision fix")]
    public bool ignoreCollisionsWithScepter = true;
    public Collider[] scepterColliders;

    [Tooltip("Option: désactive les colliders de la gemme pendant qu'elle est socketée (évite forces).")]
    public bool disableGemCollidersWhenSocketed = true;

    [Header("Swap / Safety")]
    public float cooldown = 0.1f;

    [Header("Drop Settings")]
    [Tooltip("Offset où la gemme éjectée apparaît (relatif au sceptre). x=droite, y=haut, z=avant")]
    public Vector3 dropOffset = new Vector3(0.3f, 0, 0);

    private Gem candidate;
    private XRGrabInteractable candidateGrab;

    private Gem current;
    private XRGrabInteractable currentGrab;

    private float nextAllowedTime;
    private bool isProcessingSwap = false;

    void Awake()
    {
        if (scepter == null) scepter = GetComponentInParent<Scepter>();
        if (attachPoint == null && scepter != null) attachPoint = scepter.gemAttachPoint;

        if (ignoreCollisionsWithScepter && (scepterColliders == null || scepterColliders.Length == 0) && scepter != null)
            scepterColliders = scepter.GetComponentsInChildren<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (isProcessingSwap) return;

        var gem = other.GetComponentInParent<Gem>();
        if (gem == null) return;
        if (gem == current) return;

        SetCandidate(gem);
    }

    void OnTriggerExit(Collider other)
    {
        var gem = other.GetComponentInParent<Gem>();
        if (gem == null) return;

        if (gem == candidate)
            ClearCandidate();
    }

    private void SetCandidate(Gem gem)
    {
        ClearCandidate();

        candidate = gem;
        EnsureGemRefs(candidate);

        candidateGrab = candidate.GetComponent<XRGrabInteractable>();
        if (candidateGrab != null)
        {
            candidateGrab.selectExited.AddListener(OnGemReleased);
        }
    }

    private void ClearCandidate()
    {
        if (candidateGrab != null)
        {
            candidateGrab.selectExited.RemoveListener(OnGemReleased);
        }

        candidate = null;
        candidateGrab = null;
    }

    private void OnGemReleased(SelectExitEventArgs args)
    {
        if (isProcessingSwap) return;
        if (Time.time < nextAllowedTime) return;

        nextAllowedTime = Time.time + cooldown;

        var releasedGem = args.interactableObject.transform.GetComponentInParent<Gem>();
        if (releasedGem == null || releasedGem != candidate) return;

        Attach(releasedGem);
    }

    private void Attach(Gem newGem)
    {
        if (newGem == null || isProcessingSwap) return;

        isProcessingSwap = true;

        Debug.Log($"[Socket] ==========================================");
        Debug.Log($"[Socket] ATTACH: nouvelle={newGem.name}, actuelle={current?.name ?? "null"}");

        // ÉTAPE 1 : Éjecter l'ancienne gemme si elle existe
        if (current != null && current != newGem)
        {
            Debug.Log($"[Socket] Drop de l'ancienne gemme: {current.name}");
            DropGem(current);
        }

        // ÉTAPE 2 : Nettoyer les variables
        if (currentGrab != null)
        {
            currentGrab.selectExited.RemoveListener(OnCurrentGemReleased);
            currentGrab = null;
        }
        current = null;

        // ÉTAPE 3 : Attacher la nouvelle gemme
        current = newGem;
        ClearCandidate();
        EnsureGemRefs(current);

        // Listener
        currentGrab = current.GetComponent<XRGrabInteractable>();
        if (currentGrab != null)
        {
            currentGrab.throwOnDetach = false;
            currentGrab.selectExited.AddListener(OnCurrentGemReleased);
        }

        // ORDRE CRITIQUE POUR LA PREMIÈRE GEMME :

        // 1. Parenting EN PREMIER (pendant que la physique est encore active)
        current.transform.SetParent(attachPoint, false);
        current.transform.localPosition = Vector3.zero;
        current.transform.localRotation = Quaternion.identity;

        // 2. ENSUITE désactiver la physique
        if (current.rb != null)
        {
            current.rb.linearVelocity = Vector3.zero;
            current.rb.angularVelocity = Vector3.zero;
            current.rb.isKinematic = true;
            current.rb.useGravity = false;
        }

        // 3. Forcer la synchronisation pour être sûr que la position est appliquée
        Physics.SyncTransforms();

        // 4. Ignore collisions
        if (ignoreCollisionsWithScepter && scepterColliders != null && current.colliders != null)
        {
            foreach (var gc in current.colliders)
            {
                if (gc == null) continue;
                foreach (var sc in scepterColliders)
                {
                    if (sc == null) continue;
                    Physics.IgnoreCollision(gc, sc, true);
                }
            }
        }

        // 5. Désactiver les colliders EN DERNIER (optionnel mais stable)
        if (disableGemCollidersWhenSocketed && current.colliders != null)
        {
            foreach (var c in current.colliders)
                if (c != null) c.enabled = false;
        }

        // 6. Notifier scepter
        scepter?.Equip(current);

        Debug.Log($"[Socket] {newGem.name} attachée avec succès à position {current.transform.position}");
        Debug.Log($"[Socket] ==========================================");

        isProcessingSwap = false;
    }

    private void OnCurrentGemReleased(SelectExitEventArgs args)
    {
        DetachCurrent();
    }

    public void DetachCurrent()
    {
        if (current == null || isProcessingSwap) return;

        isProcessingSwap = true;

        Debug.Log($"[Socket] Détachement: {current.name}");

        Gem gem = current;
        current = null;

        if (currentGrab != null)
        {
            currentGrab.selectExited.RemoveListener(OnCurrentGemReleased);
            currentGrab = null;
        }

        ReleaseGem(gem);

        isProcessingSwap = false;
    }

    private void DropGem(Gem gem)
    {
        if (gem == null) return;

        Debug.Log($"[Socket] DropGem START: {gem.name}");

        EnsureGemRefs(gem);

        // Réactiver colliders EN PREMIER
        if (disableGemCollidersWhenSocketed && gem.colliders != null)
        {
            foreach (var c in gem.colliders)
                if (c != null) c.enabled = true;
        }

        // Re-enable collisions
        if (ignoreCollisionsWithScepter && scepterColliders != null && gem.colliders != null)
        {
            foreach (var gc in gem.colliders)
            {
                if (gc == null) continue;
                foreach (var sc in scepterColliders)
                {
                    if (sc == null) continue;
                    Physics.IgnoreCollision(gc, sc, false);
                }
            }
        }

        // Unparent AVANT de bouger
        gem.transform.SetParent(null, true);

        // Déplacer la gemme à côté du sceptre
        Vector3 worldDropPosition = transform.TransformPoint(dropOffset);
        gem.transform.position = worldDropPosition;
        gem.transform.rotation = Quaternion.identity;

        // Forcer la synchronisation des transforms
        Physics.SyncTransforms();

        // Physique ON
        if (gem.rb != null)
        {
            gem.rb.isKinematic = false;
            gem.rb.useGravity = true;
            gem.rb.linearVelocity = Vector3.zero;
            gem.rb.angularVelocity = Vector3.zero;
            gem.rb.WakeUp();
        }

        // Notifier scepter
        scepter?.Unequip(gem);

        Debug.Log($"[Socket] DropGem END: {gem.name} droppée à {worldDropPosition}");
    }

    private void ReleaseGem(Gem gem)
    {
        if (gem == null) return;

        Debug.Log($"[Socket] ReleaseGem START: {gem.name}");

        EnsureGemRefs(gem);

        // Réactiver colliders
        if (disableGemCollidersWhenSocketed && gem.colliders != null)
        {
            foreach (var c in gem.colliders)
                if (c != null) c.enabled = true;
        }

        // Re-enable collisions
        if (ignoreCollisionsWithScepter && scepterColliders != null && gem.colliders != null)
        {
            foreach (var gc in gem.colliders)
            {
                if (gc == null) continue;
                foreach (var sc in scepterColliders)
                {
                    if (sc == null) continue;
                    Physics.IgnoreCollision(gc, sc, false);
                }
            }
        }

        // Unparent
        gem.transform.SetParent(null, true);

        // Forcer la synchronisation
        Physics.SyncTransforms();

        // Physique ON
        if (gem.rb != null)
        {
            gem.rb.isKinematic = false;
            gem.rb.useGravity = true;
            gem.rb.linearVelocity = Vector3.zero;
            gem.rb.angularVelocity = Vector3.zero;
            gem.rb.WakeUp();
        }

        scepter?.Unequip(gem);

        Debug.Log($"[Socket] ReleaseGem END: {gem.name}");
    }

    private void EnsureGemRefs(Gem gem)
    {
        if (gem.rb == null) gem.rb = gem.GetComponent<Rigidbody>();
        if (gem.colliders == null || gem.colliders.Length == 0)
            gem.colliders = gem.GetComponentsInChildren<Collider>();
    }

    void OnDestroy()
    {
        ClearCandidate();

        if (currentGrab != null)
        {
            currentGrab.selectExited.RemoveListener(OnCurrentGemReleased);
            currentGrab = null;
        }
    }
}