package model.rider

import com.jetbrains.rd.generator.nova.*
import com.jetbrains.rd.generator.nova.PredefinedType.*
import com.jetbrains.rd.generator.nova.csharp.CSharp50Generator
import com.jetbrains.rider.model.nova.ide.SolutionModel
import com.jetbrains.rider.model.nova.ide.SolutionModel.LightweightHighlighter
import model.lib.ue4.UE4Library.BlueprintReference
import model.lib.ue4.UE4Library.FString
import model.lib.ue4.UE4Library.StringRange
import model.lib.ue4.UE4Library.UnrealLogEvent

@Suppress("unused")
object RdRiderModel : Ext(SolutionModel.Solution) {
    init {
        setting(CSharp50Generator.AdditionalUsings) {
            listOf("JetBrains.Unreal.Lib")
        }
    }

    val LinkRequest = structdef("LinkRequest") {
        field("data", FString)
    }
    val ILinkResponse = basestruct("ILinkResponse") {}

    val LinkResponseBlueprint = structdef("LinkResponseBlueprint") extends ILinkResponse {
        field("fullPath", FString)
        field("range", StringRange)
    }

    val LinkResponseFilePath = structdef("LinkResponseFilePath") extends ILinkResponse {
        field("fullPath", FString)
        field("range", StringRange)
    }

    val LinkResponseUnresolved = structdef("LinkResponseUnresolved") extends ILinkResponse {}

    private val UnrealLogHighlighter = basestruct("UnrealLogHighlighter") extends LightweightHighlighter {
        field("messageNumber", int)
    }

    private val UnrealLogPathHighlighter = structdef("UnrealLogPathHighlighter") extends UnrealLogHighlighter {
        field("path", string)
    }

    private val UnrealLogBlueprintLinkHighlighter = structdef("UnrealLogBlueprintLinkHighlighter") extends UnrealLogHighlighter {
        field("path", BlueprintReference)
    }

    private val UnrealLogFileHyperlinkHighlighter = structdef("UnrealLogFileHyperlinkHighlighter") extends UnrealLogHighlighter {}

    private val UnrealLogIdentifierHighlighter = structdef("UnrealLogIdentifierHighlighter") extends UnrealLogHighlighter {}

    private val UnrealLogDefaultHighlighter = structdef("UnrealLogDefaultHighlighter") extends UnrealLogHighlighter {}

    private val UnrealLogStackFrameOuterHighlighter = structdef("UnrealLogStackFrameOuterHighlighter") extends UnrealLogHighlighter {
        field("name", FString)
    }

    private val UnrealLogStackFrameInnerHighlighter = structdef("UnrealLogStackFrameInnerHighlighter") extends UnrealLogHighlighter {
        field("outerName", FString)
        field("name", FString)
    }

    private val UnrealLogScriptMsgHeaderHighlighter = structdef("UnrealLogScriptMsgHeaderHighlighter") extends UnrealLogHighlighter {}


    private val UnrealLogNavigationPoint = structdef {
        field("line", string)
        field("hyperlink", UnrealLogHighlighter)
    }

    private val UnrealPane = classdef {
        field("projectName", string)
        signal("navigateIdentifier", UnrealLogIdentifierHighlighter).write
        signal("navigateBlueprint", BlueprintReference).write
        signal("addHighlighters", immutableList(UnrealLogHighlighter)).readonly
        signal("unrealLog", UnrealLogEvent).readonly
    }

    private val ToolWindowModel = aggregatedef("ToolWindowModel") {
        list("tabs", UnrealPane).readonly
    }

    init {
        property("editorId", 0).readonly.async
        property("play", int)
        property("playMode", int)
        signal("frameSkip", bool)

        field("toolWindowModel", ToolWindowModel)

        callback("AllowSetForegroundWindow", int, bool)

        property("isConnectedToUnrealEditor", false).readonly.async
        property("isUnrealEngineSolution", false)

        sink("onEditorModelOutOfSync", void)
        source("installEditorPlugin", void)
    }
}
