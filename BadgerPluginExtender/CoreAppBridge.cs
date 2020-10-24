using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Threading;
using AryxDevLibrary.utils;
using AryxDevLibrary.utils.logger;
using BadgerPluginExtender.dto;
using BadgerPluginExtender.interfaces;

namespace BadgerPluginExtender
{
    public class CoreAppBridge
    {
        private static CoreAppBridge _instance;
        public static CoreAppBridge Instance
        {
            get { return _instance ?? (_instance = new CoreAppBridge()); }
            private set { _instance = value; }
        }

        public IDictionary<String, MethodRecordWithInstance> AppPresentedMethods { get; private set; }

        private static readonly Logger _logger = Logger.LastLoggerInstance;


        public void RegisterMethodsForPlugin(IPresentableObject iPresentableObject)
        {
            MethodRecordWithInstance[] mRecords = iPresentableObject.GetMethodToPresents();
            if (mRecords.Length == 0)
            {
                return;
            }

            if (AppPresentedMethods == null)
            {
                AppPresentedMethods = new Dictionary<string, MethodRecordWithInstance>(mRecords.Length);
            }

            foreach (MethodRecordWithInstance mRec in mRecords)
            {
                AppPresentedMethods.Add(mRec.TargetHookName, mRec);
            }
        }




        public void PlayHook(string hookName, object[] arg1)
        {
            PlayHookAndReturn(hookName, arg1, null);

        }

        public object PlayHookAndReturn(string hookName, object[] arg1, Type returnType)
        {
            if (AppPresentedMethods == null) return null;

            if (!AppPresentedMethods.Any(r => r.Key.Equals(hookName)))
            {
                return ReflexionUtils.GetDefaultValue(returnType);
            }

            MethodRecordWithInstance methodRecordWithInstance = AppPresentedMethods.Where(r => r.Key.Equals(hookName)).Select(r => r.Value).First();

            object oRet = PlayOneMethodRecord(hookName, arg1, methodRecordWithInstance, returnType);

            return oRet ?? ReflexionUtils.GetDefaultValue(returnType);
        }





        private static object PlayOneMethodRecord(string hookName, object[] arg1, MethodRecordWithInstance methodRecord, Type returnType)
        {
            if (methodRecord == null) return null;

            MethodInfo method = null;
            object instance = null;
            if (methodRecord.Instance == null && methodRecord.StaticType != null)
            {
                instance = null;
                method = methodRecord.StaticType.GetMethod(methodRecord.MethodResponder);
            }
            else if (methodRecord.Instance != null)
            {

                instance = methodRecord.Instance;
                method = instance.GetType().GetMethod(methodRecord.MethodResponder);
            }
            else
            {
                return null;
            }

            // TODO ALB gérer le "method is null"
            // TODO ALB gérer le STA ou pas de dispatcher paramétré.

            if (returnType != null)
            {
                if (method.ReturnType != returnType)
                {
                    throw new Exception("Erreur lors d'appel de l'ancre XX : le type de retour ne correspond pas");
                }
            }

            if (method.GetParameters().Any(r => !r.IsOptional) && (arg1 == null || arg1.Length == 0))
            {
                throw new Exception("Erreur lors d'appel de l'ancre XX : le nombre d'arguments ne correspond pas");
            }

            object ret = null;
            if (method.GetParameters().Length > 0 && arg1 != null)
            {
                int i = 0;
                foreach (ParameterInfo parameterInfo in method.GetParameters())
                {
                    if (!(parameterInfo.ParameterType == arg1[i].GetType()))
                    {
                        throw new Exception(String.Format("CoreAppBridge::PlayOneMethodRecord : Le parametre {0} est de type {1}. {2} fournit. Les types ne correspondent pas", parameterInfo.Name, parameterInfo.ParameterType.Name, arg1[i].GetType().Name));

                    }
                    i++;
                }

                ret = RunMethod(arg1, methodRecord, method, instance);


            }
            else
            {

                ret = RunMethod(arg1, methodRecord, method, instance);

            }

            return returnType != null ? ret : null;
        }

        private static object RunMethod(object[] argsObjectArray, MethodRecordWithInstance methodWinstance,
            MethodInfo methodInfo, object instance)
        {
            try
            {

                object ret = null;
                if (methodWinstance.Dispatcher != null)
                {
                    methodWinstance.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                        new Action(() =>
                        {
                            ret = methodInfo.Invoke(instance, BindingFlags.Instance, null, argsObjectArray ?? null, null);
                        })
                        );
                }
                else
                {
                    ret = methodInfo.Invoke(instance, BindingFlags.Instance, null, argsObjectArray ?? null, null);
                }
                return ret;
            }
            catch (Exception ex)
            {
                _logger.Error("Erreur lors du lancement de la méthode {0} avec comme paramétre(s) {1}.", methodInfo.Name, argsObjectArray);
                throw ex;
            }
        }
    }
}
