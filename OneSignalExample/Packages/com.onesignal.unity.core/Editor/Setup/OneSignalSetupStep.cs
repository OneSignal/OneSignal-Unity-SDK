/// <summary>
/// Abstract class which must be inherited from in order to create a new install step
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
    /// 
    /// </summary>
    public abstract string DocumentationLink { get; }
    
    /// <summary>
    /// Checks whether or not this step has been completed
    /// </summary>
    /// <remarks>
    /// The result is cached and only reset on install or specific other conditions
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