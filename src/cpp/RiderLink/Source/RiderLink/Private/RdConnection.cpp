#include "RdConnection.hpp"

#include "ProtocolFactory.h"
#include "Model/Library/UE4Library/UE4Library.Generated.h"

void RdConnection::Init(rd::SingleThreadScheduler * Scheduler, rd::LifetimeDefinition& SocketLifetimeDef)
{
    Protocol = ProtocolFactory::Create(Scheduler, SocketLifetimeDef.lifetime);
    Scheduler->queue([this, &SocketLifetimeDef]()
    {
        Protocol->wire->connected.view(SocketLifetimeDef.lifetime, [&SocketLifetimeDef] (rd::Lifetime Lifetime, bool const& Cond)
        {
            UE_LOG(LogTemp, Log, TEXT("RdConnection::Connection status changed %b"), Cond);
           if(Cond)
           {
               Lifetime->add_action([&SocketLifetimeDef]()
               {
                  SocketLifetimeDef.terminate();
               });
           } 
        });
        UnrealToBackendModel.connect(SocketLifetimeDef.lifetime, Protocol.Get());
        JetBrains::EditorPlugin::UE4Library::serializersOwner.registerSerializersCore(
            UnrealToBackendModel.get_serialization_context().get_serializers()
        );
    });
}
