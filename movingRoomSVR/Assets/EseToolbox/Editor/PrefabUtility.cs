using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = System.Object;

namespace Ese
{
    /// <summary>
    /// Author: 
    ///     Skjalg S. Mæhre
    /// 
    /// Company: 
    ///     Exit Strategy Entertainment
    /// 
    /// Website: 
    ///     http://www.exitstrategyentertainment.com/
    /// 
    /// Date:
    ///     28th of June, 2011
    /// 
    /// Purpose:
    ///     For those of you who want to creat your own tools, 
    ///     or just have a look at the algorithms that were used in the creation of these tools 
    ///     we have provided the source code for our utility functions regarding Prefabs.
    /// 
    /// If you have any suggestions on how to optimize the utility functions 
    /// that come with our tools then please do not hesitate to head over to our forums.
    /// </summary>
    public class PrefabUtility
    {
        private const string LocatingPrefabsTitle = "Locating prefabs...";
        private const string SearchingForPrefabs = "Searching for prefabs...";
        private const string LoadingPrefabs = "Loading prefabs...";
        private const string FileMask = "*.prefab";

        /// <summary>
        /// Retreives all the prefabs in the Assets folder
        /// </summary>
        /// <returns></returns>
        public static List<GameObject> GetPrefabs()
        {
            return GetPrefabs(string.Empty);
        }

        /// <summary>
        /// Retreives all the prefabs in a subfolder within the Assets folder
        /// </summary>
        /// <param name="prefabPath">the subfolder</param>
        /// <returns></returns>
        public static List<GameObject> GetPrefabs(string prefabPath)
        {
            return GetPrefabsFromPath(Application.dataPath, prefabPath);
        }

        private static List<GameObject> GetPrefabsFromPath(string datapath, string path)
        {
            EditorUtility.DisplayProgressBar(LocatingPrefabsTitle, SearchingForPrefabs, 0);
            int pathLength = datapath.Length - 6;
            DirectoryInfo dir = new DirectoryInfo(datapath + path);
            FileInfo[] fileInfos = dir.GetFiles(FileMask, SearchOption.AllDirectories);

            return LoadPrefabs(fileInfos, pathLength);
        }

        private static List<GameObject> LoadPrefabs(FileInfo[] fileInfos, int path)
        {
            float progress = 0.5f / fileInfos.Length;
            float overallProgress = 0f;

            List<GameObject> assetList = new List<GameObject>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                LoadPrefab(overallProgress, fileInfo, path, assetList);

                overallProgress += progress;
            }
            return assetList;
        }

        private static void LoadPrefab(float overallProgress, FileInfo fileInfo, int path, List<GameObject> assetList)
        {
            GameObject go = LoadAsset(overallProgress, fileInfo, path);
            if (go)
            {
                assetList.Add(go);
            }
        }

        private static GameObject LoadAsset(float overallProgress, FileInfo fileInfo, int path)
        {
            EditorUtility.DisplayProgressBar(LocatingPrefabsTitle, LoadingPrefabs, overallProgress);
            string filePath = fileInfo.FullName;
            Object asset = Resources.LoadAssetAtPath(filePath.Remove(0, path), typeof(GameObject));
            return asset as GameObject;
        }
    }
}
