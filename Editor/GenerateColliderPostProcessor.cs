using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace OT.Import.Editor
{
    public class GenerateColliderPostProcessor : AssetPostprocessor
    {
        [MenuItem("Tools/Better Collider Generation")]
        static void ToggleColliderGeneration()
        {
            var betterColliderGenerationEnabled = EditorPrefs.GetBool("BetterColliderGeneration", false);
            EditorPrefs.SetBool("BetterColliderGeneration", !betterColliderGenerationEnabled);
        }

        [MenuItem("Tools/Better Collider Generation", true)]
        static bool ValidateToggleColliderGeneration()
        {
            var betterColliderGenerationEnabled = EditorPrefs.GetBool("BetterColliderGeneration", false);
            Menu.SetChecked("Tools/Better Collider Generation", betterColliderGenerationEnabled);
            return true;
        }

        private List<Transform> _transforms = new List<Transform>();
        private const string Box = "ubx";
        private const string Capsule = "ucp";
        private const string Sphere = "usp";
        private const string ConvexMesh = "ucx";
        private const string Mesh = "umc";


        private void OnPostprocessModel(GameObject g)
        {
            if (!EditorPrefs.GetBool("BetterColliderGeneration", false))
                return;

            foreach (Transform child in g.transform)
            {
                GenerateCollider(child);
            }

            for (int i = _transforms.Count - 1; i >= 0; --i)
            {
                if (_transforms[i] != null)
                {
                    GameObject.DestroyImmediate(_transforms[i].gameObject);
                }
            }

            var arr = g.GetComponentsInChildren<MeshCollider>();
            for (int i = 0; i < arr.Length; i++)
            {
                var meshName = arr[i].sharedMesh.name.ToLower();
                if (meshName.StartsWith($"{Mesh}_")
                    || meshName.StartsWith($"{ConvexMesh}_")) continue;

                // Destroy mesh collider components without naming conventions rules.
                Debug.Log($"Destroyed mesh collider component by mesh {meshName} without naming prefix.");
                GameObject.DestroyImmediate(arr[i]);
            }
        }


        private void GenerateCollider(Transform t)
        {
            foreach (Transform child in t.transform)
            {
                GenerateCollider(child);
            }

            if (DetectNamingConvention(t, Box))
            {
                AddCollider<BoxCollider>(t);
                _transforms.Add(t);
            }
            else if (DetectNamingConvention(t, Capsule))
            {
                AddCollider<CapsuleCollider>(t);
                _transforms.Add(t);
            }
            else if (DetectNamingConvention(t, Sphere))
            {
                AddCollider<SphereCollider>(t);
                _transforms.Add(t);
            }
            else if (DetectNamingConvention(t, ConvexMesh))
            {
                TransformSharedMesh(t.GetComponent<MeshFilter>());
                var collider = AddCollider<MeshCollider>(t);
                collider.convex = true;
                _transforms.Add(t);
            }
            else if (DetectNamingConvention(t, Mesh))
            {
                TransformSharedMesh(t.GetComponent<MeshFilter>());
                AddCollider<MeshCollider>(t);
                _transforms.Add(t);
            }
        }

        private void TransformSharedMesh(MeshFilter meshFilter)
        {
            if (meshFilter == null)
                return;

            var transform = meshFilter.transform;
            var mesh = meshFilter.sharedMesh;
            var vertices = mesh.vertices;

            for (int i = 0; i < vertices.Length; ++i)
            {
                vertices[i] = transform.TransformPoint(vertices[i]);
                vertices[i] = transform.parent.InverseTransformPoint(vertices[i]);
            }

            mesh.SetVertices(vertices);
        }

        private T AddCollider<T>(Transform t) where T : Collider
        {
            T collider = t.gameObject.AddComponent<T>();
            T parentCollider = t.parent.gameObject.AddComponent<T>();

            EditorUtility.CopySerialized(collider, parentCollider);
            SerializedObject parentColliderSo = new SerializedObject(parentCollider);
            var parentCenterProperty = parentColliderSo.FindProperty("m_Center");
            if (parentCenterProperty != null)
            {
                SerializedObject colliderSo = new SerializedObject(collider);
                var colliderCenter = colliderSo.FindProperty("m_Center");
                var worldSpaceColliderCenter = t.TransformPoint(colliderCenter.vector3Value);

                parentCenterProperty.vector3Value = t.parent.InverseTransformPoint(worldSpaceColliderCenter);
                parentColliderSo.ApplyModifiedPropertiesWithoutUndo();
            }

            return parentCollider;
        }


        private bool DetectNamingConvention(Transform t, string convention)
        {
            bool result = false;
            if (t.gameObject.TryGetComponent(out MeshFilter meshFilter))
            {
                var lowercaseMeshName = meshFilter.sharedMesh.name.ToLower();
                result = lowercaseMeshName.StartsWith($"{convention}_");
            }

            if (result == false)
            {
                var lowercaseName = t.name.ToLower();
                result = lowercaseName.StartsWith($"{convention}_");
            }

            return result;
        }
    }
}