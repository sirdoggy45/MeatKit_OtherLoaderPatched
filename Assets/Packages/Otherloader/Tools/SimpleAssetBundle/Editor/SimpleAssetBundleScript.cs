using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Packages.Otherloader.Tools.SimpleAssetBundle.Editor
{
    public static class SimpleAssetBundleScript
    {
        [MenuItem("Tools/Create simple AssetBundle")]
        private static void Build()
        {
            var input = Path.GetFullPath(Application.dataPath);
            var output = Path.GetFullPath(Application.dataPath) + "/SimpleAssetBundleOutput";

            // Exclude meta files
            var assetPaths = Directory
                .GetFiles(input, "*", SearchOption.TopDirectoryOnly)
                .Where(path => !path.EndsWith(".meta"))
                .Select(ToUnityPath)
                .ToArray();

            if (assetPaths.Length == 0)
            {
                Debug.LogWarning("No assets found in root directory.");
                return;
            }

            var bundleName = "assetbundle_" + System.DateTime.Now.Ticks;

            var build = new AssetBundleBuild
            {
                assetBundleName = bundleName,
                assetNames = assetPaths
            };

            if (!Directory.Exists(output))
                Directory.CreateDirectory(output);

            BuildPipeline.BuildAssetBundles(
                output,
                new[] { build },
                BuildAssetBundleOptions.ChunkBasedCompression,
                BuildTarget.StandaloneWindows64
            );

            AssetDatabase.Refresh();

            Debug.LogFormat(
                "Successfully built AssetBundle '{0}' with {1} assets!",
                bundleName,
                assetPaths.Length
            );
        }

        private static string ToUnityPath(string fullPath)
        {
            fullPath = fullPath.Replace("\\", "/");
            var dataPath = Application.dataPath.Replace("\\", "/");
            return "Assets" + fullPath.Substring(dataPath.Length);
        }
    }
}