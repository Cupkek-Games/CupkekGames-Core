#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace CupkekGames.Core.Editor
{
    internal static class CupkekGamesPackageInstaller
    {
        private const string LastErrorKey = "CupkekGames_PackageInstallLastError";

        private static ListRequest _listRequest;
        private static AddRequest _addRequest;
        private static AddAndRemoveRequest _addAndRemoveRequest;
        private static Action<Dictionary<string, string>> _listCallback;
        private static Action<bool, string> _addCallback;
        private static Action<bool, string> _addAndRemoveCallback;

        public static bool IsAddInFlight => _addRequest != null || _addAndRemoveRequest != null;
        public static bool IsListInFlight => _listRequest != null;

        public static string LastError
        {
            get => EditorPrefs.GetString(LastErrorKey, string.Empty);
            private set
            {
                if (string.IsNullOrEmpty(value)) EditorPrefs.DeleteKey(LastErrorKey);
                else EditorPrefs.SetString(LastErrorKey, value);
            }
        }

        public static void GetInstalledPackages(Action<Dictionary<string, string>> onCompleted)
        {
            if (_listRequest != null) return;
            _listRequest = Client.List(offlineMode: true, includeIndirectDependencies: false);
            _listCallback = onCompleted;
            EditorApplication.update += PumpListRequest;
        }

        private static void PumpListRequest()
        {
            if (_listRequest == null || !_listRequest.IsCompleted) return;
            EditorApplication.update -= PumpListRequest;

            var dict = new Dictionary<string, string>();
            if (_listRequest.Status == StatusCode.Success)
            {
                foreach (var pkg in _listRequest.Result)
                {
                    dict[pkg.name] = pkg.version;
                }
            }

            Action<Dictionary<string, string>> cb = _listCallback;
            _listRequest = null;
            _listCallback = null;
            cb?.Invoke(dict);
        }

        public static void InstallByGitUrl(string gitUrl, Action<bool, string> onCompleted)
        {
            if (IsAddInFlight) return;
            LastError = string.Empty;
            _addRequest = Client.Add(gitUrl);
            _addCallback = onCompleted;
            EditorApplication.update += PumpAddRequest;
        }

        private static void PumpAddRequest()
        {
            if (_addRequest == null || !_addRequest.IsCompleted) return;
            EditorApplication.update -= PumpAddRequest;

            bool ok = _addRequest.Status == StatusCode.Success;
            string msg = ok
                ? (_addRequest.Result != null ? _addRequest.Result.name : string.Empty)
                : (_addRequest.Error != null ? _addRequest.Error.message : "Unknown error");

            if (!ok) LastError = msg;

            Action<bool, string> cb = _addCallback;
            _addRequest = null;
            _addCallback = null;
            cb?.Invoke(ok, msg);
        }

        // Bulk-install via Client.AddAndRemove — single manifest write + single domain reload.
        // Available in Unity 2021.2+ (we require 6000.0).
        public static void InstallAllByGitUrls(IEnumerable<string> gitUrls, Action<bool, string> onCompleted = null)
        {
            if (IsAddInFlight) return;

            string[] toAdd = gitUrls
                .Where(u => !string.IsNullOrEmpty(u))
                .ToArray();
            if (toAdd.Length == 0)
            {
                onCompleted?.Invoke(true, "No packages to install.");
                return;
            }

            LastError = string.Empty;
            _addAndRemoveRequest = Client.AddAndRemove(packagesToAdd: toAdd, packagesToRemove: null);
            _addAndRemoveCallback = onCompleted;
            EditorApplication.update += PumpAddAndRemoveRequest;
        }

        private static void PumpAddAndRemoveRequest()
        {
            if (_addAndRemoveRequest == null || !_addAndRemoveRequest.IsCompleted) return;
            EditorApplication.update -= PumpAddAndRemoveRequest;

            bool ok = _addAndRemoveRequest.Status == StatusCode.Success;
            string msg = ok
                ? $"Installed {_addAndRemoveRequest.Result?.Count() ?? 0} package(s)."
                : (_addAndRemoveRequest.Error != null ? _addAndRemoveRequest.Error.message : "Unknown error");

            if (!ok) LastError = msg;

            Action<bool, string> cb = _addAndRemoveCallback;
            _addAndRemoveRequest = null;
            _addAndRemoveCallback = null;
            cb?.Invoke(ok, msg);
        }
    }
}
#endif
