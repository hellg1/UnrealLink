using System;
using System.IO;
using JetBrains.Collections.Viewable;
using JetBrains.DataFlow;
using JetBrains.Diagnostics;
using JetBrains.Lifetimes;
using JetBrains.Platform.Unreal.EditorPluginModel;
using JetBrains.ProjectModel;
using JetBrains.Rd;
using JetBrains.Rd.Impl;
using JetBrains.Rd.Tasks;
using JetBrains.ReSharper.Features.XamlRendererHost.Preview;
using JetBrains.Unreal.Lib;
using JetBrains.Util;

namespace ReSharperPlugin.UnrealEditor
{
    [SolutionComponent]
    public class RiderBackendToUnrealEditor
    {
        private readonly IScheduler myDispatcher;
        private readonly ISolution mySolution;
        private readonly ILogger myLogger;
        private readonly UnrealHost myUnrealHost;
        private readonly IProperty<RdEditorModel> myEditorModel;

        private const string PortFileName = "UnrealProtocolPort.txt";
        private const string ClosedFileExtension = ".closed";

        public RiderBackendToUnrealEditor(Lifetime lifetime, IScheduler dispatcher, ISolution solution, ILogger logger,
            UnrealHost unrealHost)
        {
            myLogger.Info("RiderBackendToUnrealEditor building started");
            System.Diagnostics.Debugger.Launch();
            myDispatcher = dispatcher;
            mySolution = solution;
            myLogger = logger;
            myUnrealHost = unrealHost;
            var modelLifetimeDefinition = lifetime.CreateNested();
            var modelLifetime = modelLifetimeDefinition.Lifetime;
            myEditorModel = new Property<RdEditorModel>(modelLifetime, "RiderTounrealModel");


            var projectName = mySolution.Name;
            var portDirectoryFullPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "..",
                "Local", "Jetbrains", "Rider", "Unreal", projectName, "Ports");


            var watcher = new FileSystemWatcher(portDirectoryFullPath) {Filter = $"*{ClosedFileExtension}"};

            OnCreated(watcher, modelLifetime, projectName, portDirectoryFullPath);

            OnDeleted(lifetime, watcher, modelLifetimeDefinition);

            StartWatcher(watcher);

            myLogger.Info("RiderBackendToUnrealEditor building finished");
        }

        private static void StartWatcher(FileSystemWatcher watcher)
        {
            watcher.EnableRaisingEvents = true;
        }

        private void OnDeleted(Lifetime lifetime, FileSystemWatcher watcher,
            LifetimeDefinition modelLifetimeDefinition)
        {
            watcher.Deleted += (sender, args) =>
            {
                myLogger.Info("File with port's deleted");

                //modelLifetimeDefinition.Terminate();
                //modelLifetimeDefinition = lifetime.CreateNested();
            };
        }

        private void OnCreated(FileSystemWatcher watcher, Lifetime modelLifetime, string projectName,
            string portDirectoryFullPath)
        {
            var portFileFullPath = Path.Combine(portDirectoryFullPath, PortFileName);

            var portFileClosedPath = Path.Combine(portDirectoryFullPath, $"{PortFileName}{ClosedFileExtension}");
            Directory.CreateDirectory(portDirectoryFullPath);
            FileSystemEventHandler handler = (_, e) =>
            {
                Assertion.Assert(portFileClosedPath.Equals(e.FullPath), "Invalid event received from watcher");
                myLogger.Info("File with port's created");

                var text = File.ReadAllText(portFileFullPath);
                if (!int.TryParse(text, out var port))
                {
                    myLogger.Error("Couldn't parse port for from file:{0}, text:{1}", portFileFullPath, text);
                    return;
                }

                var wire = new SocketWire.Client(modelLifetime, myDispatcher, port, "UnrealEditorClient");

                //todo think about alive file from previous session

                wire.Connected.WhenTrue(modelLifetime, lf =>
                {
                    myLogger.Info("WireConnected");
                    var serializers = new Serializers();
                    var identities = new Identities(IdKind.Client);
                    var protocol = new Protocol($"UnrealRiderClient-{projectName}", serializers, identities,
                        myDispatcher, wire, modelLifetime);

                    ResetModel(lf, protocol);
                });
            };
            watcher.Created += handler;
            watcher.Changed += handler;
        }

        private void ResetModel(Lifetime lf, IProtocol protocol)
        {
            myEditorModel.SetValue(lf, new RdEditorModel(lf, protocol));
            myEditorModel.View(lf,
                (lf2, model) =>
                {
                    model.UnrealLog.Advise(lf, s =>
                    {
                        myUnrealHost.PerformModelAction(m =>
                            m.UnrealLog.Fire(s));
                    });
                    myUnrealHost.PerformModelAction(riderModel =>
                    {
                        riderModel.IsBlueprint.Set((lifetime, s) =>
                            model.IsBlueprint.Start(s) as RdTask<bool>);
                    });
                });
        }

        public RdEditorModel GetCurrentEditorModel()
        {
            return myEditorModel.Value;
        }
    }
}