using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.AR;

public class ARPlacementExtension : MonoBehaviour
{
    public static event Action<GameObject> OnObjectPlacedEvent;

    // Subscribe to the objectPlaced event in ARPlacementInteractable
    private ARPlacementInteractable arPlacementInteractable;

    private void Awake()
    {
        arPlacementInteractable = GetComponent<ARPlacementInteractable>();
        arPlacementInteractable.objectPlaced.AddListener(OnObjectPlaced);
    }

    private void OnDestroy()
    {
        arPlacementInteractable.objectPlaced.RemoveListener(OnObjectPlaced);
    }

    private void OnObjectPlaced(ARObjectPlacementEventArgs args)
    {
        // Trigger your custom event and pass the placed object
        OnObjectPlacedEvent?.Invoke(args.placementObject);
    }
}
