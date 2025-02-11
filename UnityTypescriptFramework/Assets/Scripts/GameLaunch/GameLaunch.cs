﻿using System.Collections;
using AssetBundles;
using CS.Framework.Conf;
using GameChannel;
using UnityEngine;

namespace CS
{
    /// <summary>
    /// 游戏全局入口
    /// </summary>
    public class GameLaunch : MonoBehaviour
    {
        //调试模式开关
        [SerializeField] private JsDebugger jsDebugger;

        //启动相关用到的prefab，不使用js代码运行，本次热更下次生效的类型
        private const string launchPrefabPath = "ui/prefabs/view/launch/ui_launch.prefab";
        private const string noticeTipPrefabPath = "ui/prefabs/view/launch/ui_notice_tip.prefab";

        //ab更新器
        private AssetbundleUpdater _updater;

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns></returns>
        IEnumerator Start()
        {
            //帧率设置
            Application.targetFrameRate = 60;
            //js调试器模式
            Config.JsDebugger = jsDebugger;

            LoggerHelper.Instance.Startup();

            //初始版本号
            yield return InitAppVersion();

            //初始平台
            yield return InitChannel();

            //ab管理器启动
            yield return AssetBundleManager.Instance.Initialize();

            yield return InitLaunchPrefab();
            yield return null;
            yield return InitNoticePrefab();

            //开始更新下载
            if (_updater)
            {
                _updater.StartCheckUpdate();
            }
            yield break;
        }

        //初始化app版本
        IEnumerator InitAppVersion()
        {
            var appVersionRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.AppVersionFileName);
            yield return appVersionRequest;
            var streamingAppVersion = appVersionRequest.text;
            appVersionRequest.Dispose();

            var appVersionPath = AssetBundleUtility.GetPersistentDataPath(BuildUtils.AppVersionFileName);
            var persistentAppVersion = GameUtility.SafeReadAllText(appVersionPath);
            Logger.Log($"streamingAppVersion = {streamingAppVersion}, persistentAppVersion = {persistentAppVersion}");
            // 如果persistent目录版本比streamingAssets目录app版本低，说明是大版本覆盖安装，清理过时的缓存
            if (!string.IsNullOrEmpty(persistentAppVersion) &&
                BuildUtils.CheckIsNewVersion(persistentAppVersion, streamingAppVersion))
            {
                var path = AssetBundleUtility.GetPersistentDataPath();
                GameUtility.SafeDeleteDir(path);
            }

            GameUtility.SafeWriteAllText(appVersionPath, streamingAppVersion);
            ChannelManager.instance.appVersion = streamingAppVersion;
            yield break;
        }

        //初始渠道
        IEnumerator InitChannel()
        {
#if UNITY_EDITOR
            if (AssetBundleConfig.IsEditorMode)
            {
                yield break;
            }
#endif
            var channelNameRequest = AssetBundleManager.Instance.RequestAssetFileAsync(BuildUtils.ChannelNameFileName);
            yield return channelNameRequest;
            var channelName = channelNameRequest.text;
            channelNameRequest.Dispose();
            ChannelManager.instance.Init(channelName);
            Logger.Log($"channelName = {channelName}");
            yield break;
        }

        //初始提示界面
        IEnumerator InitNoticePrefab()
        {
            var loader = AssetBundleManager.Instance.LoadAssetAsync(noticeTipPrefabPath, typeof(GameObject));
            yield return loader;
            var prefab = loader.asset as GameObject;
            loader.Dispose();
            if (prefab == null)
            {
                Logger.LogError($"LoadAssetAsync noticeTipPrefab err :{noticeTipPrefabPath} ");
                yield break;
            }

            var go = InstantiatePrefab(prefab);
            UINoticeTip.Instance.UIGameObject = go;
            yield break;
        }

        //初始启动界面
        IEnumerator InitLaunchPrefab()
        {
            var loader = AssetBundleManager.Instance.LoadAssetAsync(launchPrefabPath, typeof(GameObject));
            yield return loader;
            var prefab = loader.asset as GameObject;
            loader.Dispose();
            if (prefab == null)
            {
                Logger.LogError($"LoadAssetAsync launchPrefab err:{launchPrefabPath}");
                yield break;
            }

            var launcher = InstantiatePrefab(prefab);
            _updater = launcher.GetComponent<AssetbundleUpdater>();
            if (_updater == null)
            {
                _updater = launcher.AddComponent<AssetbundleUpdater>();
            }

            yield break;
        }

        //通过prefab初始一个GO
        private GameObject InstantiatePrefab(GameObject prefab)
        {
            var instanceGO = Instantiate(prefab);
            var layer = GameObject.Find("UIRoot/LaunchLayer");
            instanceGO.transform.SetParent(layer.transform);
            var rt = instanceGO.GetComponent<RectTransform>();
            rt.offsetMax = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.localPosition = Vector3.zero;
            return instanceGO;
        }
    }
}