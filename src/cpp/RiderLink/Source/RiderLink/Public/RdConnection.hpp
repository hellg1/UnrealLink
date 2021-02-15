// Copyright 1998-2020 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "base/IProtocol.h"
#include "scheduler/SingleThreadScheduler.h"
#include "RdEditorProtocol/RdEditorModel/RdEditorModel.Generated.h"

#include "Templates/UniquePtr.h"

class RdConnection
{
public:
	RdConnection() = default;
	~RdConnection() = default;

	void Init(rd::SingleThreadScheduler* Scheduler, rd::LifetimeDefinition& SocketLifetimeDef);

	JetBrains::EditorPlugin::RdEditorModel UnrealToBackendModel;

private:
	TUniquePtr<rd::LifetimeDefinition> Definition;
	TUniquePtr<rd::IProtocol> Protocol;
};
