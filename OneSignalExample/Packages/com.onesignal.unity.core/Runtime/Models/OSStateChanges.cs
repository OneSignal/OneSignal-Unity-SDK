public abstract class OSStateChanges<T>
{
    public T to, from;
}

public class OSPermissionStateChanges : OSStateChanges<OSPermissionState> {}
public class OSSubscriptionStateChanges : OSStateChanges<OSSubscriptionState> {}
public class OSEmailSubscriptionStateChanges : OSStateChanges<OSEmailSubscriptionState> {}


