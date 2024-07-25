import android.app.NotificationChannel;
import android.app.NotificationManager;
import android.content.Context;
import android.graphics.BitmapFactory;
import android.os.Build;
import androidx.core.app.NotificationCompat;

import com.onesignal.notifications.IDisplayableMutableNotification;
import com.onesignal.notifications.INotificationReceivedEvent;
import com.onesignal.notifications.INotificationServiceExtension;

import org.json.JSONException;
import org.json.JSONObject;

import java.util.Objects;

/** @noinspection unused*/
public class NotificationServiceExtension implements INotificationServiceExtension {
    // Live Notification IDs
    private static final String PROGRESS_LIVE_NOTIFICATION = "progress";
    private static final String ANOTHER_LIVE_NOTIFICATION = "another";
    private static final String DISMISS_LIVE_NOTIFICATION = "dismiss";

    // Channels
    private static final String PROGRESS_CHANNEL_ID = "progress_channel";
    private static final String ANOTHER_CHANNEL_ID = "another_channel";


    @Override
    public void onNotificationReceived(INotificationReceivedEvent event) {
        IDisplayableMutableNotification notification = event.getNotification();
        Context context = event.getContext();
        NotificationManager notificationManager = (NotificationManager) context.getSystemService(Context.NOTIFICATION_SERVICE);
        createNotificationChannels(notificationManager);

        NotificationCompat.Builder builder;

        JSONObject lnPayload = Objects
                .requireNonNull(notification.getAdditionalData())
                .optJSONObject("live_notification");
        if (lnPayload == null) {
            return;
        }

        try {

            String lnEvent = lnPayload.getString("event");
            if (lnEvent.equals("dismiss")) {
                notificationManager.cancelAll();
                event.preventDefault();
                return;
            }

            String lnKey = lnPayload.optString("key");
            JSONObject lnEventUpdates = lnPayload.getJSONObject("event_updates");

            switch (lnKey) {
                case PROGRESS_LIVE_NOTIFICATION:
                    int currentProgress = lnEventUpdates
                            .getInt("current_progress");

                    builder = new NotificationCompat.Builder(context, PROGRESS_CHANNEL_ID)
                            .setContentTitle(" Progress Live Notifications")
                            .setContentText("It's working...")
                            .setSmallIcon(android.R.drawable.ic_media_play)
                            .setLargeIcon(BitmapFactory.decodeResource(context.getResources(), android.R.drawable.ic_dialog_info))
                            .setOngoing(true)
                            .setOnlyAlertOnce(true)
                            .setProgress(100, currentProgress, false);
                    notificationManager.notify(PROGRESS_LIVE_NOTIFICATION, 1, builder.build());
                    break;
                case ANOTHER_LIVE_NOTIFICATION:
                    builder = new NotificationCompat.Builder(context, ANOTHER_CHANNEL_ID)
                            .setContentTitle("Some other Live Notification")
                            .setContentText("Content goes here")
                            .setSmallIcon(android.R.drawable.ic_media_play)
                            .setLargeIcon(BitmapFactory.decodeResource(context.getResources(), android.R.drawable.ic_dialog_info))
                            .setOngoing(true)
                            .setOnlyAlertOnce(true);
                    notificationManager.notify(ANOTHER_LIVE_NOTIFICATION, 2, builder.build());
                    break;
                default:
                    throw new IllegalStateException("Unsupported Live Notification Key provided: " + lnKey);
            }
           // ID 1 is arbitrary
        } catch (JSONException | NullPointerException e) {
            System.err.println(e.getMessage());
        }
    }

    private void createNotificationChannels(NotificationManager notificationManager) {
        if (Build.VERSION.SDK_INT < Build.VERSION_CODES.O) return;

        NotificationChannel channel1 = notificationManager.getNotificationChannel(PROGRESS_CHANNEL_ID);
        if (channel1 == null) {
            channel1 = new NotificationChannel(PROGRESS_CHANNEL_ID, "Progress Live Notification", NotificationManager.IMPORTANCE_LOW);
            channel1.setDescription("Shows the progress of a download");
            notificationManager.createNotificationChannel(channel1);
        }

        NotificationChannel channel2 = notificationManager.getNotificationChannel(ANOTHER_CHANNEL_ID);
        if (channel2 == null) {
            channel2 = new NotificationChannel(ANOTHER_CHANNEL_ID, "Another Live Notification", NotificationManager.IMPORTANCE_LOW);
            channel2.setDescription("Whatever you like");
            notificationManager.createNotificationChannel(channel2);
        }
    }
}