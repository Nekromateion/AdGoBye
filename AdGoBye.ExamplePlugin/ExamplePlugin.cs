﻿using AdGoBye.Plugins;

namespace AdGoBye.ExamplePlugin;

using AssetsTools.NET;
using AssetsTools.NET.Extra;

public class ExamplePlugin : BasePlugin
{
    public override EPluginType PluginType()
    {
        return EPluginType.Global;
    }

    public override EPatchResult Patch(string contentId, string dataDirectoryPath)
    {
        var manager = new AssetsManager();
        var dataLocation = dataDirectoryPath + "/__data";
        try
        {
            var bundleInstance = manager.LoadBundleFile(dataLocation);
            var bundle = bundleInstance.file;

            var assetFileInstance = manager.LoadAssetsFileFromBundle(bundleInstance, 1, false);
            var assetsFile = assetFileInstance.file;

            var foundOneChair = false;
            foreach (var monoBehaviour in assetsFile.GetAssetsOfType(AssetClassID.MonoBehaviour))
            {
                var monoBehaviourInfo = manager.GetBaseField(assetFileInstance, monoBehaviour);
                if (monoBehaviourInfo["PlayerMobility"].IsDummy) continue;
                
                var parentGameObject = assetsFile.GetAssetInfo(monoBehaviourInfo["m_GameObject.m_PathID"].AsLong);
                var parentGameObjectInfo = manager.GetBaseField(assetFileInstance, parentGameObject);
                
                parentGameObjectInfo["m_IsActive"].AsBool = false;
                parentGameObject.SetNewData(parentGameObjectInfo);

                if (foundOneChair) continue;
                foundOneChair = true;
            }

            if (!foundOneChair)
            {
                Console.WriteLine("Skipping, no chairs found");
                return EPatchResult.Skipped;
            }
            
            bundle.BlockAndDirInfo.DirectoryInfos[1].SetNewData(assetsFile);
            using var writer = new AssetsFileWriter(dataLocation + ".mod");
            bundle.Write(writer);
            
            writer.Close();
            assetsFile.Close();
            bundle.Close();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return EPatchResult.Fail;
        }

        File.Replace(dataLocation + ".mod",dataLocation, null);
        return EPatchResult.Success;
    }
}