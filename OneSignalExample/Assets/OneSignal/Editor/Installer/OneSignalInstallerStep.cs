#if !ONE_SIGNAL_INSTALLED
/// <summary>
/// 
/// </summary>
public abstract class OneSignalInstallerStep
{
    /// <summary>
    /// 
    /// </summary>
    public abstract string Summary { get; }
    /// <summary>
    /// 
    /// </summary>
    public abstract string Details { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public abstract string DocumentationLink { get; }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsStepCompleted {
        get
        {
            if (!_shouldCheckForCompletion) 
                return _isComplete;
            
            _isComplete = _getIsStepCompleted();
            _shouldCheckForCompletion = false;

            return _isComplete;
        } 
    }

    /// <summary>
    /// 
    /// </summary>
    public void Install()
    {
        if (IsStepCompleted)
            return;
        
        _install();
        _shouldCheckForCompletion = true;
    }

    protected abstract bool _getIsStepCompleted();
    protected abstract void _install();

    private bool _isComplete = false;
    protected bool _shouldCheckForCompletion = true;
}
#endif