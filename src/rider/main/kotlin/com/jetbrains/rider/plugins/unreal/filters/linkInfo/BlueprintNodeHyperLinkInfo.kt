package com.jetbrains.rider.plugins.unreal.filters.linkInfo

import com.intellij.execution.filters.HyperlinkInfo
import com.intellij.openapi.project.Project
import com.jetbrains.rd.util.getLogger
import com.jetbrains.rd.util.info
import com.jetbrains.rd.util.reactive.ISignal
import com.jetbrains.rider.model.BlueprintReference

@Suppress("unused")
class BlueprintNodeHyperLinkInfo(
        private val navigation: ISignal<BlueprintReference>,
        private val function: BlueprintReference
) : HyperlinkInfo {
    companion object {
        val logger = getLogger<BlueprintNodeHyperLinkInfo>()
    }

    override fun navigate(project: Project) {
        logger.info { "navigate by BlueprintNodeHyperLinkInfo:$function" }

        navigation.fire(function)
    }
}
