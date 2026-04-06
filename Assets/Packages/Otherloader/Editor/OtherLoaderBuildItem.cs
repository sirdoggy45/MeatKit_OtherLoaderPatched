using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;

#if H3VR_IMPORTED
using FistVR;
#endif

namespace MeatKit
{
    [CreateAssetMenu(menuName = "MeatKit/Build Items/OtherLoader Item", fileName = "BuildItemNew")]
    public class OtherLoaderBuildItem : BuildItem
    {

#if H3VR_IMPORTED

        [Tooltip("The name of this bundle pair")]
        public string BundleName;

        [Tooltip("Drag your item prefabs here")]
        public List<GameObject> Prefabs;

        [Tooltip("Drag your SpawnerIDs here")]
        public List<OtherLoader.ItemSpawnerEntry> SpawnerEntries;

        [Tooltip("Drag your FVRObjects here")]
        public List<FVRObject> FVRObjects;

        public List<FVRFireArmMechanicalAccuracyChart> AccuracyCharts;

        public List<FVRFireArmRoundDisplayData> RoundData;

        public List<HandlingGrabSet> HandlingGrabSets;

        public List<HandlingReleaseSet> HandlingReleaseSets;

        public List<HandlingReleaseIntoSlotSet> HandlingReleaseSlotSets;

        public List<AudioBulletImpactSet> AudioBulletImpactSets;

        public List<AudioImpactSet> AudioImpactSets;

        public List<TutorialBlock> TutorialBlocks;

        public List<Object> TutorialVideos;

        [Tooltip("When true, contents of item will be broken into two bundles: the data and the assets. This improves load times")]
        public bool OnDemand = true;

        public override IEnumerable<string> RequiredDependencies
        {
            get { return new[] { "Sirdoggy-OtherLoaderPatched-2.0.0" }; }
        }

        public override List<AssetBundleBuild> ConfigureBuild()
        {
            var bundles = new List<AssetBundleBuild>();

            var dataNames = new List<string>();
            dataNames.AddRange(SpawnerEntries.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(FVRObjects.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(AccuracyCharts.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(RoundData.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(HandlingGrabSets.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(HandlingReleaseSets.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(HandlingReleaseSlotSets.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(AudioBulletImpactSets.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(AudioImpactSets.Select(o => AssetDatabase.GetAssetPath(o)));
            dataNames.AddRange(TutorialBlocks.Select(o => AssetDatabase.GetAssetPath(o)));

            var prefabNames = new List<string>();
            prefabNames.AddRange(Prefabs.Select(o => AssetDatabase.GetAssetPath(o)));

            if (OnDemand)
            {
                bundles.Add(new AssetBundleBuild
                {
                    assetBundleName = BundleName.ToLower(),
                    assetNames = dataNames.ToArray()
                });


                bundles.Add(new AssetBundleBuild
                {
                    assetBundleName = "late_" + BundleName.ToLower(),
                    assetNames = prefabNames.ToArray()
                });
            }
            else
            {
                bundles.Add(new AssetBundleBuild
                {
                    assetBundleName = BundleName.ToLower(),
                    assetNames = dataNames.Concat(prefabNames).ToArray()
                });
            }

            ExportVideos();

            return bundles;
        }

        private void ExportVideos()
        {
            foreach (var videoFile in TutorialVideos)
            {
                var assetPath = AssetDatabase.GetAssetPath(videoFile);
                File.Copy(assetPath, BuildWindow.SelectedProfile.ExportPath + Path.GetFileName(assetPath));
            }
        }

        public override void GenerateLoadAssets(TypeDefinition plugin, ILProcessor il)
        {
            EnsurePluginDependsOn(plugin, "h3vr.otherloader", "2.0.0");
        }

#endif
    }

}



