using System;
using UnityEngine;

// Put on an empty GameObject in the scene called RevenueManager.
public class RevenueManager : MonoBehaviour
{
    public static RevenueManager Instance { get; private set; }

    [Tooltip("Target for the revenue bar (e.g. the night's goal)")]
    [SerializeField] private int revenueGoal = 20;

    public int Cash { get; private set; }
    public int RevenueGoal => revenueGoal;
    public event Action OnChanged;

    private void Awake() => Instance = this;
    private void Start() => OnChanged?.Invoke();

    public void AddCash(int amount)
    {
        Cash += amount;
        OnChanged?.Invoke();
    }
}
