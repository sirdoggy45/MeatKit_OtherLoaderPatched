using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEditor;
using UnityEngine;

namespace MeatKit
{

    [System.Serializable]
    public class BepinexDepPair
    {
        public string guid;
        public string version;
    }

    [CreateAssetMenu(menuName = "MeatKit/Build Items/OtherLoader Root", fileName = "BuildRootNew")]
    public class OtherLoaderBuildRoot : BuildItem
    {
#if H3VR_IMPORTED

        [Tooltip("Build items that should load first, in the order they appear")]
        public List<OtherLoaderBuildItem> BuildItemsFirst = new List<OtherLoaderBuildItem>();

        [Tooltip("Build items that should in parralel, after the first items load")]
        public List<OtherLoaderBuildItem> BuildItemsAny = new List<OtherLoaderBuildItem>();

        [Tooltip("Build items that should load last, in the order they appear")]
        public List<OtherLoaderBuildItem> BuildItemsLast = new List<OtherLoaderBuildItem>();

        [Tooltip("Guids of otherloader mods that must be loaded before these assets will load. Only applies to SelfLoading mods")]
        public List<string> LoadDependancies = new List<string>();

        [Tooltip("Guids of bepinex mods that this mod will depend on")]
        public List<BepinexDepPair> BepinexDependancies = new List<BepinexDepPair>();

        [Tooltip("When true, additional code will be generated that allows the mod to automatically load itself into otherloader")]
        public bool SelfLoading = true;

        public override IEnumerable<string> RequiredDependencies
        {
            get { return new[] { "Sirdoggy-OtherLoaderPatched-2.0.0" }; }
        }

        public override Dictionary<string, BuildMessage> Validate()
        {
            var messages = base.Validate();

            var allBuildItems = BuildItemsFirst.Concat(BuildItemsAny).Concat(BuildItemsLast).ToList();

            ValidateBuildItems(messages, BuildItemsFirst, allBuildItems, "BuildItemsFirst");
            ValidateBuildItems(messages, BuildItemsAny, allBuildItems, "BuildItemsAny");
            ValidateBuildItems(messages, BuildItemsLast, allBuildItems, "BuildItemsLast");

            return messages;
        }

        private void ValidateBuildItems(
            Dictionary<string, BuildMessage> messages,
            List<OtherLoaderBuildItem> targetBuildItems,
            List<OtherLoaderBuildItem> allBuildItems,
            string messageField)
        {
            foreach (var buildItem in targetBuildItems)
            {
                if (buildItem == null)
                {
                    messages[messageField] = BuildMessage.Error("Child build item cannot be null!");
                    continue;
                }

                if (allBuildItems.Count(o => o != null && buildItem != null && o.BundleName == buildItem.BundleName) > 1)
                {
                    messages[messageField] = BuildMessage.Error("Child build items must have unique bundle names!");
                }

                var itemMessages = buildItem.Validate();
                itemMessages.ToList().ForEach(o => { messages[messageField] = o.Value; });
            }
        }

        public override List<AssetBundleBuild> ConfigureBuild()
        {
            var bundles = new List<AssetBundleBuild>();

            BuildItemsFirst.ForEach(o => { bundles.AddRange(o.ConfigureBuild()); });

            BuildItemsAny.ForEach(o => { bundles.AddRange(o.ConfigureBuild()); });

            BuildItemsLast.ForEach(o => { bundles.AddRange(o.ConfigureBuild()); });

            return bundles;
        }

        public override void GenerateLoadAssets(TypeDefinition plugin, ILProcessor il)
        {
            EnsurePluginDependsOn(plugin, "h3vr.otherloader", "2.0.0");

            foreach (var dependancy in BepinexDependancies)
            {
                EnsurePluginDependsOn(plugin, dependancy.guid, dependancy.version);
            }

            if (SelfLoading)
            {
                var loadFirst = BuildItemsFirst.Select(o => o.BundleName.ToLower()).ToArray();
                var loadAny = BuildItemsAny.Select(o => o.BundleName.ToLower()).ToArray();
                var loadLast = BuildItemsLast.Select(o => o.BundleName.ToLower()).ToArray();
                
                // Get reference to the RegisterDirectLoad method and path to it
                var basePath = plugin.Fields.First(f => f.Name == "BasePath");
                const BindingFlags publicStatic = BindingFlags.Public | BindingFlags.Static;
                var registerLoadMethod = typeof(OtherLoader.OtherLoader).GetMethod("RegisterDirectLoad", publicStatic);

                // Pass the path, guid, dependancies, and 3 arrays of bundle names as params
                il.Emit(OpCodes.Ldsfld, basePath);
                il.Emit(OpCodes.Ldstr, BuildWindow.SelectedProfile.Author + "." + 
                                       BuildWindow.SelectedProfile.PackageName);
                il.Emit(OpCodes.Ldstr, string.Join(",", LoadDependancies.ToArray()));
                il.Emit(OpCodes.Ldstr, string.Join(",", loadFirst));
                il.Emit(OpCodes.Ldstr, string.Join(",", loadAny));
                il.Emit(OpCodes.Ldstr, string.Join(",", loadLast));
                il.Emit(OpCodes.Call, plugin.Module.ImportReference(registerLoadMethod));
            }
        }
#endif
    }
}

