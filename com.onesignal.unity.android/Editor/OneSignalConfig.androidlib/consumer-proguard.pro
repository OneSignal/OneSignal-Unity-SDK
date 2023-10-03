-keep class com.onesignal.** { *; }

# Work around for IllegalStateException with kotlinx-coroutines-android
-keep class kotlinx.coroutines.android.AndroidDispatcherFactory {*;}