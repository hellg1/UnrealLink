#include "RdConnection.hpp"

#include "ProtocolFactory.h"
#include "Model/Library/UE4Library/UE4Library.Generated.h"

void RdConnection::Init(rd::SingleThreadScheduler * Scheduler, rd::Lifetime AppLifetime)
{
    Definition = MakeUnique<rd::LifetimeDefinition>(AppLifetime);
    rd::Lifetime SocketLifetime = Definition->lifetime;
    SocketLifetime->add_action([this, Scheduler, AppLifetime]()
    {
        if(!AppLifetime->is_terminated())
        {
            this->Init(Scheduler, AppLifetime);
        }
    });
    Protocol = ProtocolFactory::Create(Scheduler, SocketLifetime);
    Scheduler->queue([&, SocketLifetime]()
    {
        UnrealToBackendModel.connect(SocketLifetime, Protocol.Get());
        JetBrains::EditorPlugin::UE4Library::serializersOwner.registerSerializersCore(
            UnrealToBackendModel.get_serialization_context().get_serializers()
        );
        Protocol->wire->connected.view(SocketLifetime, [&] (rd::Lifetime Lifetime, bool const& Cond)
        {
           if(Cond)
           {
               Lifetime->add_action([&]()
               {
                  Definition->terminate();
               });
           } 
        });
    });
}
