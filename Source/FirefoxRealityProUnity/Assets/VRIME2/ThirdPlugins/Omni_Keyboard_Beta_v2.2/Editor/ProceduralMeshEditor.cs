using UnityEditor;

namespace Htc.Omni
{
    public class ProceduralMeshEditor
    {
        [MenuItem("Assets/Create/Omni/Procedural Box")]
        public static void CreateBox()
        {
            AssetUtility.CreateAsset<ProceduralPanel>();
        }
    }
}