-keep class com.onesignal.** { *; }

# Work around for IllegalStateException with kotlinx-coroutines-android
-keep class kotlinx.coroutines.android.AndroidDispatcherFactory {*;}

# WorkManager initializes a Room database through AndroidX Startup before Unity starts.
# Unity release builds run R8, so keep the generated database implementation reachable.
-keep class androidx.work.impl.WorkDatabase* { *; }
-keep class androidx.work.impl.model.** { *; }
