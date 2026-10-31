using UnityEditor;
using UnityEngine;

// Leader portraits are 300 x 400 pixel-art paintings. Unity's default import scaled them to 256 x 512, compressed them and
// added mipmaps, which blurred them (owner request, 31 Oct 2026: sharpen the diplomacy portraits). Imported here at their
// true size, uncompressed and point-filtered; the game makes a 2x pixel-perfect copy for display (VisualOverhaul.Portrait).
// The picture files themselves are unchanged.
public class LeaderImport : AssetPostprocessor
{
    public override uint GetVersion(){return 2;}
    void OnPreprocessTexture()
    {
        if(!assetPath.Replace('\\','/').Contains("/Resources/Leaders/"))return;
        var importer=(TextureImporter)assetImporter;
        importer.npotScale=TextureImporterNPOTScale.None;importer.mipmapEnabled=false;
        importer.textureCompression=TextureImporterCompression.Uncompressed;importer.filterMode=FilterMode.Point;
        importer.wrapMode=TextureWrapMode.Clamp;
    }
    // Unity -batchmode -quit -projectPath <Unity> -executeMethod LeaderImport.Reimport
    public static void Reimport()
    {
        foreach(var guid in AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets/Resources/Leaders"}))
            AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid),ImportAssetOptions.ForceUpdate);
        AssetDatabase.Refresh();
        Debug.Log("Leader portraits reimported at full size.");
    }
}
