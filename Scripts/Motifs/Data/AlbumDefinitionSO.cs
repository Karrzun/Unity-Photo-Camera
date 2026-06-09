using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Album", menuName = "Photo Camera/Album")]
public class AlbumDefinitionSO : ScriptableObject
{
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Rewards")]
    [SerializeField] private int completionReward = 100;

    [Header("Motifs")]
    [SerializeField] private List<MotifDefinitionSO> motifs = new();

    public string Id => id;
    public string DisplayName => displayName;
    public int CompletionReward => completionReward;
    public IReadOnlyList<MotifDefinitionSO> Motifs => motifs; 

    public bool Contains(MotifDefinitionSO motif)
    {
        return motif != null && motifs.Contains(motif);
    }
}