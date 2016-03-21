using ICities;
using MarkARoute.Managers;
using MarkARoute.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace MarkARoute
{
    public class MarkARouteSerializer : SerializableDataExtensionBase
    {
        private readonly string routeDataKey = "MarkARoute";

        public override void OnSaveData()
        {
            LoggerUtils.Log("Saving routes");

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            MemoryStream routeMemoryStream = new MemoryStream();

            try
            {
                RouteContainer[] routeNames = RouteManager.Instance().SaveRoutes();
                if (routeNames != null)
                {
                    binaryFormatter.Serialize(routeMemoryStream, routeNames);
                    serializableDataManager.SaveData(routeDataKey, routeMemoryStream.ToArray());
                    LoggerUtils.Log("Routes have been saved!");
                }
                else
                {
                    LoggerUtils.LogWarning("Couldn't save routes, as the array is null!");
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
            LoggerUtils.Log("Loading routes");

            byte[] loadedRouteData = serializableDataManager.LoadData(routeDataKey);

            if (loadedRouteData != null)
            {
                MemoryStream routeMemoryStream = new MemoryStream();

                routeMemoryStream.Write(loadedRouteData, 0, loadedRouteData.Length);
                routeMemoryStream.Position = 0;

                BinaryFormatter binaryFormatter = new BinaryFormatter();

                try
                {
                    RouteContainer[] routeNames = binaryFormatter.Deserialize(routeMemoryStream) as RouteContainer[];

                    if (routeNames != null)
                    {
                        RouteManager.Instance().Load(routeNames);
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
                    routeMemoryStream.Close();
                }
            }
            else
            {
                LoggerUtils.LogWarning("Found no data to load");
            }
        }
    }
}

