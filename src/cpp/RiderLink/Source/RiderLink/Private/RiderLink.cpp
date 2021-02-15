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
  JetBrains::EditorPlugin::RdEditorModel& UnrealToBackendModel = RdConnection->UnrealToBackendModel;
  Scheduler.queue([&UnrealToBackendModel, Action]()
  {
    Action(UnrealToBackendModel);
  });
}

void FRiderLinkModule::InitConnection()
{
  UE_LOG(FLogRiderLinkModule, Log, TEXT("Connection initialized"));
  ConnectionLifetimeDefPtr = MakeUnique<rd::LifetimeDefinition>(ModuleLifetime);
  ConnectionLifetimeDefPtr->lifetime->add_action([this]()
  {
    UE_LOG(FLogRiderLinkModule, Log, TEXT("Connection stopped"));
    if(!ModuleLifetimeDef.is_terminated())
      InitConnection();
  });
  RdConnection = MakeUnique<class RdConnection>();
  RdConnection->Init(&Scheduler, *ConnectionLifetimeDefPtr.Get());
}

void FRiderLinkModule::StartupModule()
{
  UE_LOG(FLogRiderLinkModule, Log, TEXT("STARTUP START"));
  InitConnection();
  UE_LOG(FLogRiderLinkModule, Log, TEXT("STARTUP FINISH"));
}

#undef LOCTEXT_NAMESPACE
