using System;
using Unisave.Facets;
using Unisave.Foundation;
using UnityEditor.SceneManagement;
using UnityEngine.UIElements;
using Application = UnityEngine.Application;

namespace Unisave.Editor.Windows.Main.Tabs
{
    public class HomeTabController : ITabContentController
    {
        public Action<TabTaint> SetTaint { get; set; }
        
        private readonly VisualElement root;

        private VisualElement checklistItemRegister;
        private VisualElement checklistItemConnect;

        private Label versionsLabel;

        private DeviceIdRepository deviceIdRepository = new DeviceIdRepository();

        public HomeTabController(VisualElement root)
        {
            this.root = root;
        }

        public void OnCreateGUI()
        {
            // === Quick link buttons ===
            
            root.Q<Button>(name: "link-guides").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/guides?" + AssetMeta.LinkUtmParams
                );
            };
            root.Q<Button>(name: "link-documentation").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/docs?" + AssetMeta.LinkUtmParams
                );
            };
            root.Q<Button>(name: "link-discord").clicked += () => {
                Application.OpenURL("https://discord.gg/XV696Tp");
            };
            root.Q<Button>(name: "link-pricing").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/pricing?" + AssetMeta.LinkUtmParams
                );
            };
            
            // === Checklist items ===

            checklistItemRegister = root.Q(name: "ci-register");
            checklistItemConnect = root.Q(name: "ci-connect");
            
            // === Checklist buttons ===
            
            root.Q<Button>(name: "cb-register").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/register?" + AssetMeta.LinkUtmParams
                );
            };
            root.Q<Button>(name: "cb-connection-tab").clicked += () => {
                UnisaveMainWindow.ShowTab(MainWindowTab.Connection);
            };
            root.Q<Button>(name: "cb-ex-chat").clicked += () => {
                EditorSceneManager.OpenScene(
                    "Assets/Plugins/Unisave/Examples/Chat/Chat.unity"
                );
            };
            root.Q<Button>(name: "cb-ex-email-auth").clicked += () => {
                EditorSceneManager.OpenScene(
                    "Assets/Plugins/Unisave/Examples/LoginViaEmail/" +
                    "LoginViaEmail.unity"
                );
            };
            root.Q<Button>(name: "cb-storing-player-data").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/guides/" +
                    "how-to-store-player-data-online-with-unity?"
                    + AssetMeta.LinkUtmParams
                );
            };
            root.Q<Button>(name: "cb-sending-emails").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/docs/mail?" + AssetMeta.LinkUtmParams
                );
            };
            root.Q<Button>(name: "cb-steam-microtransactions").clicked += () => {
                Application.OpenURL(
                    "https://unisave.cloud/docs/steam-microtransactions?"
                        + AssetMeta.LinkUtmParams
                );
            };
            
            root.Q<Button>(name: "cb-discord").clicked += () => {
                Application.OpenURL("https://discord.gg/XV696Tp");
            };
            root.Q<Button>(name: "cb-email").clicked += () => {
                Application.OpenURL("mailto:info@unisave.cloud");
            };
            root.Q<Button>(name: "cb-review").clicked += () => {
                Application.OpenURL(
                    "https://assetstore.unity.com/packages/tools/network/" +
                    "unisave-backend-server-142705#reviews"
                );
            };
            
            // === Other ===

            versionsLabel = root.Q<Label>(name: "versions-label");
        }

        public void OnObserveExternalState()
        {
            var preferences = UnisavePreferences.Resolve();
            
            bool notConnected = string.IsNullOrWhiteSpace(preferences.GameToken) ||
                string.IsNullOrWhiteSpace(preferences.EditorKey);
            
            checklistItemRegister.EnableInClassList(
                "checklist-item--hidden", enable: !notConnected
            );
            checklistItemConnect.EnableInClassList(
                "checklist-item--hidden", enable: !notConnected
            );

            versionsLabel.text = $"Unisave Asset: {AssetMeta.Version}\n" +
                                 $"Unisave Framework: {FrameworkMeta.Version}\n" +
                                 $"Device ID: {deviceIdRepository.GetDeviceId()}";
        }

        public void OnWriteExternalState()
        {
            // nothing
        }
    }
}