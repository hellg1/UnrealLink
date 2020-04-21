package com.jetbrains.rider.plugins.unreal.toolWindow

import com.intellij.ide.impl.ContentManagerWatcher
import com.intellij.openapi.components.service
import com.intellij.openapi.project.Project
import com.intellij.openapi.wm.ToolWindow
import com.intellij.openapi.wm.ToolWindowAnchor
import com.intellij.openapi.wm.ToolWindowManager
import com.jetbrains.rider.model.rdRiderModel
import com.jetbrains.rider.plugins.unreal.UnrealPane
import com.jetbrains.rider.projectView.solution
import com.jetbrains.rider.ui.toolWindow.RiderOnDemandToolWindowFactory
import icons.RiderIcons

class UnrealToolWindowFactory(val project: Project)
    : RiderOnDemandToolWindowFactory<String>(project, TOOLWINDOW_ID, { it }, ::UnrealPane, { it }) {

    companion object {
        const val TOOLWINDOW_ID = "Unreal"
        const val TITLE_ID = "Unreal Editor Log"
        const val ACTION_PLACE = "unreal"

        fun getInstance(project: Project): UnrealToolWindowFactory = project.service()
    }

    override fun registerToolWindow(toolWindowManager: ToolWindowManager, project: Project): ToolWindow {
        val toolWindow = toolWindowManager.registerToolWindow(TOOLWINDOW_ID, false, ToolWindowAnchor.BOTTOM, project, true, false)

        ContentManagerWatcher.watchContentManager(toolWindow, toolWindow.contentManager)

        toolWindow.title = "unreal"
        toolWindow.setIcon(RiderIcons.Stacktrace.Stacktrace) //todo change

        return toolWindow
    }

    internal val model = project.solution.rdRiderModel
}
