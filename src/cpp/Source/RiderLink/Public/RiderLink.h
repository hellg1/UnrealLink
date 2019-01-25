// Copyright 1998-2018 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"

#include "Windows/AllowWindowsPlatformTypes.h"

//The external headers and defines goes here
#include "RdConnection.hpp"
#include "RiderOutputDevice.hpp"

#include "Windows/HideWindowsPlatformTypes.h"

#include "Modules/ModuleInterface.h"
#include "RiderSourceCodeAccessor.h"


DECLARE_LOG_CATEGORY_EXTERN(FLogRiderLinkModule, Log, All);


class FRiderLinkModule : public IModuleInterface
{
public:
	FRiderLinkModule();
	~FRiderLinkModule();

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	virtual bool SupportsDynamicReloading() override;

	FRiderSourceCodeAccessor& GetAccessor();

private:
	/** Handle to the test dll we will load */
	RdConnection rdConnection;
	FRiderOutputDevice outputDevice;
	FRiderSourceCodeAccessor RiderSourceCodeAccessor;
};
