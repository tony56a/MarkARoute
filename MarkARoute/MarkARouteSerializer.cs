using ColossalFramework;
using ICities;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;

namespace MarkARoute
{
    public class MarkARouteSerializer : SerializableDataExtensionBase
    {
        private readonly string routeDataKey = "MarkARoute";
        private readonly string signDataKey = "MarkARouteSigns";
        private readonly string dynamicSignDataKey = "MarkARouteDynamicSigns";
        private readonly string overrideSignDataKey = "MarkARouteOverrideSigns";

        private Object thisLock = new Object();

        public override void OnSaveData()
        {
            base.OnSaveData();
            LoggerUtils.Log("Saving routes and signs");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream routeMemoryStream = new MemoryStream();
            MemoryStream signMemoryStream = new MemoryStream();
            MemoryStream dynamicSignMemoryStream = new MemoryStream();
            MemoryStream overrideSignMemoryStream = new MemoryStream();

            try
            {
                RouteContainer[] routeNames = RouteManager.instance.GetRouteMarkers();

                if (routeNames != null )
                {
                    binaryFormatter.Serialize(routeMemoryStream, routeNames);
                    serializableDataManager.EraseData(routeDataKey);
                    serializableDataManager.SaveData(routeDataKey, routeMemoryStream.ToArray());
                    LoggerUtils.Log("Routes have been saved!");

                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save routes, as the array is null!");
                }

                SignContainer[] signs = RouteManager.instance.m_signList.ToArray();
                if (signs != null)
                {
                    binaryFormatter.Serialize(signMemoryStream, signs);
                    serializableDataManager.EraseData(signDataKey);

                    serializableDataManager.SaveData(signDataKey, signMemoryStream.ToArray());
                    LoggerUtils.Log("Signs have been saved!");

                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save signs, as the array is null!");
                }

                DynamicSignContainer[] dynamicSigns = RouteManager.instance.m_dynamicSignList.ToArray();
                if (dynamicSignMemoryStream != null)
                {
                    binaryFormatter.Serialize(dynamicSignMemoryStream, dynamicSigns);
                    serializableDataManager.EraseData(dynamicSignDataKey);

                    serializableDataManager.SaveData(dynamicSignDataKey, dynamicSignMemoryStream.ToArray());
                    LoggerUtils.Log("Dynamic signs have been saved!");

                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save dynamic signs, as the array is null!");
                }

                OverrideSignContainer[] overrideSigns = RouteManager.instance.GetOverrideSigns();
                if (overrideSignMemoryStream != null)
                {
                    binaryFormatter.Serialize(overrideSignMemoryStream, overrideSigns);
                    serializableDataManager.EraseData(overrideSignDataKey);

                    serializableDataManager.SaveData(overrideSignDataKey, overrideSignMemoryStream.ToArray());
                    LoggerUtils.Log("Override signs have been saved!");
                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save dynamic signs, as the array is null!");
                }

            }
            catch (Exception ex)
            {
                LoggerUtils.LogException(ex);
            }
            finally
            {
                routeMemoryStream.Close();
            }
        }

        public override void OnLoadData()
        {
            base.OnLoadData();
            LoggerUtils.Log("Loading data");
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            RouteManager instance = RouteManager.instance;

            lock (thisLock)
            {
                byte[] loadedRouteData = serializableDataManager.LoadData(routeDataKey);
                LoggerUtils.Log("Routes loaded, parsing data");
                if (loadedRouteData != null && loadedRouteData.Length > 0)
                {
                    using (MemoryStream routeMemoryStream = new MemoryStream(loadedRouteData))
                    {
                        try
                        {
                            RouteContainer[] routeNames = binaryFormatter.Deserialize(routeMemoryStream) as RouteContainer[];

                            if (routeNames != null && routeNames.Length > 0)
                            {
                                instance.Load(routeNames);
                            }
                            else
                            {
                                LoggerUtils.LogWarning("Couldn't load routes, as the array is null!");
                            }
                        }
                        catch (Exception ex)
                        {
                            LoggerUtils.LogException(ex);

                        }
                    }
                }
                else
                {
                    LoggerUtils.LogWarning("Found no data to load");
                }
            }
            lock (thisLock)
            {
                byte[] loadedSignData = serializableDataManager.LoadData(signDataKey);
                LoggerUtils.Log("Signs loaded, parsing data");
                if (loadedSignData != null && loadedSignData.Length > 0)
                {
                    MemoryStream signMemoryStream = new MemoryStream(loadedSignData);

                    LoggerUtils.Log("MemoryStream init");

                    try
                    {

                        SignContainer[] signNames = binaryFormatter.Deserialize(signMemoryStream) as SignContainer[];
                        LoggerUtils.Log("Deserialized");

                        if (signNames != null && signNames.Length > 0)
                        {
                            instance.LoadSigns(signNames.ToList());
                        }
                        else
                        {
                            LoggerUtils.LogWarning("Couldn't load routes, as the array is null!");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerUtils.LogException(ex);

                    }
                    finally
                    {
                        signMemoryStream.Close();
                    }
                }
                else
                {
                    LoggerUtils.LogWarning("Found no data to load");
                }
            }
            lock(thisLock)
            {
                byte[] loadedDynamicSignData = serializableDataManager.LoadData(dynamicSignDataKey);
                LoggerUtils.Log("Dynamic signs loaded, parsing data");
                if (loadedDynamicSignData != null && loadedDynamicSignData.Length > 0)
                {
                    MemoryStream dynamicSignMemoryStream = new MemoryStream(loadedDynamicSignData);

                    try
                    {
                        DynamicSignContainer[] dynamicSignNames = binaryFormatter.Deserialize(dynamicSignMemoryStream) as DynamicSignContainer[];

                        if (dynamicSignNames != null && dynamicSignNames.Length > 0)
                        {
                            instance.LoadDynamicSigns(dynamicSignNames.ToList());
                        }
                        else
                        {
                            LoggerUtils.LogWarning("Couldn't load routes, as the array is null!");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerUtils.LogException(ex);

                    }
                    finally
                    {
                        dynamicSignMemoryStream.Close();
                    }
                }
                else
                {
                    LoggerUtils.LogWarning("Found no data to load");
                }
            }
            lock (thisLock)
            {
                byte[] loadedOverrideSignData = serializableDataManager.LoadData(overrideSignDataKey);
                LoggerUtils.Log("Override signs loaded, parsing data");
                if (loadedOverrideSignData != null && loadedOverrideSignData.Length > 0)
                {
                    MemoryStream overrideSignMemoryStream = new MemoryStream(loadedOverrideSignData);

                    try
                    {
                        OverrideSignContainer[] overrideSignNames = binaryFormatter.Deserialize(overrideSignMemoryStream) as OverrideSignContainer[];

                        if (overrideSignNames != null && overrideSignNames.Length > 0)
                        {
                            instance.LoadOverrideSigns(overrideSignNames.ToList());
                        }
                        else
                        {
                            LoggerUtils.LogWarning("Couldn't load routes, as the array is null!");
                        }
                    }
                    catch (Exception ex)
                    {
                        LoggerUtils.LogException(ex);

                    }
                    finally
                    {
                        overrideSignMemoryStream.Close();
                    }
                }
                else
                {
                    LoggerUtils.LogWarning("Found no data to load");
                }
            }
             
        }
    }
}

