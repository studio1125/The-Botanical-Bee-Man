using UnityEngine;

public class ResearchUIManager : BaseUIManager {

    [Header("UI References")]
    [SerializeField] private RectTransform laptopMenu;

    private new void Start() {

        base.Start();
        RefreshLayout(laptopMenu); // refresh the layout of the laptop menu at the start to ensure all UI elements are properly aligned

    }
}
