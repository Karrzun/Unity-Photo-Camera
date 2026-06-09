#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[CreateAssetMenu(fileName = "New Photo Database", menuName = "Photo Camera/Photo Database")]
public class PhotoDatabaseSO : ScriptableObject
{
    [SerializeField] private List<AlbumDefinitionSO> albums = new();

    public IReadOnlyList<AlbumDefinitionSO> Albums => albums;

    private IEnumerable<MotifDefinitionSO> allMotifs;
    public IEnumerable<MotifDefinitionSO> AllMotifs
    {
        get
        {
            allMotifs ??= albums
                            .Where(album => album != null)
                            .SelectMany(album => album.Motifs)
                            .Where(motif => motif != null)
                            .Distinct();
            return allMotifs;
        }
    }

    public IEnumerable<AlbumDefinitionSO> GetAlbumsContaining(MotifDefinitionSO motif)
    {
        if (motif == null)
            return Enumerable.Empty<AlbumDefinitionSO>();

        return albums.Where(album => album != null && album.Contains(motif));
    }



#if UNITY_EDITOR
    [ContextMenu("Rebuild")]
    public void Rebuild()
    {
        albums = FindAssetsOfType<AlbumDefinitionSO>();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    private static List<T> FindAssetsOfType<T>() where T : ScriptableObject
    {
        string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
        List<T> assets = new();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
                assets.Add(asset);
        }

        return assets;
    }
#endif
}