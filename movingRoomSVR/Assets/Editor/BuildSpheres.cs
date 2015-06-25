using UnityEngine;
using UnityEditor;
//using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class StaticBatchBuilder {
    public string materialName;
    public List<GameObject> staticObjects;
}

public class BuildSpheres : ScriptableWizard
{
///  public string baseBuildLetter;
///
///do not change the order of the public parameters 
    public string defaultPath = "Assets/Scenes/";
    public string baseLevelToBuild = "theblu/OceanView";
    public bool buildAllScenesInFolder = false;
    public bool forceBuild = false;
    public string gitVersion;
//Borut
//public string pathToCloudRepo = "E:\\theblu\\";
    public string pathToCloudRepo = "C:\\Projects\\Wemo\\theBluBuild\\android\\data";
    string buildFromFolder = "Assets/Scenes/builddata/";
    string uvFixupFolder = "Assets/UV_Fixup/";


    List<StaticBatchBuilder> staticBatchBuilders;

// string websiteBundlePath = "web/sites/theblu.com/htdocs/public/v/2/unity/spheres";
	//BOrut
    string websiteBundlePath = "unity/spheres";

    string prefabFolder = "Assets/Standard Assets/Prefabs/Streaming/";
    string streamingPrefabName = "LevelStreamingData.prefab";
    //string streamingSceneFolder = "Assets/Scenes/Streaming/";
    string buildAllInSelectedFolder //= "theblu/";
    {
        get
        {
            return baseLevelToBuild.Substring(0,baseLevelToBuild.LastIndexOf("/")+1);
        }
    }
    string baseScenePath
    {
        get
        {
            return buildFromFolder + baseLevelToBuild + ".unity";
        }
    }
 
    string oldBluLoadingData;

    [MenuItem("WEMOTools/Build/Build Spheres")]
    static void CreateWizard()
    {

        BuildSpheres me = ScriptableWizard.DisplayWizard("BUILD THE SPHERES", typeof(BuildSpheres), "Build") as BuildSpheres;
        string LastBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/LastBuildSetting.json");
        if( File.Exists(LastBuildSettingFile) )
        {
            string[] lastBuildSettings = File.ReadAllLines(LastBuildSettingFile);
            me.defaultPath = lastBuildSettings[0];
            me.baseLevelToBuild = lastBuildSettings[1];
            me.buildAllScenesInFolder = bool.Parse(lastBuildSettings[2]);
            if( lastBuildSettings.Length > 3 ) {
                me.pathToCloudRepo = lastBuildSettings[3];
            }
            if( lastBuildSettings.Length > 4 ) {
                me.forceBuild = bool.Parse(lastBuildSettings[4]);
            }
        }
        me.gitVersion = GitUtilities.VersionString();     
    }
    
    void OnWizardUpdate()
    {
        helpString =  "Default Path: the base scenes directory, normally it doesn't need to be changed.\n\n" +
        "Base Level To Build: a combination of sub directory of \n" +
        "directory of the scene you want to build and the scene name without file type.\n" +
        "Example: build a scene that is under Scenes/theblu directory,\n" +
        "the scene name is open-water,\n" +
        "then the Base Level To Build is theblu/open-water\n\n" +
        "When buildAllScenesInFolder is checked, you need to change the Base Level To Build to a subdirectory\n" +
        "Example: if you only want to build all scene in theblu folder, then make the Base Level To Build to theblu/OceanView\n" +
        "where OceanView is going to be the 'master scene', which is required.\n\n" +
        "When no box checked, you only build one normal scene.";
    }

    string GetFullPath(string relative)
    {
        string tmp = Application.dataPath;
        return tmp.Substring(0, tmp.LastIndexOf("/") + 1) + relative;
    }

    string GetFullPrefabPath(string sceneName)
    {
        if(!sceneName.EndsWith("/"))
        {
            sceneName += "/";
        }
        return prefabFolder + sceneName + streamingPrefabName;
    }

    string GetSceneName(string path)
    {
        return path.Substring(path.LastIndexOf("/") + 1, path.LastIndexOf(".unity") - path.LastIndexOf("/") - 1);
    }

    void OnWizardCreate()
    {
		Caching.CleanCache(); // nuke web cache so it doesn't have to be done manually

        string sceneWeAreStartingFrom = EditorApplication.currentScene;

        /*    if( quickBuild )
        {
        string path = "Assets/Standard Assets/Prefabs/Streaming/" + baseLevelToBuild + "/" + streamingPrefabName;
        string bundleDirectoryPath = "AssetBundles/Spheres/";
        GameObject level = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;

        BuildLevelAssetBundle(level, bundleDirectoryPath + baseLevelToBuild + ".unity3d", true);

        return;
        }*/
        if (!buildFromFolder.EndsWith("/"))
        {
            buildFromFolder += "/";
        }

        if(!Directory.Exists(buildFromFolder))
        {
            Directory.CreateDirectory(buildFromFolder);
        }

        string LastEditedTimeDataFile = string.Concat(Application.dataPath, "/../WEMOData/LastEditedTimeData.json");
        string WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
        if(!Directory.Exists(WEMODataDirectory))
        {
            Directory.CreateDirectory(WEMODataDirectory);
        }
        if(!File.Exists(LastEditedTimeDataFile))
        {
            File.WriteAllText(LastEditedTimeDataFile, "");
        }

        string[] sceneCreatedTimeInformation = File.ReadAllLines(LastEditedTimeDataFile);
        int length = sceneCreatedTimeInformation.Length;
        string[] sceneCreatedTimeStrings = new string[length];
        string[] sceneCreatedFromPaths = new string[length];
        System.DateTime[] sceneCreatedTimeFromFile = new System.DateTime[length];

        for(int i = 0; i < length; ++i)
        {
            sceneCreatedTimeStrings[i] = sceneCreatedTimeInformation[i].Substring(0, sceneCreatedTimeInformation[i].LastIndexOf(":"));
            sceneCreatedFromPaths[i] = sceneCreatedTimeInformation[i].Substring(sceneCreatedTimeInformation[i].LastIndexOf(":")+1);
            sceneCreatedFromPaths[i] = sceneCreatedFromPaths[i].Replace("\\", "/");
            sceneCreatedFromPaths[i] = sceneCreatedFromPaths[i] .Replace(buildFromFolder, defaultPath);
            sceneCreatedTimeFromFile[i] = System.DateTime.Parse(sceneCreatedTimeStrings[i]);
        }       

        if (!buildAllScenesInFolder)
        {
            CopySingleSceneFileAndLightMap();
            AssetDatabase.SaveAssets();

            string savedStreamingBundle = ProcessScene(buildFromFolder, baseLevelToBuild);
            if (savedStreamingBundle != string.Empty)
            {
                List<string> sceneBundles = new List<string>();
                sceneBundles.Add(savedStreamingBundle);

                MoveBundlesToBuildFolder(sceneBundles);
                AssetDatabase.SaveAssets();
            }
        }
        else
        {
            List<string> sceneBundles = new List<string>();

            string[] copyFromFilePaths = Directory.GetFiles(defaultPath + buildAllInSelectedFolder , "*.*", SearchOption.AllDirectories);

            foreach(string path in copyFromFilePaths)
            {
                if(path.EndsWith(".meta") || path.Contains("the-blu-boot"))
                {
                    continue;
                }
                bool isCurrentPathModified = true;
                if( !forceBuild ) {
                    for(int i = 0; i < length; ++i)
                    {
                        if(path.Contains(sceneCreatedFromPaths[i]))
                        {
                            if(File.GetLastAccessTime(path).ToString() == sceneCreatedTimeFromFile[i].ToString())
                            {
                            //not modified
                                isCurrentPathModified = false;
                                break;
                            }
                        }
                    }
                }
                if(!isCurrentPathModified)
                {
                    continue;
                }


                string srcPath = path.Replace("\\", "/");
                string destPath = srcPath.Replace(defaultPath, buildFromFolder);
                string destDirectory = Path.GetDirectoryName(destPath);
                if(!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }
                AssetDatabase.Refresh();
                File.Copy(srcPath, destPath, true);
                AssetDatabase.Refresh();
            }

            AssetDatabase.Refresh();
            AssetDatabase.SaveAssets();             

            string tmp; // Preparing for streaming scenes
            string[] sceneFiles = Directory.GetFiles(buildFromFolder+buildAllInSelectedFolder, "*.unity");
            foreach (string scenePath in sceneFiles)
            {
                bool isCurrentPathModified = true;
                if( !forceBuild ) {
                    string path = scenePath.Replace(buildFromFolder, defaultPath);
                    for(int i = 0; i < length; ++i)
                    {
                        if(path.Contains(sceneCreatedFromPaths[i]))
                        {
                            if(File.GetLastAccessTime(path).ToString() == sceneCreatedTimeFromFile[i].ToString())
                            {
                                //not modified
                                isCurrentPathModified = false;
                                break;
                            }
                        }
                    }
                }
                if(!isCurrentPathModified)
                {
                    continue;
                }
                string sceneNameToPassIn = scenePath.Replace(buildFromFolder, "");
                sceneNameToPassIn = sceneNameToPassIn.Replace(".unity", "");
                tmp = ProcessScene(buildFromFolder, sceneNameToPassIn);

                if (!string.IsNullOrEmpty(tmp))
                {
                    Debug.Log("adding " + tmp );
                    sceneBundles.Add(tmp);
                }
            }

            MoveBundlesToBuildFolder(sceneBundles);

            AssetDatabase.SaveAssets();
        }

        //go back to the we started from.
        HasSceneOpened(sceneWeAreStartingFrom);
        AssetDatabase.Refresh();

        if (File.Exists(LastEditedTimeDataFile))
        {
            File.Delete(LastEditedTimeDataFile);
        }
        string[] scenePaths = Directory.GetFiles(buildFromFolder, "*.unity", SearchOption.AllDirectories);
        string[] LastEditedTimeDataFileContent = new string[scenePaths.Length];
        for(int i = 0; i < scenePaths.Length; ++i)
        {
            System.DateTime sceneCreatedTime = File.GetLastAccessTime(scenePaths[i].Replace(buildFromFolder,defaultPath));
            string scenesCreatedPath = scenePaths[i].Replace(buildFromFolder, defaultPath);
            scenesCreatedPath = scenesCreatedPath.Replace("\\", "/");
            LastEditedTimeDataFileContent[i] =  sceneCreatedTime.ToString() + ":" + scenePaths[i];
        }
        File.WriteAllLines(LastEditedTimeDataFile, LastEditedTimeDataFileContent);

        string LastBuildSettingFile = string.Concat(Application.dataPath, "/../WEMOData/LastBuildSetting.json");
        WEMODataDirectory = string.Concat(Application.dataPath, "/../WEMOData/");
        if(!Directory.Exists(WEMODataDirectory))
        {
            Directory.CreateDirectory(WEMODataDirectory);
        }
        if(File.Exists(LastBuildSettingFile))
        {
            File.Delete(LastBuildSettingFile);
        }

        string[] lastBuildSettings = new string[5];
        lastBuildSettings[0] = defaultPath;
        lastBuildSettings[1] = baseLevelToBuild;
        lastBuildSettings[2] = buildAllScenesInFolder.ToString();
        lastBuildSettings[3] = pathToCloudRepo;
        lastBuildSettings[4] = forceBuild.ToString();

        File.WriteAllLines(LastBuildSettingFile, lastBuildSettings);
        Directory.Delete(buildFromFolder,true);
        AssetDatabase.Refresh();
    }
 
    void CopySingleSceneFileAndLightMap()
    {
        string[] copyFromScenefilePaths = Directory.GetFiles(defaultPath,"*.unity", SearchOption.AllDirectories);


        foreach(string path in copyFromScenefilePaths)
        {
            if(path.EndsWith(".meta"))
            {
                continue;
            }
            string sceneFilePath = path.Replace("\\", "/");
            string sceneFileDirectory = sceneFilePath.Substring(0, sceneFilePath.LastIndexOf("/")+1);
            sceneFileDirectory = sceneFileDirectory.Replace(defaultPath, buildFromFolder);

            if(sceneFilePath.Contains(baseLevelToBuild + ".unity"))
            {
                if(!Directory.Exists( sceneFileDirectory))
                {
                    Directory.CreateDirectory(sceneFileDirectory);
                }
                if(File.Exists(baseScenePath))
                {
                    File.Delete(baseScenePath);
                }
                AssetDatabase.Refresh();
                AssetDatabase.CopyAsset(sceneFilePath, baseScenePath);
                AssetDatabase.Refresh();
            }
        }

        string[] copyFromLightMapFilePaths;
        if(Directory.Exists(defaultPath + baseLevelToBuild + "/"))
        {
            copyFromLightMapFilePaths = Directory.GetFiles(defaultPath + baseLevelToBuild + "/");
            foreach(string path in copyFromLightMapFilePaths)
            {
                if(path.EndsWith(".meta"))
                {
                    continue;
                }
                string lightMapPath = path.Replace("\\", "/");
                string lightMapFileDirectory = lightMapPath.Substring(0, lightMapPath.LastIndexOf("/")+1);
                lightMapFileDirectory = lightMapFileDirectory.Replace(defaultPath, buildFromFolder);
                if(!lightMapFileDirectory.EndsWith("/"))
                {
                    lightMapFileDirectory += "/";
                }
                if(!Directory.Exists( lightMapFileDirectory))
                {
                    Directory.CreateDirectory(lightMapFileDirectory);
                    AssetDatabase.Refresh();
                }
                string lightMapCopyToPath = lightMapPath.Replace(defaultPath, buildFromFolder);
                File.Copy(lightMapPath, lightMapCopyToPath,true);
                AssetDatabase.Refresh();
            }
        }
    }
 
    bool HasSceneOpened(string scenePath)
    {
        // force open the chosen view to build
        if (!EditorApplication.currentScene.Equals(scenePath))
        {
            if (File.Exists(scenePath))
            {
                EditorApplication.OpenScene(scenePath);             
            }
            else
            {
                Debug.LogWarning("Cannot find " + scenePath);
                return false;
            }
        }

        return true;
    }

    string BuildLevelAssetBundle(Object dataObject, string sceneName, bool use_scene_name)
    {
        BuildAssetBundleOptions options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
        string bundlePath = GetBundleTempPath(sceneName);
        if( use_scene_name ) {
            BuildPipeline.BuildAssetBundle(dataObject, null, sceneName, options, EditorUserBuildSettings.activeBuildTarget);
        }
        else {
            BuildPipeline.BuildAssetBundle(dataObject, null, bundlePath, options, EditorUserBuildSettings.activeBuildTarget);
        }

        return bundlePath;
    }

    string GetBundleTempPath(string sceneName)
    {
        return Path.Combine(prefabFolder, sceneName) + ".unity3d";
    }

    string GetWebsiteBundlePath(string sceneName) 
    {
        return Path.Combine(pathToCloudRepo, Path.Combine(websiteBundlePath, sceneName)) + ".unity3d";
    }

    void MoveBundlesToBuildFolder(List<string> bundles)
    {
        foreach (string bundle in bundles)
        {
            string sceneName = GetSceneName(bundle);
            //               Debug.LogError("bundle: " + bundle);
            //               Debug.LogError("sceneName: " + sceneName);
            string full_path = GetWebsiteBundlePath(sceneName);
            if( File.Exists(full_path) )
            {
                File.Delete(full_path);
            }
            //               Debug.LogError("move from: " + buildAllInSelectedFolder+ sceneName + ".unity3d");
            //               Debug.LogError("full_path: " +full_path);

            string destDirName = Path.GetDirectoryName(full_path);
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            File.Copy(prefabFolder + buildAllInSelectedFolder + sceneName + ".unity3d", full_path, true);
        }
    }

    void ProcessLightmaps( GameObject streamingData ) {
        Debug.Log("save lightmap");
        Transform childLightMapData = streamingData.transform.Find(SphereInstance.levelLightMapDataObjName);
        GameObject goLightMapData;
        if (childLightMapData == null) {
            Debug.Log("Generate obj dynamically");
            goLightMapData = new GameObject(SphereInstance.levelLightMapDataObjName);
            goLightMapData.transform.parent = streamingData.transform;
        }
        else {
            goLightMapData = childLightMapData.gameObject;
        }
        
        LightmapDataHolder dataHolder = (LightmapDataHolder) goLightMapData.GetComponent(typeof(LightmapDataHolder));
        if (dataHolder == null) {
            Debug.Log("Generate component dynamically");
            dataHolder = (LightmapDataHolder) goLightMapData.AddComponent(typeof(LightmapDataHolder));
        }
        
        /*           Renderer[] renderers = GameObject.FindObjectsOfType(typeof(Renderer)) as Renderer[];
        foreach(Renderer render in renderers ) {
        Debug.Log("lightmap idx " + render.gameObject.name + " :: " + render.lightmapIndex); 
        }*/
        
        int cnt = LightmapSettings.lightmaps.Length;
        Debug.Log("saving lightmaps:" + cnt);
        dataHolder.lightmapData = new WemoLightmapData[cnt];
        for( int i = 0; i < cnt; i ++ ) {
            dataHolder.lightmapData[i] = new WemoLightmapData();
            dataHolder.lightmapData[i].far = LightmapSettings.lightmaps[i].lightmapFar;
            dataHolder.lightmapData[i].near =  LightmapSettings.lightmaps[i].lightmapNear;
        }
        dataHolder.lightmapsMode = LightmapSettings.lightmapsMode;
    }

    void StaticBatchBuilderStep( string materialName, GameObject go ) {
        Debug.Log ("MATERIAL " + materialName );
        bool found = false;
        for( int i = 0; i < staticBatchBuilders.Count; i++ ) {
            StaticBatchBuilder sb = staticBatchBuilders[ i ];
            if( sb == null ) {
                // uh.
                Debug.LogError("WTF> STATIC BATCH BUILDER IS NULL?!?");
            }
            if( sb.materialName.Equals( materialName ) ) {
                sb.staticObjects.Add( go );
                found = true;
                i = staticBatchBuilders.Count;
            }
        }

        if( !found ) {
            StaticBatchBuilder sb = new StaticBatchBuilder();
            sb.materialName = materialName;
            sb.staticObjects = new List<GameObject>();
            sb.staticObjects.Add( go );
            staticBatchBuilders.Add( sb );
        }
    }

    void ProcessStaticBatchBuilder( GameObject streamingData ) {
        if( staticBatchBuilders.Count <= 0 ) {
            return;
        }

        GameObject staticBatcher = new GameObject( "StaticBatcher" );
        staticBatcher.transform.parent = streamingData.transform;
        AssetBundleStaticBatcher asb = staticBatcher.AddComponent<AssetBundleStaticBatcher>();
        asb.staticBatches = new StaticBatch[staticBatchBuilders.Count];
        for( int i = 0; i < staticBatchBuilders.Count; i++ ) {
            StaticBatchBuilder sbb = staticBatchBuilders[i];
            StaticBatch sb = new StaticBatch();
            sb.materialName = sbb.materialName;
            sb.batchObjects = sbb.staticObjects.ToArray();
            asb.staticBatches[i] = sb;
        }
    }

    void SetupCritterInfoData( GameObject critter ) {
        CritterInfoData new_info = critter.AddComponent<CritterInfoData>();
        new_info.critterTransform = critter.transform;
        new_info.critterCollider = critter.GetComponent<Collider>();
        new_info.critterAnimation = critter.GetComponent<Animation>();
        new_info.critterLODData = critter.GetComponent<LODModelData>();
        new_info.audioData = critter.GetComponent<FishAudioData>();
        
        // upgrade old FishAnimationData components to new CritterAnimationBase
        new_info.animBase = critter.GetComponent<CritterAnimationBase>();
        if( new_info.animBase == null ) {
            OGFishAnimation.CreateFromDeprecatedData( critter, new_info );
        }
        new_info.swimSchoolFollowData = critter.GetComponent<SwimSchoolFollowData>();
        new_info.swimTargetedData = critter.GetComponent<SwimTargetedData>();
        new_info.swimToPointData = critter.GetComponent<SwimToPointData>();
        new_info.swimIdleData = critter.GetComponent<SwimIdleData>();
        new_info.swimIdleBiteData = critter.GetComponent<SwimIdleBiteData>();
        new_info.swimDisperseData = critter.GetComponent<SwimDisperseData>();
        new_info.swimChaseData = critter.GetComponent<SwimChaseData>();
        new_info.swimFreefallData = critter.GetComponent<SwimFreefallData>();
        new_info.swimStrafingData = critter.GetComponent<SwimStrafingData>();
        new_info.swimParkingData = critter.GetComponent<SwimParkingData>();
        new_info.swimFreeData = critter.GetComponent<SwimFreeData>();
        new_info.swimPlayerInteractData = critter.GetComponent<SwimPlayerInteractData>();
        new_info.swimPlayerViewData = critter.GetComponent<SwimPlayerViewData>();
        new_info.swimStrafePlayerData = critter.GetComponent<SwimStrafePlayerData>();
        new_info.swimFollowPathData = critter.AddComponent<SwimFollowPathData>(); // this component added in code for all fish
        new_info.swimScriptGoToPointData = critter.AddComponent<SwimScriptGoToPointData>(); // this component added in code for all fish
        new_info.holdData = critter.AddComponent<HoldData>();
        new_info.deadData = critter.GetComponent<DeadData>();
        new_info.interactionData = critter.GetComponent<InteractionData>();
        new_info.viewportMotionData = critter.GetComponent<ViewportMotionData>();
        new_info.circleAroundObjectData = critter.GetComponent<CircleAroundObjectData>();
        new_info.critterBendData = critter.GetComponent<FishBendControllerData>();
        new_info.critterEyeData = critter.GetComponent<EyeControllerData>();
        new_info.generalSpeciesData = critter.GetComponent<GeneralSpeciesData>();
        new_info.generalMotionData = critter.GetComponent<GeneralMotionData>();
        
        new_info.critterSteering = critter.GetComponent<SteeringBase>();
        if( new_info.critterSteering == null ) {
            Log.Main.Error("Critter prefab (" + critter.name + ") does not have a steering component. Creating default and copying deprecated data from General Motion.");
            OGFishSteering.CreateFromGeneralMotion( critter, new_info );
        }
        
        new_info.critterBoxCollider = critter.GetComponentInChildren<BoxCollider>();
        new_info.critterItemData = critter.GetComponent<WemoItemData>();
    }

    void ProcessAtlasUVCorrection( string sceneName, GameObject streamingData ) {
        if( staticBatchBuilders == null ) {
            staticBatchBuilders = new List<StaticBatchBuilder>();
        }
        else {
            staticBatchBuilders.Clear();
        }

        // create our meshes directory if it doesnt exist.
        string mesh_dir = uvFixupFolder + sceneName + "_UVCorrections";
        if( Directory.Exists( mesh_dir ) ) {
            Directory.Delete( mesh_dir, true );
        }
        Directory.CreateDirectory( mesh_dir );        

        mesh_dir += "/";

        // gather all in-scene atlas materials.
        AtlasMaterial[] atlas_materials = streamingData.GetComponentsInChildren<AtlasMaterial>(true);
        if( atlas_materials.Length > 0 ) {
            for( int i = 0; i < atlas_materials.Length; i++ ) {
                AtlasMaterial am = atlas_materials[i];
                if( am != null && am.atlasName != null && am.atlasName.Length > 0 ) {
                    float xMin,xMax,yMin,yMax,xDiff,yDiff;
                    am.GetData(out xMin,out xMax,out yMin,out yMax,out xDiff,out yDiff);
                    MeshFilter mf = am.GetComponent<MeshFilter>();
                    if( mf != null && mf.sharedMesh != null ) {
                        if( mf.gameObject.isStatic ) {
                            StaticBatchBuilderStep( mf.gameObject.GetComponent<Renderer>().sharedMaterial.name, mf.gameObject );
                        }
                        string mesh_path = AssetDatabase.GetAssetPath( mf.sharedMesh );
                        int index = mesh_path.LastIndexOf("/");
                        string fbx_name = mesh_path.Substring(index + 1);
                        fbx_name = fbx_name.Substring(0,fbx_name.Length-4);
//                        Debug.Log("MESH PATH :: " + fbx_name + " :: " + mf.sharedMesh.name + " :: " + mesh_dir);
                        
                        string asset_path = mesh_dir+fbx_name+"__"+mf.sharedMesh.name+".asset";
                        Mesh newMesh = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                        
                        if( newMesh == null ) {
                            newMesh = new Mesh();
                            newMesh.name = mf.sharedMesh.name;
                            newMesh.vertices = mf.sharedMesh.vertices;
                            newMesh.uv = mf.sharedMesh.uv;
                            newMesh.uv2 = mf.sharedMesh.uv2;
                            newMesh.uv2 = mf.sharedMesh.uv2;
                            newMesh.colors = mf.sharedMesh.colors;
                            newMesh.normals = mf.sharedMesh.normals;
                            newMesh.boneWeights = mf.sharedMesh.boneWeights;
                            newMesh.bindposes = mf.sharedMesh.bindposes;
                            newMesh.triangles = mf.sharedMesh.triangles;
                            Vector2[] uv = mf.sharedMesh.uv;
                            Vector2[] newUv = new Vector2[newMesh.vertexCount];
                            
                            
                            for(int j = 0 ; j < newMesh.vertexCount; j++){
                                float x = xMin + uv[j].x * xDiff; 
                                //float x = xMax - uv[i].x * xDiff; 
                                //float y = yMin + uv[i].y * yDiff;
                                //float y = 1f - yMin - uv[i].y * yDiff;
                                float y = 1f - yMax + uv[j].y * yDiff;
                                
                                newUv[j] = new Vector2( x, y);
                                //newUv[i] = new Vector2( 1f - x,1f - y);
                                //Debug.Log((float)uv[i].x + " " + (float)uv[i].y);
                                //Debug.Log("new " + (float)newUv[i].x + " " + (float)newUv[i].y);
                            }
                            newMesh.uv = newUv;
                            //                    AssetDatabase.CopyAsset(mesh_path,mesh_dir+"/"+fbx_name);
                            AssetDatabase.CreateAsset(newMesh,asset_path);
                            newMesh = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                        }
                        mf.sharedMesh = newMesh;
                    }
                    else {
                        SkinnedMeshRenderer smr = am.GetComponent<SkinnedMeshRenderer>();
                        if( smr != null ) {
                            string mesh_path = AssetDatabase.GetAssetPath( smr.sharedMesh );
                            int index = mesh_path.LastIndexOf("/");
                            string fbx_name = mesh_path.Substring(index + 1);
                            fbx_name = fbx_name.Substring(0,fbx_name.Length-4);
                            //                                            Debug.Log("SKINNED MESH PATH :: " + fbx_name + " :: " + smr.sharedMesh.name + " :: " + mesh_dir);
                            
                            string asset_path = mesh_dir+fbx_name+"__"+smr.sharedMesh.name+".asset";
                            Mesh new_asset = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                            
                            if( new_asset == null ) {
                                new_asset = new Mesh();
                                new_asset.name = smr.sharedMesh.name;
                                new_asset.vertices = smr.sharedMesh.vertices;
                                new_asset.uv = smr.sharedMesh.uv;
                                new_asset.uv2 = smr.sharedMesh.uv2;
                                new_asset.uv2 = smr.sharedMesh.uv2;
                                new_asset.colors = smr.sharedMesh.colors;
                                new_asset.normals = smr.sharedMesh.normals;
                                new_asset.boneWeights = smr.sharedMesh.boneWeights;
                                new_asset.bindposes = smr.sharedMesh.bindposes;
                                new_asset.triangles = smr.sharedMesh.triangles;
                                Vector2[] uv = smr.sharedMesh.uv;
                                Vector2[] newUv = new Vector2[new_asset.vertexCount];
                                
                                for(int l = 0 ; l < new_asset.vertexCount; l++){
                                    float x = xMin + uv[l].x * xDiff; 
                                    float y = 1f - yMax + uv[l].y * yDiff;
                                    newUv[l] = new Vector2( x, y);
                                    //Debug.Log((float)uv[i].x + " " + (float)uv[i].y);
                                    //Debug.Log("new " + (float)newUv[i].x + " " + (float)newUv[i].y);
                                }
                                new_asset.uv = newUv;
                                
                                AssetDatabase.CreateAsset(new_asset,asset_path);
                                new_asset = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                            }
                            smr.sharedMesh = new_asset;
                        }
                    }
                }
            }
        }

        ProcessStaticBatchBuilder( streamingData );

        // Handle the critters
        SB_CritterSpawn[] critter_spawns = streamingData.GetComponentsInChildren<SB_CritterSpawn>(true);
        for( int i = 0; i < critter_spawns.Length; i++ ) {
            SB_CritterSpawn sb_cs = critter_spawns[i];
            if( sb_cs != null ) {
                for( int j = 0; j < sb_cs.spawnCritters.Length; j++ ) {
                    CritterToSpawn cs = sb_cs.spawnCritters[j];
                    if( cs != null && cs.critter != null ) {
                        string critter_prefab_path = mesh_dir + cs.critter.name + ".prefab";

                        Object new_critter_object = AssetDatabase.LoadAssetAtPath( critter_prefab_path, typeof(Object) );
                        if( new_critter_object == null ) {
//                            string og_path = AssetDatabase.GetAssetPath( cs.critter );
//                            Debug.Log("ORIGINAL CRITTER PATH " + og_path);
//                            bool result = AssetDatabase.CopyAsset( og_path, critter_prefab_path );
//                            Debug.Log ("NEW CRITTER " + critter_prefab_path + " :: " + result );

                            GameObject new_critter = GameObject.Instantiate( cs.critter ) as GameObject;

                            // cache our critter info data.
                            SetupCritterInfoData( new_critter );

                            AtlasMaterial[] critter_atlas_mats = new_critter.GetComponentsInChildren<AtlasMaterial>(true);
                            if( critter_atlas_mats.Length > 0 ) {
                                for( int k = 0; k < critter_atlas_mats.Length; k++ ) {
                                    AtlasMaterial am = critter_atlas_mats[k];
                                    if( am != null && am.atlasName != null && am.atlasName.Length > 0 ) {
                                        float xMin,xMax,yMin,yMax,xDiff,yDiff;
                                        am.GetData(out xMin,out xMax,out yMin,out yMax,out xDiff,out yDiff);

                                        SkinnedMeshRenderer smr = am.GetComponent<SkinnedMeshRenderer>();
                                        if( smr != null ) {
                                            string mesh_path = AssetDatabase.GetAssetPath( smr.sharedMesh );
                                            int index = mesh_path.LastIndexOf("/");
                                            string fbx_name = mesh_path.Substring(index + 1);
                                            fbx_name = fbx_name.Substring(0,fbx_name.Length-4);
//                                            Debug.Log("SKINNED MESH PATH :: " + fbx_name + " :: " + smr.sharedMesh.name + " :: " + mesh_dir);
                                            
                                            string asset_path = mesh_dir+cs.critter.name+fbx_name+"__"+smr.sharedMesh.name+".asset";
                                            Mesh new_asset = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                                            
                                            if( new_asset == null ) {
                                                new_asset = new Mesh();
                                                new_asset.name = smr.sharedMesh.name;
                                                new_asset.vertices = smr.sharedMesh.vertices;
                                                new_asset.uv = smr.sharedMesh.uv;
                                                new_asset.uv2 = smr.sharedMesh.uv2;
                                                new_asset.uv2 = smr.sharedMesh.uv2;
                                                new_asset.colors = smr.sharedMesh.colors;
                                                new_asset.normals = smr.sharedMesh.normals;
                                                new_asset.boneWeights = smr.sharedMesh.boneWeights;
                                                new_asset.bindposes = smr.sharedMesh.bindposes;
                                                new_asset.triangles = smr.sharedMesh.triangles;
                                                Vector2[] uv = smr.sharedMesh.uv;
                                                Vector2[] newUv = new Vector2[new_asset.vertexCount];
                                                
                                                for(int l = 0 ; l < new_asset.vertexCount; l++){
                                                    float x = xMin + uv[l].x * xDiff; 
                                                    float y = 1f - yMax + uv[l].y * yDiff;
                                                    newUv[l] = new Vector2( x, y);
                                                    //Debug.Log((float)uv[i].x + " " + (float)uv[i].y);
                                                    //Debug.Log("new " + (float)newUv[i].x + " " + (float)newUv[i].y);
                                                }
                                                new_asset.uv = newUv;

                                                AssetDatabase.CreateAsset(new_asset,asset_path);
                                                new_asset = AssetDatabase.LoadAssetAtPath(asset_path,typeof(Mesh)) as Mesh;
                                            }
                                            smr.sharedMesh = new_asset;
                                        }
                                        else {
                                            Debug.LogError("FISH WITHOUT SKINED MESH!");
                                        }
                                    }
                                }
                            }

                            new_critter_object = PrefabUtility.CreateEmptyPrefab( critter_prefab_path );
                            new_critter_object = PrefabUtility.ReplacePrefab( new_critter, new_critter_object );

                        }
                        cs.critter = new_critter_object;
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
    }
            
            /// <summary>
    /// Cut LevelStreamingData off the scene, save as an asset bundle.
    /// </summary>
    /// <returns>Saved path of level bundle.</returns>
    string ProcessScene(string sceneFolder, string sceneName)
    {
        Debug.Log("Processing:" + sceneFolder + sceneName + ".unity");
        if (!HasSceneOpened(sceneFolder + sceneName + ".unity")) {
            return string.Empty;
        }

        // Attempt to cut LevelStreamingData, save as a prefab.
        GameObject streamingData = GameObject.Find("LevelStreamingData");
        if (streamingData == null) {            
            return string.Empty;
        }


        ProcessLightmaps( streamingData );

        ProcessAtlasUVCorrection( sceneName, streamingData );


        // save our build date into the level streaming data prefab.
        // this is then outputted when we load the level.
        GameObject level_build_date_obj = GameObject.Find("LevelBuildStringObject");
        if (level_build_date_obj != null) {
            LevelBuildDateString ld = level_build_date_obj.GetComponent<LevelBuildDateString>();
            if( ld != null ) {
                ld.buildDate = System.DateTime.Now.ToString("G") + " " + gitVersion;
            }
        }

        string fullPrefabPath = GetFullPrefabPath(sceneName);

        string emptyPrefabDirectory = fullPrefabPath.Substring(0,fullPrefabPath.LastIndexOf("/"));
        if(!Directory.Exists(emptyPrefabDirectory))
        {
            Directory.CreateDirectory(emptyPrefabDirectory);
        }

        AssetDatabase.SaveAssets();

        // Cache streaming data as a prefab so that we can reattach to scene later, after the build.
        Object cachedStreamingData = PrefabUtility.CreateEmptyPrefab(fullPrefabPath);
        string cachedStreamingDataPath = AssetDatabase.GetAssetPath(cachedStreamingData);
        Debug.Log(AssetDatabase.GetAssetPath(cachedStreamingData));

        if(!cachedStreamingData)
        {
            Debug.LogError("cachedStreamingData is not valid");
            return string.Empty;
        }
        //  cachedStreamingData = null;
        //         AssetDatabase.Refresh();
        //         AssetDatabase.SaveAssets();
        cachedStreamingData = PrefabUtility.ReplacePrefab(streamingData,cachedStreamingData);
        //         cachedStreamingDataPath = AssetDatabase.GetAssetPath(cachedStreamingData);
        //         AssetDatabase.Refresh();
        //         AssetDatabase.SaveAssets();

        cachedStreamingDataPath = AssetDatabase.GetAssetPath(cachedStreamingData);
        Debug.Log(cachedStreamingDataPath);
        string levelBundlePath = BuildLevelAssetBundle(cachedStreamingData, sceneName, false);
        Debug.Log("Bundle saved to: " + levelBundlePath);

        //         EditorApplication.SaveScene(EditorApplication.currentScene);

        return levelBundlePath;
    }

}
