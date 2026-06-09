using UnityEngine;

[DisallowMultipleComponent]
public class MotifIdentity : MonoBehaviour
{
    [SerializeField] private MotifDefinitionSO motif;

    public MotifDefinitionSO Motif => motif;

    public bool HasMotif => motif != null;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (motif == null)
            return;

        if (!string.IsNullOrWhiteSpace(motif.DisplayName))
        {
            gameObject.name = motif.DisplayName;
        }
    }
#endif
}