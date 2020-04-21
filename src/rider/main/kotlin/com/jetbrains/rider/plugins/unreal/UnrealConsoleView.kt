package com.jetbrains.rider.plugins.unreal

import com.intellij.execution.impl.ConsoleViewImpl
import com.intellij.execution.ui.ConsoleView
import com.intellij.execution.ui.ConsoleViewContentType
import com.jetbrains.rd.util.eol
import com.jetbrains.rider.model.*
import com.jetbrains.rider.plugins.unreal.toolWindow.CATEGORY_WIDTH
import com.jetbrains.rider.plugins.unreal.toolWindow.TIME_WIDTH
import com.jetbrains.rider.plugins.unreal.toolWindow.VERBOSITY_WIDTH

class UnrealConsoleView(private val consoleView: ConsoleViewImpl) : ConsoleView by consoleView {
    /**
     * returns range in console view
     */
    internal fun print(unrealLogEvent: UnrealLogEvent): RdTextRange {
        print(unrealLogEvent.info)
        val start = consoleView.contentSize
        print(unrealLogEvent.text)
        val end = consoleView.contentSize
        return RdTextRange(start, end)
    }

    private fun println() {
        print(eol, ConsoleViewContentType.NORMAL_OUTPUT)
    }

    private fun print(message: FString) {
        print(message.data, ConsoleViewContentType.NORMAL_OUTPUT)
    }

    internal fun flush() {
        println()
    }

    private fun printSpaces(n: Int = 1) {
        consoleView.print(" ".repeat(n), ConsoleViewContentType.NORMAL_OUTPUT)
    }

    private fun print(s: LogMessageInfo) {
        val timeString = s.time?.toString() ?: " ".repeat(TIME_WIDTH)
        consoleView.print(timeString, ConsoleViewContentType.SYSTEM_OUTPUT)
        printSpaces()

        val verbosityContentType = when (s.type) {
            VerbosityType.Fatal -> ConsoleViewContentType.ERROR_OUTPUT
            VerbosityType.Error -> ConsoleViewContentType.ERROR_OUTPUT
            VerbosityType.Warning -> ConsoleViewContentType.LOG_WARNING_OUTPUT
            VerbosityType.Display -> ConsoleViewContentType.LOG_INFO_OUTPUT
            VerbosityType.Log -> ConsoleViewContentType.LOG_INFO_OUTPUT
            VerbosityType.Verbose -> ConsoleViewContentType.LOG_VERBOSE_OUTPUT
            VerbosityType.VeryVerbose -> ConsoleViewContentType.LOG_DEBUG_OUTPUT
            else -> ConsoleViewContentType.NORMAL_OUTPUT
        }

        val verbosityString = s.type.toString().take(VERBOSITY_WIDTH)
        consoleView.print(verbosityString, verbosityContentType)
        printSpaces(VERBOSITY_WIDTH - verbosityString.length + 1)

        val category = s.category.data.take(CATEGORY_WIDTH)
        consoleView.print(category, ConsoleViewContentType.SYSTEM_OUTPUT)
        printSpaces(CATEGORY_WIDTH - category.length + 1)
    }
}

