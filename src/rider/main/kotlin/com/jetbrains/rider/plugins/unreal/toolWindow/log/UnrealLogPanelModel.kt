package com.jetbrains.rider.plugins.unity.toolWindow.log

//import com.jetbrains.rider.plugins.unity.editorPlugin.model.RdLogEvent
//import com.jetbrains.rider.plugins.unity.editorPlugin.model.RdLogEventMode
//import com.jetbrains.rider.plugins.unity.editorPlugin.model.RdLogEventType
import com.jetbrains.rd.util.lifetime.Lifetime
import com.jetbrains.rd.util.reactive.Signal
import com.jetbrains.rd.util.reactive.fire

class UnrealLogPanelModel(val lifetime: Lifetime, val project: com.intellij.openapi.project.Project) {
    private val lock = Object()
    private val maxItemsCount = 10000

//    inner class TypeFilters {
//        private var showErrors = true
//        private var showWarnings = true
//        private var showMessages = true

//        fun getShouldBeShown(type: RdLogEventType) = when (type) {
//            RdLogEventType.Error -> showErrors
//            RdLogEventType.Warning -> showWarnings
//            RdLogEventType.Message -> showMessages
//        }

//        fun setShouldBeShown(type: RdLogEventType, value: Boolean) = when (type) {
//            RdLogEventType.Error -> {
//                synchronized(lock) { showErrors = value }
//                onChanged.fire()
//            }
//            RdLogEventType.Warning -> {
//                synchronized(lock) { showWarnings = value }
//                onChanged.fire()
//            }
//            RdLogEventType.Message -> {
//                synchronized(lock) { showMessages = value }
//                onChanged.fire()
//            }
//        }

//        val onChanged = Signal.Void()
//    }

//    inner class ModeFilters {
//        private var showEdit = true
//        private var showPlay = true
//
//        fun getShouldBeShown(mode: RdLogEventMode) = when (mode) {
//            RdLogEventMode.Edit -> showEdit
//            RdLogEventMode.Play -> showPlay
//        }
//
//        fun setShouldBeShown(mode: RdLogEventMode, value: Boolean) = when (mode) {
//            RdLogEventMode.Edit -> {
//                synchronized(lock) { showEdit = value }
//                onChanged.fire()
//            }
//            RdLogEventMode.Play -> {
//                synchronized(lock) { showPlay = value }
//                onChanged.fire()
//            }
//        }
//
//        val onChanged = Signal.Void()
//    }

//    inner class TextFilter {
//        private var searchTerm = ""
//
//        fun getShouldBeShown(text: String):Boolean {
//            return text.contains(searchTerm, true)
//        }
//
//        fun setPattern(value: String) {
//            synchronized(lock) { searchTerm = value }
//            onChanged.fire()
//        }
//
//        val onChanged = Signal.Void()
//    }

    inner class Events {
        val allEvents = ArrayList<String>()

        fun clear() {
            synchronized(lock) { allEvents.clear() }
            selectedItem = null
            onChanged.fire()
        }

        fun addEvent(event: String) {
            synchronized(lock) {
                if (allEvents.count() > maxItemsCount)
                {
                    clear()
                }
                allEvents.add(event)
            }

            if (isVisibleEvent(event))
                onAdded.fire(event)
        }

        val onChanged = Signal.Void()
    }

    private fun isVisibleEvent(event: String):Boolean
    {
        return true
    }

    private fun getVisibleEvents(): List<String> {
        synchronized(lock) {
            return events.allEvents
                .filter { isVisibleEvent(it) }
        }
    }

//    val typeFilters = TypeFilters()
//    val modeFilters = ModeFilters()
//    val textFilter = TextFilter()
    val events = Events()
//    var mergeSimilarItems = Property<Boolean>(false)

    val onAdded = Signal<String>()
    val onChanged = Signal<List<String>>()
//    val onCleared = Signal.Void()

    fun fire() = onChanged.fire(getVisibleEvents())

    var selectedItem : LogPanelItem? = null

    init {
//        typeFilters.onChanged.advise(lifetime) { fire() }
//        modeFilters.onChanged.advise(lifetime) { fire() }
//        textFilter.onChanged.advise(lifetime) { fire() }
        events.onChanged.advise(lifetime) { fire() }
//        mergeSimilarItems.advise(lifetime) { fire() }
    }
}