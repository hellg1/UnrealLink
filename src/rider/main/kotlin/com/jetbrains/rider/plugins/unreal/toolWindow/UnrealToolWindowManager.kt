package com.jetbrains.rider.plugins.unreal.toolWindow

import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.reactive.IViewableList
import com.jetbrains.rdclient.util.idea.LifetimedProjectComponent
import com.jetbrains.rider.model.UnrealPane
import com.jetbrains.rider.plugins.unreal.UnrealHost
import com.jetbrains.rider.plugins.unreal.UnrealToolWindowHost
import com.jetbrains.rider.util.idea.getLogger

class UnrealToolWindowManager(project: Project,
                              host: UnrealHost,
                              private val unrealToolWindowHost: UnrealToolWindowHost
) : LifetimedProjectComponent(project) {
    companion object {
        private val logger = getLogger<UnrealToolWindowManager>()
    }

    init {
        val tabs = host.model.toolWindowModel.tabs
        tabs.change.advise(componentLifetime) { event ->
            when (event) {
                is IViewableList.Event.Add<UnrealPane> -> {
                    with(event.newValue) {
                        val model = unrealToolWindowHost.addTab(this)

                        addHighlighters.advise(componentLifetime) { hyperlinks ->
                            model.processHyperlinks(hyperlinks)
                        }

                        unrealLog.advise(componentLifetime) { logEvent ->
                            model.print(logEvent)
                        }
                    }
                }
            }
        host.model.isConnectedToUnrealEditor.advise(componentLifetime) {
            if(it) unrealToolWindowContextFactory.showTabForNewSession()
        }
    }
}
