using UnityEngine;

public enum MotifCategory
{
    Animal,
    Plant,
    Object
}

[CreateAssetMenu(fileName = "New Motif", menuName = "Photo Camera/Motif")]
public class MotifDefinitionSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;
    [SerializeField] private string scientificName;
    [SerializeField] private MotifCategory category;

    [Header("Rewards")]
    [SerializeField] private int rewardAmount = 10;

    public string Id => id;
    public string DisplayName => displayName;
    public string ScientificName => scientificName;
    public MotifCategory Category => category;
    public int RewardAmount => rewardAmount;

    public string GetInfoText()
    {
        if (!string.IsNullOrWhiteSpace(scientificName))
            return $"{displayName}\n{scientificName}";

        return $"{displayName}";
    }
}