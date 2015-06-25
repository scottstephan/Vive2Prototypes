using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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
    ///     we have provided the source code for our utility functions regarding MonoScripts.
    /// 
    /// If you have any suggestions on how to optimize the utility functions 
    /// that come with our tools then please do not hesitate to head over to our forums.
    /// </summary>
    public class MonoScriptUtility
    {
        private const string LoadingEntities = "Loading scripts...";
        private const string CSharpFileMask = "*.cs";
        private const string DLLFileMask = "*.dll";
        private const string StandardAssets = "{0}/Standard Assets";
        private const string LoadingFromScripts = "Loading from scripts...";
        private const string LoadingFromAssemblies = "Loading from assemblies";

        /// <summary>
        /// Retreives all the script in the Assets folder.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetScripts()
        {
            return GetScripts(string.Empty);
        }

        /// <summary>
        /// Retreives all the scripts in a subfolder within the Assets folder.
        /// </summary>
        /// <param name="scriptsPath">the subfolder</param>
        /// <returns></returns>
        public static List<Type> GetScripts(string scriptsPath)
        {
            List<Type> monoScripts = new List<Type>();

            monoScripts.AddRange(GetScriptFromAssemblies(Application.dataPath, scriptsPath, DLLFileMask));

            monoScripts.AddRange(GetScriptsFromPath(Application.dataPath, scriptsPath, CSharpFileMask));

            EditorUtility.ClearProgressBar();

            return monoScripts;
        }

        private static List<Type> GetScriptFromAssemblies(string datapath, string path, string fileMask)
        {
            int pathLength = datapath.Length - 6;
            DirectoryInfo dir = new DirectoryInfo(datapath + path);
            FileInfo[] fileInfos = dir.GetFiles(fileMask, SearchOption.AllDirectories);

            float progress = 0.5f / fileInfos.Count();
            float totalProgress = 0.0f;

            List<Type> monoScripts = new List<Type>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                EditorUtility.DisplayProgressBar(LoadingEntities, LoadingFromAssemblies, totalProgress);
                string filePath = fileInfo.FullName.Remove(0, pathLength);
                monoScripts.AddRange(GetScriptsFromAssembly(filePath));
                totalProgress += progress;
            }

            return monoScripts;
        }

        private static List<Type> GetScriptsFromAssembly(string assemblyName)
        {
            Assembly assembly = Assembly.LoadFrom(assemblyName);

            Type[] types = assembly.GetTypes();

            List<Type> monoScripts = new List<Type>();
            foreach (Type type in types)
            {
                if(CheckType(type))
                {
                    monoScripts.Add(type);
                }
            }

            return monoScripts;
        }

        private static IEnumerable<Type> GetScriptsFromPath(string datapath, string path, string fileMask)
        {
            return Search(datapath, path, fileMask);
        }

        private static IEnumerable<Type> Search(string datapath, string path, string fileMask)
        {
            int pathLength = datapath.Length - 6;
            DirectoryInfo dir = new DirectoryInfo(datapath + path);
            FileInfo[] fileInfos = dir.GetFiles(fileMask, SearchOption.AllDirectories);
            Type monoScriptType = typeof(MonoScript);

            float progress = 0.5f / fileInfos.Count();
            float totalProgress = 0.5f;

            List<Type> monoScripts = new List<Type>();
            foreach (FileInfo fileInfo in fileInfos)
            {
                EditorUtility.DisplayProgressBar(LoadingEntities, LoadingFromScripts, totalProgress);
                LoadAsset(fileInfo, pathLength, monoScriptType, monoScripts);
                totalProgress += progress;
            }

            return monoScripts;
        }

        private static void LoadAsset(FileInfo fileInfo, int pathLength, Type monoScriptType, List<Type> monoScripts)
        {
            string filePath = fileInfo.FullName.Remove(0, pathLength);
            MonoScript script = AssetDatabase.LoadAssetAtPath(filePath, monoScriptType) as MonoScript;
            if (script != null)
            {
                Type type = script.GetClass();

                if(CheckType(type))
                {
                    monoScripts.Add(type);
                }
            }
        }

        private static bool CheckType(Type type)
        {
            if (type != null)
            {
                return true;
            }
            return false;
        }
    }
}
