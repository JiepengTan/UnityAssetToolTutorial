// https://github.com/JiepengTan/UnityAssetToolTutorial
using UnityEditor;
using UnityEngine;

namespace GamesTan.Workflow {
    class ModelPostprocessor : AssetPostprocessor {
        private static string modelTexPath = "Assets/UnityAssetTool/Res/Texture";
        private static string modelDir = "Assets/UnityAssetTool/Res/Model";
        private static string uiDir = "Assets/UnityAssetTool/Res/UI";

        void OnPostprocessModel(GameObject go) {
            var isModel = assetPath.Contains(modelDir);
            if (isModel) {
                var importer = this.assetImporter as ModelImporter;
                DealModel(importer);
            }
        }

        void OnPostprocessTexture(Texture2D tex) {
            var isUI = assetPath.Contains(uiDir);
            var isModelTex = assetPath.Contains(modelTexPath);

            var importer = this.assetImporter as TextureImporter;
            importer.isReadable = false;
            if (isUI) {
                DealUI(importer);
            }

            if (isModelTex) {
                DealModelTex(importer);
            }
        }

        private void DealModel(ModelImporter importer) {
            // importer.globalScale = 100; // 适配Houdini
            // importer.isReadable = false;
        }

        private void DealModelTex(TextureImporter importer) {
            importer.mipmapEnabled = true;
            if (assetPath.Contains("_Normal")) {
                importer.textureType = TextureImporterType.NormalMap;
            }
        }

        private static void DealUI(TextureImporter importer) {
            Debug.Log(importer.assetPath);
            importer.mipmapEnabled = false; 
            importer.textureType = TextureImporterType.Sprite; 
        }
    }
}