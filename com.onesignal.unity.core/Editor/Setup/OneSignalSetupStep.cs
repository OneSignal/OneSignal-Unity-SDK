/*
 * Modified MIT License
 *
 * Copyright 2022 OneSignal
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * 1. The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * 2. All copies of substantial portions of the Software may only be used in connection
 * with services provided by OneSignal.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

namespace OneSignalSDK {
    /// <summary>
    /// Abstract class which must be inherited from in order to create a new setup step
    /// </summary>
    public abstract class OneSignalSetupStep {
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
            get {
                if (!_shouldCheckForCompletion)
                    return _isComplete;

                _isComplete               = _getIsStepCompleted();
                _shouldCheckForCompletion = false;

                return _isComplete;
            }
        }

        /// <summary>
        /// Runs all code necessary in order to fulfill the step
        /// </summary>
        public void RunStep() {
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
}