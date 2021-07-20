#if !ONE_SIGNAL_INSTALLED
/// <summary>
/// Abstract class which must be inherited from in order to create a new setup step
/// </summary>
public abstract class OneSignalSetupStep
{
    /// <summary>
    /// Short description of what this step will do
    /// </summary>
    public abstract string Summary { get; }
    
    /// <summary>
    /// Detailed description of precisely what this step will do
    /// </summary>
    public abstract string Details { get; }

    /// <summary>
    /// Whether this step is required for operation of the SDK
    /// </summary>
    public abstract bool IsRequired { get; }
    
    /// <summary>
    /// Checks whether or not this step has been completed
    /// </summary>
    /// <remarks>
    /// The result is cached and only reset on run or specific other conditions
    /// </remarks>
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
    /// Runs all code necessary in order to fulfill the step
    /// </summary>
    public void RunStep()
    {
        if (IsStepCompleted)
            return;
        
        _runStep();
        _shouldCheckForCompletion = true;
    }

    protected abstract bool _getIsStepCompleted();
    protected abstract void _runStep();

    private bool _isComplete = false;
    protected bool _shouldCheckForCompletion = true;
}
#endif