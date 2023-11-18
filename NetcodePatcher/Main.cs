﻿using BepInEx;
using BepInEx.Logging;
using Mono.Cecil;
using NetcodePatcher.CodeGen;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Unity.Netcode;
using Unity.Netcode.Editor.CodeGen;
using UnityEngine;

namespace NetcodePatcher
{
    public static class Patcher
    {
        public static IEnumerable<string> TargetDLLs
        {
            get
            {
                return Patcher.CollectTargetDLLs();
            }
        }

        public static void Initialize()
        {
            Patcher.Logger.LogMessage("Initializing NetcodePatcher");
            HashSet<string> hashSet = new HashSet<string>();
            List<string> references = new List<string>() {
                Paths.ManagedPath + "\\Unity.Netcode.Runtime.dll",
                Paths.ManagedPath + "\\UnityEngine.CoreModule.dll",
                Paths.ManagedPath + "\\Unity.Netcode.Components.dll",
                Paths.ManagedPath + "\\Unity.Networking.Transport.dll",
            };
            foreach (string text3 in Directory.GetFiles(Paths.PluginPath, "*.dll", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(text3);
                if (!fileName.ToLower().Contains("mmhook"))
                {
                    foreach (TypeDefinition typeDefinition in AssemblyDefinition.ReadAssembly(text3).MainModule.Types)
                    {
                        

                        if (typeDefinition.BaseType != null)
                        {
;                           // check if subclass of NetworkBehaviour
                            if (typeDefinition.IsSubclassOf(typeof(NetworkBehaviour).FullName) || typeDefinition.HasInterface(typeof(INetworkMessage).FullName) || typeDefinition.HasInterface(typeof(INetworkSerializable).FullName))
                            {
    
                                hashSet.Add(text3);
                                break;
                            }
                        }
                    }
                }
            }
            foreach (string text4 in hashSet)
            {
                try
                {
                    Patcher.Logger.LogMessage("Patching : " + Path.GetFileName(text4));

                    ILPostProcessorFromFile.ILPostProcessFile(text4, references.ToArray(), (warning) =>
                    {
                        // replace || with new line
                        warning = warning.Replace("||  ", "\r\n").Replace("||", " ");
                        Patcher.Logger.LogWarning($"Warning when patching ({Path.GetFileName(text4)}): {warning}");
                    }, 
                    (error) =>
                    {
                        error = error.Replace("||  ", "\r\n").Replace("||", " ");
                        Patcher.Logger.LogError($"Error when patching ({Path.GetFileName(text4)}): {error}");
                    });
                    
                }
                catch (Exception exception)
                {
                    // error
                    Patcher.Logger.LogWarning($"Did not patch ({Path.GetFileName(text4)}): {exception.Message} (Already patched?)");
                }
            }
        }

        public static void Patch(AssemblyDefinition _)
        {
        }

        private static IEnumerable<string> CollectTargetDLLs()
        {
            return new List<string>();
        }

        private static readonly ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("NetcodePatcher");
    }
}