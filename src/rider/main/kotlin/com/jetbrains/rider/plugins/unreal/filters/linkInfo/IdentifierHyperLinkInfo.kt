package com.jetbrains.rider.plugins.unreal.filters.linkInfo

import com.intellij.execution.filters.HyperlinkInfo
import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.getLogger
import com.jetbrains.rd.util.info
import com.jetbrains.rd.util.reactive.ISignal
import com.jetbrains.rider.model.UnrealLogHighlighter
import com.jetbrains.rider.model.UnrealLogIdentifierHighlighter

class IdentifierHyperLinkInfo(
        private val navigation: ISignal<UnrealLogIdentifierHighlighter>,
        private val highlighter: UnrealLogIdentifierHighlighter
) : HyperlinkInfo {
    companion object {
        val logger = getLogger<IdentifierHyperLinkInfo>()
    }

    override fun navigate(project: Project) {
        logger.info { "navigate by IdentifierHyperLinkInfo:$highlighter" }

        navigation.fire(highlighter)
    }
}
