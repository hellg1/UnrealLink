// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

#include "RiderLink.hpp"

#include "Modules/ModuleManager.h"
#include "HAL/Platform.h"

#define LOCTEXT_NAMESPACE "RiderLink"

DEFINE_LOG_CATEGORY(FLogRiderLinkModule);

IMPLEMENT_MODULE(FRiderLinkModule, RiderLink);

void FRiderLinkModule::ShutdownModule()
{
  UE_LOG(FLogRiderLinkModule, Verbose, TEXT("SHUTDOWN START"));
  ModuleLifetimeDef.terminate();
  UE_LOG(FLogRiderLinkModule, Verbose, TEXT("SHUTDOWN FINISH"));
}

void FRiderLinkModule::QueueModelAction(std::function<void(JetBrains::EditorPlugin::RdEditorModel&)> Action)
{
  JetBrains::EditorPlugin::RdEditorModel& UnrealToBackendModel = RdConnection.UnrealToBackendModel;
  Scheduler.queue([&UnrealToBackendModel, Action]()
  {
    Action(UnrealToBackendModel);
  });
}

FRiderLinkModule::FRiderLinkModule():
  ModuleLifetimeDef(rd::Lifetime::Eternal()),
  ModuleLifetime(ModuleLifetimeDef.lifetime),
  Scheduler{ModuleLifetime, "UnrealEditorScheduler"}
{
}

void FRiderLinkModule::StartupModule()
{
  UE_LOG(FLogRiderLinkModule, Verbose, TEXT("STARTUP START"));
  RdConnection.Init(&Scheduler, ModuleLifetime);
  UE_LOG(FLogRiderLinkModule, Verbose, TEXT("STARTUP FINISH"));
}

#undef LOCTEXT_NAMESPACE
