package com.jetbrains.rider.plugins.unreal.filters

import com.intellij.execution.filters.Filter
import com.intellij.execution.filters.FilterMixin
import com.intellij.execution.filters.FilterMixin.AdditionalHighlight
import com.intellij.execution.filters.HyperlinkInfo
import com.intellij.execution.filters.UrlFilter
import com.intellij.ide.browsers.OpenUrlHyperlinkInfo
import com.intellij.openapi.editor.Document
import com.intellij.openapi.project.Project
import com.intellij.util.Consumer
import com.jetbrains.rd.platform.util.application
import com.jetbrains.rdclient.daemon.HighlighterRegistrationHost
import com.jetbrains.rider.model.*
import com.jetbrains.rider.plugins.unreal.UnrealTabModel
import com.jetbrains.rider.plugins.unreal.UnrealToolWindowHost
import com.jetbrains.rider.plugins.unreal.filters.linkInfo.BlueprintHyperLinkInfo
import com.jetbrains.rider.plugins.unreal.filters.linkInfo.IdentifierHyperLinkInfo
import com.jetbrains.rider.util.idea.getComponent
import com.jetbrains.rider.util.idea.getLogger

class UnrealHeavyLogFilter(
        val project: Project,
        private val model: RdRiderModel
) : Filter, FilterMixin {
    companion object {
        private val logger = getLogger<UnrealHeavyLogFilter>()
    }

    private val toolWindowHost = project.getComponent<UnrealToolWindowHost>()

    private val urlFilter = UrlFilter()

    override fun applyFilter(line: String, entireLength: Int): Filter.Result? {
        return urlFilter.applyFilter(line, entireLength)
    }

    override fun shouldRunHeavy() = true

    override fun getUpdateMessage() = "Looking for valid Blueprint"

    override fun applyHeavyFilter(
            copiedFragment: Document,
            startOffset: Int,
            startLineNumber: Int,
            consumer: Consumer<in AdditionalHighlight>
    ) {
        //heavy filters in tests can try to get access via Vfs to files outside of allowed roots
        if (application.isUnitTestMode) {
            return
        }

        val text = copiedFragment.charsSequence

        processLinks(startOffset, startOffset + copiedFragment.textLength, consumer)
    }

    private fun processLinks(
            startOffset: Int, endOffset: Int,
            consumer: Consumer<in AdditionalHighlight>
    ) {
        val tab = toolWindowHost.unrealTabs.last()
        val queue = tab.queue
        val ranges = tab.ranges

        val newHighlighters = ArrayList<UnrealLogHighlighter>()
        while (queue.isNotEmpty()) {
            val element = queue.element()
            val range = ranges[element.messageNumber]
            if (endOffset < range.endOffset) {
                break
            }
            newHighlighters.add(queue.poll())
        }
        val map = newHighlighters.map { highlighter ->
            val textAttributes = HighlighterRegistrationHost.getInstance().getTextAttributes(highlighter.attributeId)
            val linkInfo = tab.createHyperlinkInfo(highlighter)
            val offset = ranges[highlighter.messageNumber].startOffset
            val start = highlighter.start + offset
            val end = highlighter.end + offset
            Filter.ResultItem(start, end, linkInfo, textAttributes)
        }
        consumer.consume(AdditionalHighlight(map))
    }

    private fun UnrealTabModel.createHyperlinkInfo(highlighter: LightweightHighlighter): HyperlinkInfo? {
        return when (highlighter) {
            is UnrealLogIdentifierHighlighter -> {
                IdentifierHyperLinkInfo(unrealPane.navigateIdentifier, highlighter)
            }
            is UnrealLogPathHighlighter -> {
                OpenUrlHyperlinkInfo(highlighter.path)
            }
            is UnrealLogBlueprintLinkHighlighter -> {
                BlueprintHyperLinkInfo(unrealPane.navigateBlueprint, highlighter.path)
            }
            /*is UnrealLogStackFrameOuterHighlighter -> {
                BlueprintHyperLinkInfo(unrealPane.navigateBlueprint, BlueprintReference(highlighter.name))
            }*/
            //todo get full name on backend
            /*is UnrealLogStackFrameInnerHighlighter -> {
                val fullName = highlighter.outerName.data + BlueprintReference.separator + highlighter.name.data
                BlueprintNodeHyperLinkInfo(unrealPane.navigateBlueprint, BlueprintReference(FString(fullName)))
            }*/
            //todo set focus on node
            else -> null
        }
    }
}
