using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace AttachToDockerContainer
{
    public partial class AttachToDockerContainerDialog : DialogWindow
    {
        public ContainerToAttachInfo ContainerToAttachInfo { get; set; }

        private const string VsDbgDefaultPath = "/vsdbg/vsdbg";

        private readonly IServiceProvider _serviceProvider;

        public AttachToDockerContainerDialog(IServiceProvider serviceProvider)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _serviceProvider = serviceProvider;

            InitializeComponent();

            var containerNames = GetContainerNames();

            var (previousContainer, previousVsDbgPath) = GetSettings();

            this.ContainerToAttachInfo = new ContainerToAttachInfo();

            this.ContainerToAttachInfo.PropertyChanged += ContainerToAttachInfo_PropertyChanged;
            this.ContainerToAttachInfo.VSDBGPath = previousVsDbgPath ?? VsDbgDefaultPath; 
            this.ContainerToAttachInfo.ContainerNames = containerNames;
            this.ContainerToAttachInfo.ContainerName =  containerNames.Contains(previousContainer)
                                                            ? previousContainer
                                                            : containerNames.FirstOrDefault();
        }

        private void ContainerToAttachInfo_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ContainerName")
            {
                if (this.ContainerToAttachInfo.ContainerName != default(string))
                {
                    this.ContainerToAttachInfo.ProcessIds = GetProcessIdsFromContainer(this.ContainerToAttachInfo.ContainerName);
                }
            }
        }

        private void AttachButton_Click(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            SetSettings(this.ContainerToAttachInfo.ContainerName, 
                        this.ContainerToAttachInfo.VSDBGPath);

            DebugAdapterHostLauncher.Instance.Launch(this.ContainerToAttachInfo.ContainerName,
                                                     this.ContainerToAttachInfo.ProcessId,
                                                     this.ContainerToAttachInfo.VSDBGPath);

            Close();
        }

        private string[] GetContainerNames()
        {
            var output = DockerCli.Execute("ps --format \"{{.Names}}\"");

            return output
                .Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .OrderBy(n => n)
                .ToArray();
        }

        private string[] GetProcessIdsFromContainer(string containerName)
        {
            var dotnetPids = DockerCli.Execute($"exec -it {containerName} pidof dotnet");

            return dotnetPids
                        .Split(new[] { "\r\n", "\r", "\n", " " }, StringSplitOptions.RemoveEmptyEntries)
                            .OrderBy(n => n)
                                .ToArray();
        }

        private (string container, string vsDbg) GetSettings()
        {
            const string collectionPath = nameof(AttachToDockerContainerDialog);

            ThreadHelper.ThrowIfNotOnUIThread();

            SettingsStore.CollectionExists(collectionPath, out int exists);

            if (exists != 1)
            {
                SettingsStore.CreateCollection(collectionPath);
            }

            SettingsStore.GetString(collectionPath, "container", out string container);
            SettingsStore.GetString(collectionPath, "vsdbg", out string vsdbg);

            return (container, vsdbg);
        }

        private void SetSettings(string containerName, string vsdbgPath)
        {
            const string collectionPath = nameof(AttachToDockerContainerDialog);

            ThreadHelper.ThrowIfNotOnUIThread();

            SettingsStore.CollectionExists(collectionPath, out int exists);
            if (exists != 1)
            {
                SettingsStore.CreateCollection(collectionPath);
            }

            SettingsStore.SetString(collectionPath, "containerName", containerName);
            SettingsStore.SetString(collectionPath, "vsdbgPath", vsdbgPath);
        }

        private IVsWritableSettingsStore _settingsStore = null;

        private IVsWritableSettingsStore SettingsStore
        {
            get
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                if (_settingsStore == null)
                {
                    var settingsManager = (IVsSettingsManager)_serviceProvider.GetService(typeof(SVsSettingsManager));

                    // Write the user settings to _settingsStore.
                    settingsManager.GetWritableSettingsStore(
                        (uint)__VsSettingsScope.SettingsScope_UserSettings,
                        out _settingsStore);
                }
                return _settingsStore;
            }
        }
    }
}
