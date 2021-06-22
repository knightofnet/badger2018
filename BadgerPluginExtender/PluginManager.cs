using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using AryxDevLibrary.utils.logger;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;

namespace BadgerPluginExtender
{
    public class PluginManager
    {

        public IDictionary<String, IGenericPluginInterface> PluginsInstance { get; private set; }

        public bool IsAnyPluginLoaded { get; set; }

        private static readonly Logger _logger = Logger.LastLoggerInstance;
        public void LoadPluginsFromDir(string directoryPath)
        {
            DirectoryInfo dirPlugin = new DirectoryInfo(directoryPath);
            if (!dirPlugin.Exists)
            {
                _logger.Warn("Le dossier contenant les plugins n'existe pas.");
                return;
            }

            string[] dllFileNames = Directory.GetFiles(dirPlugin.FullName, "*.dll");

            //  Next we have to load the assemblies. Therefore we are using Reflections (System.Reflection).
            ICollection<Assembly> assemblies = new List<Assembly>(dllFileNames.Length);
            foreach (string dllFile in dllFileNames)
            {
                AssemblyName an = AssemblyName.GetAssemblyName(dllFile);
                Assembly assembly = Assembly.Load(an);
                assemblies.Add(assembly);
            }

            // Now we have loaded all assemblies from our predefined location, we can search for all types that implement our Interface IPlugin.
            Type pluginType = typeof(IGenericPluginInterface);
            ICollection<Type> pluginTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                if (assembly == null) continue;

                Type[] types = assembly.GetTypes();
                foreach (Type type in types)
                {
                    if (type.IsInterface || type.IsAbstract)
                    {
                        continue;
                    }
                    else
                    {
                        if (type.GetInterface(pluginType.FullName) != null)
                        {
                            pluginTypes.Add(type);
                        }
                    }
                }
            }

            // Last we create instances from our found types using Reflections.
            PluginsInstance = new Dictionary<string, IGenericPluginInterface>(pluginTypes.Count);
            foreach (Type type in pluginTypes)
            {
                bool isAddPlugin = true;
                IGenericPluginInterface plugin = (IGenericPluginInterface)Activator.CreateInstance(type);

                if (!IsPluginOk(plugin)) isAddPlugin = false;

                if (!isAddPlugin)
                {
                    continue;
                }


                if (PluginsInstance.ContainsKey(plugin.GetPluginInfo().Name))
                {
                    _logger.Warn("Le plugin " + plugin.GetPluginInfo().Name + " est déjà chargé.");
                    continue;
                }


                PluginsInstance.Add(plugin.GetPluginInfo().Name, plugin);

                IsAnyPluginLoaded = true;

                _logger.Debug("Extension {0}[{1}] chargée", plugin.GetPluginInfo().Name, plugin.GetPluginInfo().Version);
            }


        }

        private bool IsPluginOk(IGenericPluginInterface plugin)
        {
            foreach (MethodRecord mRec in plugin.GetMethodToRecords())
            {
                if (mRec.MethodResponder == null)
                {
                    _logger.Warn("Erreur lors du chargement du plugin " + plugin.GetPluginInfo().Name + " : le nom de la méthode répondante est null.");
                    return false;
                }

                if (mRec.TargetHookName == null)
                {
                    _logger.Warn("Erreur lors du chargement du plugin " + plugin.GetPluginInfo().Name + " : le nom de l'ancre est null.");
                    return false;
                }
            }

            return true;
        }

        public void PlayHook(string hookName, object[] arg1)
        {
            PlayHookAndReturn(hookName, arg1, null);

        }

        public HookReturns PlayHookAndReturn(string hookName, object[] arg1, Type returnType)
        {
            HookReturns retList = new HookReturns();
            retList.ReturnType = returnType;
            if (!IsAnyPluginLoaded || (PluginsInstance != null && !PluginsInstance.Any()))
            {
                return retList;
            }

            foreach (KeyValuePair<string, IGenericPluginInterface> valuePair in PluginsInstance)
            {
                List<MethodRecord> methodRecords = valuePair.Value.GetMethodToRecords().Where(r => r.TargetHookName.Equals(hookName)).ToList();

                foreach (MethodRecord methodRecord in methodRecords)
                {
                    object oRet = PlayOneMethodRecord(hookName, arg1, methodRecord, returnType, valuePair.Value);
                    HookReturn r = new HookReturn();
                    r.MethodRecord = methodRecord;
                    r.PluginInfo = valuePair.Value.GetPluginInfo();
                    r.ReturnedObject = oRet;
                    retList.Add(r);
                }


            }

            return retList;
        }

        public HookReturns PlayHookPluginAndReturn(String pluginName, string hookName, object[] arg1, Type returnType)
        {
            HookReturns retList = new HookReturns();
            retList.ReturnType = returnType;
            if (!IsAnyPluginLoaded || (PluginsInstance != null && !PluginsInstance.Any()))
            {
                return retList;
            }

            foreach (KeyValuePair<string, IGenericPluginInterface> valuePair in PluginsInstance)
            {
                if (!valuePair.Key.Equals(pluginName)) continue;

                List<MethodRecord> methodRecords = valuePair.Value.GetMethodToRecords().Where(r => r.TargetHookName.Equals(hookName)).ToList();

                foreach (MethodRecord methodRecord in methodRecords)
                {
                    object oRet = PlayOneMethodRecord(hookName, arg1, methodRecord, returnType, valuePair.Value);
                    HookReturn r = new HookReturn();
                    r.MethodRecord = methodRecord;
                    r.PluginInfo = valuePair.Value.GetPluginInfo();
                    r.ReturnedObject = oRet;
                    retList.Add(r);
                }


            }

            return retList;
        }



        private static object PlayOneMethodRecord(string hookName, object[] arg1, MethodRecord a, Type returnType, IGenericPluginInterface instance)
        {
            try
            {

                if (a != null)
                {

                    MethodInfo method = instance.GetType().GetMethod(a.MethodResponder);

                    if (returnType != null)
                    {
                        if (method.ReturnType != returnType)
                        {
                            throw new Exception(String.Format(
                                "Erreur lors d'appel de l'ancre {0} : le type de retour ne correspond pas", hookName));
                        }
                    }

                    if (method.GetParameters().Length > 0 && (arg1 == null || arg1.Length == 0 || arg1.Length != method.GetParameters().Length))
                    {
                        throw new Exception(String.Format("PluginManager::PlayOneMethodRecord : La méthode {0} est attendue avec {1} paramètre(s). {2} transmi(s). Le nombre de paramètres obligatoire ne correspond pas", method.Name, method.GetParameters().Length, arg1.Length));
                    }

                    object ret = null;
                    if (method.GetParameters().Length > 0 && arg1 != null)
                    {
                        int i = 0;
                        foreach (ParameterInfo parameterInfo in method.GetParameters())
                        {
                            if (!(parameterInfo.ParameterType == arg1[i].GetType()))
                            {
                                throw new Exception(String.Format("PluginManager::PlayOneMethodRecord : Le parametre {0} est de type {1}. {2} fournit. Les types ne correspondent pas", parameterInfo.Name, parameterInfo.ParameterType.Name, arg1[i].GetType().Name));
                            }
                            i++;
                        }

                        ret = method.Invoke(instance, BindingFlags.Instance, null, arg1, null);


                    }
                    else
                    {
                        ret = method.Invoke(instance, BindingFlags.Instance, null, null, null);
                    }

                    return returnType != null ? ret : null;
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                _logger.Error(ex.GetType().Name);
                _logger.Error(ex.StackTrace);
                if (ex.InnerException != null)
                {
                    _logger.Error("InnerException");
                    _logger.Error(ex.InnerException.Message);
                    _logger.Error(ex.InnerException.GetType().Name);
                    _logger.Error(ex.InnerException.StackTrace);
                }
            }
            return null;
        }
    }
}
