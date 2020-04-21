package com.jetbrains.rider.plugins.unreal

import com.intellij.openapi.project.Project
import com.jetbrains.rdclient.util.idea.ProtocolSubscribedProjectComponent
import com.jetbrains.rider.model.RdTextRange
import com.jetbrains.rider.model.UnrealLogEvent
import com.jetbrains.rider.model.UnrealLogHighlighter
import com.jetbrains.rider.model.UnrealPane
import com.jetbrains.rider.plugins.unreal.toolWindow.UnrealToolWindowFactory
import java.util.concurrent.ConcurrentLinkedQueue

class UnrealToolWindowHost(
        project: Project,
        private val host: UnrealHost,
        private val unrealToolWindowContextFactory: UnrealToolWindowFactory
) : ProtocolSubscribedProjectComponent(project) {
    private var editorId = 0

    fun addTab(unrealPane: UnrealPane): UnrealTabModel {
        val number = editorId++
        val projectName = unrealPane.projectName
        val tabName = "${UnrealToolWindowFactory.TITLE_ID} #$number ($projectName)"
        unrealToolWindowContextFactory.showTab(tabName, componentLifetime)

        //todo retrieve consoleView not using hacks
        val consoleView = com.jetbrains.rider.plugins.unreal.UnrealPane.currentConsoleView

        return UnrealTabModel(unrealPane, UnrealConsoleView(consoleView)).also { model ->
            unrealTabs.add(model)
        }
    }

    val unrealTabs = mutableListOf<UnrealTabModel>()
}

class UnrealTabModel(val unrealPane: UnrealPane, private val consoleView: UnrealConsoleView) {
    val queue = ConcurrentLinkedQueue<UnrealLogHighlighter>()
    val ranges = ArrayList<RdTextRange>()

    internal fun print(logEvent: UnrealLogEvent) {
        consoleView.isOutputPaused = true
        val textRange = consoleView.print(logEvent)
        ranges.resize(logEvent.number) { RdTextRange(0, 0) }
        ranges.add(textRange)
        consoleView.flush()
        consoleView.isOutputPaused = false
    }

    internal fun processHyperlinks(hyperlinks: List<UnrealLogHighlighter>) {
        queue.addAll(hyperlinks)
    }
}

private fun <E> java.util.ArrayList<E>.resize(minSize: Int, element: () -> E) {
    ensureCapacity(minSize)
    while (size < minSize) {
        add(element())
    }
}

