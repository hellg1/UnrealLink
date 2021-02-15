// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "RdConnection.hpp"

#include "Logging/LogMacros.h"
#include "Logging/LogVerbosity.h"
#include "Modules/ModuleInterface.h"
#include "Modules/ModuleManager.h"

DECLARE_LOG_CATEGORY_EXTERN(FLogRiderLinkModule, Log, All);

DECLARE_DELEGATE(FConnectionEstablishedDelegate);

class RIDERLINK_API FRiderLinkModule : public IModuleInterface
{
public:
	FRiderLinkModule() = default;
	~FRiderLinkModule() = default;

	static FRiderLinkModule& Get()
	{		
		return FModuleManager::GetModuleChecked<FRiderLinkModule>(GetModuleName());
	}

	static FName GetModuleName()
	{
		static const FName ModuleName = TEXT("RiderLink");
		return ModuleName;
	}

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	virtual bool SupportsDynamicReloading() override { return true; }

	void QueueModelAction(std::function<void(JetBrains::EditorPlugin::RdEditorModel&)> Action);

	rd::Lifetime CreateSocketNestedLifetime() const { return Scheduler.lifetime.create_nested(); }

	FConnectionEstablishedDelegate ConnectionEstablishedDelegate;

private:

	void InitConnection();
	
	rd::LifetimeDefinition ModuleLifetimeDef{};
	rd::Lifetime ModuleLifetime{ModuleLifetimeDef.lifetime};
	rd::SingleThreadScheduler Scheduler{ModuleLifetime, "UnrealEditorScheduler"};

	
	TUniquePtr<rd::LifetimeDefinition> ConnectionLifetimeDefPtr;
	TUniquePtr<RdConnection> RdConnection;
};
