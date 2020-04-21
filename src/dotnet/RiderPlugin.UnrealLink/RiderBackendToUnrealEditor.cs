using System;
using System.IO;
using System.Linq;
using JetBrains.Application.Threading;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ProjectModel;
using JetBrains.Rd;
using JetBrains.Rd.Base;
using JetBrains.Rd.Impl;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Features.XamlRendererHost.Preview;
using JetBrains.Rider.Model;
using JetBrains.Unreal.Lib;
using JetBrains.Util;
using ReSharperPlugin.UnrealEditor;
using RiderPlugin.UnrealLink.PluginInstaller;

namespace RiderPlugin.UnrealLink
{
    [SolutionComponent]
    public class RiderBackendToUnrealEditor
    {
        private readonly IScheduler myDispatcher;
        private readonly ILogger myLogger;
        private readonly UnrealToolWindowHost myToolWindowHost;
        private readonly UnrealHost myUnrealHost;
        private readonly ViewableProperty<RdEditorModel> myEditorModel = new ViewableProperty<RdEditorModel>(null);

        private bool PlayedFromUnreal = false;
        private bool PlayedFromRider = false;
        private bool PlayModeFromUnreal = false;
        private bool PlayModeFromRider = false;
        private Lifetime myComponentLifetime;
        private readonly IShellLocks myLocks;
        private SequentialLifetimes myConnectionLifetimeProducer;

        public RiderBackendToUnrealEditor(Lifetime lifetime, IShellLocks locks, IScheduler dispatcher, ILogger logger,
            UnrealHost unrealHost,
            UnrealPluginDetector pluginDetector, UnrealToolWindowHost toolWindowHost)
        {
            myComponentLifetime = lifetime;
            myLocks = locks;
            myConnectionLifetimeProducer = new SequentialLifetimes(lifetime);
            myDispatcher = dispatcher;
            myLogger = logger;
            myUnrealHost = unrealHost;
            myToolWindowHost = toolWindowHost;

            myLogger.Info("RiderBackendToUnrealEditor building started");

            pluginDetector.InstallInfoProperty.View(myComponentLifetime, (lt, pluginInfo) =>
            {
                if (pluginInfo == null) return;

                var portDirectoryFullPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..",
                    "Local", "Jetbrains", "Rider", "Unreal", "Ports");

                Directory.CreateDirectory(portDirectoryFullPath);

                var watcher = new FileSystemWatcher(portDirectoryFullPath)
                {
                    NotifyFilter = NotifyFilters.LastWrite
                };
                var projects = pluginInfo.ProjectPlugins.Select(it => it.UprojectFilePath.NameWithoutExtension)
                    .ToList();

                FileSystemEventHandler handler = (obj, fileSystemEvent) =>
                {
                    var path = FileSystemPath.Parse(fileSystemEvent.FullPath);
                    if (projects.Contains(path.NameWithoutExtension) && myComponentLifetime.IsAlive)
                    {
                        myLocks.ExecuteOrQueue(myComponentLifetime, "CreateProtocol", () => CreateProtocols(path));
                    }
                };

                watcher.Changed += handler;
                watcher.Created += handler;

                // Check if it's even possible to happen
                lt.Bracket(() => { }, () => { watcher.Dispose(); });

                StartWatcher(watcher);

                foreach (var projectName in projects)
                {
                    var portFileFullPath = Path.Combine(portDirectoryFullPath, projectName);
                    CreateProtocols(FileSystemPath.Parse(portFileFullPath));
                }
            });

            myLogger.Info("RiderBackendToUnrealEditor building finished");
        }

        private static void StartWatcher(FileSystemWatcher watcher)
        {
            watcher.EnableRaisingEvents = true;
        }

        private void CreateProtocols(FileSystemPath portFileFullPath)
        {
            if (!portFileFullPath.ExistsFile) return;

            var text = File.ReadAllText(portFileFullPath.FullPath);
            if (!int.TryParse(text, out var port))
            {
                myLogger.Error("Couldn't parse port for from file:{0}, text:{1}", portFileFullPath, text);
                return;
            }

            var modelLifetime = myConnectionLifetimeProducer.Next();

            myLogger.Info("Creating SocketWire with port = {0}", port);
            var wire = new SocketWire.Client(modelLifetime, myDispatcher, port, "UnrealEditorClient");
            wire.Connected.Advise(modelLifetime, isConnected => myUnrealHost.PerformModelAction(riderModel =>
                riderModel.IsConnectedToUnrealEditor.SetValue(isConnected)));

            var protocol = new Protocol("UnrealEditorPlugin", new Serializers(),
                new Identities(IdKind.Client), myDispatcher, wire, modelLifetime);

            wire.Connected.WhenTrue(modelLifetime, lifetime =>
            {
                myLogger.Info("Wire connected");
                ResetModel(lifetime, protocol);
            });
        }

        private void OnMessageReceived(UnrealLogEvent message, UnrealTabModel unrealTabModel,
            RdEditorModel unrealModel, RdRiderModel riderModel)
        {
            unrealTabModel.UnrealPane.UnrealLog(message);
            myToolWindowHost.Highlight(message, unrealTabModel, unrealModel);
        }

        private void ResetModel(Lifetime lf, IProtocol protocol)
        {
            myUnrealHost.PerformModelAction(riderModel =>
            {
                UE4Library.RegisterDeclaredTypesSerializers(riderModel.SerializationContext.Serializers);
                riderModel.EditorId.SetValue(riderModel.EditorId.Value + 1);
            });

            var unrealModel = new RdEditorModel(lf, protocol);
            UE4Library.RegisterDeclaredTypesSerializers(unrealModel.SerializationContext.Serializers);

            var unrealTabModel = myToolWindowHost.AddTab("Default", unrealModel);
            
            unrealModel.UnrealLog.Advise(lf,
                logEvent => myUnrealHost.PerformModelAction(riderModel =>
                    OnMessageReceived(logEvent, unrealTabModel, unrealModel, riderModel)));
            
            unrealModel.AllowSetForegroundWindow.Set((lt, pid) =>
            {
                return myUnrealHost.PerformModelAction(riderModel =>
                    riderModel.AllowSetForegroundWindow.Start(lt, pid)) as RdTask<bool>;
            });


            unrealModel.Play.Advise(lf, val =>
            {
                myUnrealHost.PerformModelAction(riderModel =>
                {
                    if (PlayedFromRider)
                        return;
                    try
                    {
                        PlayedFromUnreal = true;
                        riderModel.Play.Set(val);
                    }
                    finally
                    {
                        PlayedFromUnreal = false;
                    }
                });
            });
            unrealModel.PlayMode.Advise(lf, val =>
            {
                myUnrealHost.PerformModelAction(riderModel =>
                {
                    if (PlayModeFromRider)
                        return;
                    try
                    {
                        PlayModeFromUnreal = true;
                        riderModel.PlayMode.Set(val);
                    }
                    finally
                    {
                        PlayModeFromUnreal = false;
                    }
                });
            });

            myUnrealHost.PerformModelAction(riderModel =>
            {
                riderModel.Play.Advise(lf, val =>
                {
                    if (PlayedFromUnreal)
                        return;
                    try
                    {
                        PlayedFromRider = true;
                        unrealModel.Play.Set(val);
                    }
                    finally
                    {
                        PlayedFromRider = false;
                    }
                });

                riderModel.PlayMode.Advise(lf, val =>
                {
                    if (PlayModeFromUnreal)
                        return;
                    try
                    {
                        PlayModeFromRider = true;
                        unrealModel.PlayMode.Set(val);
                    }
                    finally
                    {
                        PlayModeFromRider = false;
                    }
                });
                riderModel.FrameSkip.Advise(lf, skip =>
                    unrealModel.FrameSkip.Fire(skip));
            });

            if (myComponentLifetime.IsAlive)
                myLocks.ExecuteOrQueueEx(myComponentLifetime, "setModel",
                    () => { myEditorModel.SetValue(unrealModel); });
        }

        public RdEditorModel GetCurrentEditorModel()
        {
            return myEditorModel.Value;
        }
    }
}