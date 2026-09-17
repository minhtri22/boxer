using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BoxerP0.Editor
{
    public static class Blender3DAssetAudit
    {
        public static void Run()
        {
            GameObject opponent = Resources.Load<GameObject>("Boxer3D/Ramirez_UAT3");
            GameObject glove = Resources.Load<GameObject>("Boxer3D/PlayerPOVGlove_UAT3");
            if (opponent == null) throw new Exception("Missing Resources/Boxer3D/Ramirez_UAT3.fbx");
            if (glove == null) throw new Exception("Missing Resources/Boxer3D/PlayerPOVGlove_UAT3.fbx");

            Audit("RAMIREZ", opponent);
            Audit("PLAYER_POV", glove);
            Debug.Log("BLENDER_3D_ASSET_AUDIT=PASS");
        }

        private static void Audit(string label, GameObject prefab)
        {
            Transform[] transforms = prefab.GetComponentsInChildren<Transform>(true);
            MeshFilter[] meshes = prefab.GetComponentsInChildren<MeshFilter>(true);
            SkinnedMeshRenderer[] skinned = prefab.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            int triangles = meshes.Sum(m => m.sharedMesh == null ? 0 : m.sharedMesh.triangles.Length / 3)
                + skinned.Sum(m => m.sharedMesh == null ? 0 : m.sharedMesh.triangles.Length / 3);
            Debug.Log($"{label}_TRANSFORMS={transforms.Length} MESH_FILTERS={meshes.Length} SKINNED={skinned.Length} TRIANGLES={triangles}");
            foreach (Transform t in transforms)
                Debug.Log($"{label}_NODE={Path(t, prefab.transform)} local={t.localPosition:R} rot={t.localEulerAngles:R} scale={t.localScale:R}");
        }

        private static string Path(Transform t, Transform root)
        {
            if (t == root) return root.name;
            string value = t.name;
            while (t.parent != null && t.parent != root)
            {
                t = t.parent;
                value = t.name + "/" + value;
            }
            return root.name + "/" + value;
        }
    }
}
