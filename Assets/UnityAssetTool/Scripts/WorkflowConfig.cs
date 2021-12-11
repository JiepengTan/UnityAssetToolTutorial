// https://github.com/JiepengTan/UnityAssetToolTutorial

using System.Collections;
using System.Collections.Generic;
using System.IO;
using GamesTan.Util;
using NaughtyAttributes;
using UnityEditor;
using UnityEngine;

namespace GamesTan.Workflow {
    [CreateAssetMenu(menuName = "Configs/WorkflowConfig")]
    public class WorkflowConfig : ScriptableObject {
        public Vector3 Offset;
        public string modelDir;
        public string texDir;

        public string dstModelDir;
        public string dstTexDir;
        public string dstPrefabDir;
        public string dstMatDir;

        public Material defaultMat;
        public int RowCount = 4;


        void Clear() {
            AssetDatabase.DeleteAsset(dstModelDir);
            AssetDatabase.DeleteAsset(dstTexDir);
        }


        [Button]
        void CopyAssets() {
            PathUtil.EnsureDirectoryExist(dstModelDir);
            PathUtil.EnsureDirectoryExist(dstTexDir);
            PathUtil.EnsureDirectoryExist(dstPrefabDir);
            PathUtil.EnsureDirectoryExist(dstMatDir);

            PathUtil.Walk(texDir, "*.png", (p) => {
                var fileName = Path.GetFileName(p);
                File.Copy(p, Path.Combine(dstTexDir, fileName), true);
            });
            PathUtil.Walk(modelDir, "*.fbx", (p) => {
                if (p.EndsWith("_low.fbx")) {
                    // 只拷贝低模
                    var fileName = Path.GetFileName(p);
                    File.Copy(p, Path.Combine(dstModelDir, fileName), true);
                }
            });
            AssetDatabase.ImportAsset(dstTexDir);
            AssetDatabase.ImportAsset(dstModelDir);
            AssetDatabase.Refresh();
        }

        [Button]
        void CreateAndShowPrefabs() {
            CreatePrefabs();
            ShowPrefabs();
        }

        void CreatePrefabs() {
            PathUtil.EnsureDirectoryExist(dstPrefabDir);
            PathUtil.EnsureDirectoryExist(dstMatDir);

            var allFbxs = new List<string>();
            PathUtil.Walk(dstModelDir, "*.fbx", (p) => { allFbxs.Add(p); });
            foreach (var fbx in allFbxs) {
                var mat = new Material(defaultMat);
                var fileName = Path.GetFileNameWithoutExtension(fbx);
                SetTexture(mat, fileName, "_MainTex", "_AlbedoTransparency");
                SetTexture(mat, fileName, "_BumpMap", "_Normal");
                SetTexture(mat, fileName, "_MetallicGlossMap", "_MetallicSmoothness");
                mat.EnableKeyword("_METALLICGLOSSMAP");
                AssetDatabase.CreateAsset(mat, dstMatDir + "/" + fileName + ".mat");
                Debug.Log(fbx);
                var go = new GameObject(fileName);
                var fab = AssetDatabase.LoadAssetAtPath<GameObject>(dstModelDir + "/" + fileName + ".fbx");
                var model = PrefabUtility.InstantiatePrefab(fab) as GameObject;
                model.transform.SetParent(go.transform, false);
                model.GetComponent<Renderer>().sharedMaterial = mat;
                model.transform.localScale = Vector3.one * 100;
                PrefabUtility.SaveAsPrefabAsset(go, dstPrefabDir + "/" + fileName + ".prefab");
                GameObject.DestroyImmediate(go);
            } 
            void SetTexture(Material mat, string fileName, string keyName, string picName) {
                string posfixTag = ".png";
                mat.SetTexture(keyName, AssetDatabase.LoadAssetAtPath<Texture>(dstTexDir + "/" + fileName + picName +posfixTag));
            }
        }

        void ShowPrefabs() {
            var parentName = "___ShowPrefabRoot";
            var parent = GameObject.Find(parentName);
            if (parent == null) {
                parent = new GameObject(parentName);
            }

            while (parent.transform.childCount > 0) {
                GameObject.DestroyImmediate(parent.transform.GetChild(0));
            }

            List<string> paths = new List<string>();
            PathUtil.Walk(dstPrefabDir, "*.prefab", (p) => {
                var path = PathUtil.GetAssetPath(p);
                paths.Add(path);
            });
            int idx = 0;
            foreach (var path in paths) {
                var fab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var go = GameObject.Instantiate(fab, parent.transform);
                go.transform.position = new Vector3(idx % RowCount, 0, idx / RowCount) * 1;
                idx++;
            }
        }
    }
}