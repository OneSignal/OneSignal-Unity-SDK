/*
 * Modified MIT License
 *
 * Copyright 2023 OneSignal
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

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages dialog display and input collection for the OneSignal demo app.
/// </summary>
public class DialogManager : MonoBehaviour
{
    public enum OutcomeType
    {
        Normal,
        Unique,
        WithValue
    }
    
    [Header("Dialog Overlay")]
    public GameObject dialogOverlay;
    public GameObject dialogContainer;
    
    [Header("Dialog Content")]
    public Text titleText;
    public GameObject keyFieldContainer;
    public GameObject valueFieldContainer;
    public GameObject dropdownContainer;
    
    [Header("Input Fields")]
    public InputField keyField;
    public InputField valueField;
    public Text keyLabel;
    public Text valueLabel;
    
    [Header("Dropdown")]
    public Dropdown outcomeTypeDropdown;
    
    [Header("Buttons")]
    public Button cancelButton;
    public Button confirmButton;
    public Text confirmButtonText;
    
    // Callbacks
    private Action<string> onSingleFieldConfirm;
    private Action<string, string> onDoubleFieldConfirm;
    private Action<string, float, OutcomeType> onOutcomeConfirm;
    
    private DialogType currentDialogType;
    
    private enum DialogType
    {
        Login,
        Email,
        SMS,
        Alias,
        Tag,
        Trigger,
        Outcome
    }
    
    private void Start()
    {
        // Set up button listeners
        if (cancelButton != null)
        {
            cancelButton.onClick.AddListener(HideDialog);
        }
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(OnConfirmClicked);
        }
        
        if (outcomeTypeDropdown != null)
        {
            outcomeTypeDropdown.onValueChanged.AddListener(OnOutcomeTypeChanged);
            
            // Populate outcome type dropdown
            outcomeTypeDropdown.ClearOptions();
            outcomeTypeDropdown.AddOptions(new System.Collections.Generic.List<string>
            {
                "Normal Outcome",
                "Unique Outcome",
                "Outcome with Value"
            });
        }
        
        // Hide dialog initially
        HideDialog();
    }
    
    /// <summary>
    /// Show login dialog with single field for External User Id
    /// </summary>
    public void ShowLoginDialog(Action<string> onConfirm)
    {
        currentDialogType = DialogType.Login;
        onSingleFieldConfirm = onConfirm;
        
        SetupSingleFieldDialog("", "External User Id", "LOGIN");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show email dialog with single field
    /// </summary>
    public void ShowEmailDialog(Action<string> onConfirm)
    {
        currentDialogType = DialogType.Email;
        onSingleFieldConfirm = onConfirm;
        
        SetupSingleFieldDialog("", "New Email", "ADD");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show SMS dialog with single field
    /// </summary>
    public void ShowSmsDialog(Action<string> onConfirm)
    {
        currentDialogType = DialogType.SMS;
        onSingleFieldConfirm = onConfirm;
        
        SetupSingleFieldDialog("", "New SMS", "ADD");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show alias dialog with two fields
    /// </summary>
    public void ShowAliasDialog(Action<string, string> onConfirm)
    {
        currentDialogType = DialogType.Alias;
        onDoubleFieldConfirm = onConfirm;
        
        SetupDoubleFieldDialog("Add Alias", "Key", "Value", "ADD");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show tag dialog with two fields
    /// </summary>
    public void ShowTagDialog(Action<string, string> onConfirm)
    {
        currentDialogType = DialogType.Tag;
        onDoubleFieldConfirm = onConfirm;
        
        SetupDoubleFieldDialog("Add Tag", "Key", "Value", "ADD");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show trigger dialog with two fields
    /// </summary>
    public void ShowTriggerDialog(Action<string, string> onConfirm)
    {
        currentDialogType = DialogType.Trigger;
        onDoubleFieldConfirm = onConfirm;
        
        SetupDoubleFieldDialog("Add Trigger", "Key", "Value", "ADD");
        ShowDialogOverlay();
    }
    
    /// <summary>
    /// Show outcome dialog with dropdown and conditional fields
    /// </summary>
    public void ShowOutcomeDialog(Action<string, float, OutcomeType> onConfirm)
    {
        currentDialogType = DialogType.Outcome;
        onOutcomeConfirm = onConfirm;
        
        SetupOutcomeDialog();
        ShowDialogOverlay();
    }
    
    private void SetupSingleFieldDialog(string title, string fieldLabel, string confirmText)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }
        
        if (keyFieldContainer != null) keyFieldContainer.SetActive(true);
        if (valueFieldContainer != null) valueFieldContainer.SetActive(false);
        if (dropdownContainer != null) dropdownContainer.SetActive(false);
        
        if (keyLabel != null) keyLabel.text = fieldLabel;
        if (keyField != null) keyField.text = "";
        
        if (confirmButtonText != null) confirmButtonText.text = confirmText;
    }
    
    private void SetupDoubleFieldDialog(string title, string keyLabelText, string valueLabelText, string confirmText)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.gameObject.SetActive(!string.IsNullOrEmpty(title));
        }
        
        if (keyFieldContainer != null) keyFieldContainer.SetActive(true);
        if (valueFieldContainer != null) valueFieldContainer.SetActive(true);
        if (dropdownContainer != null) dropdownContainer.SetActive(false);
        
        if (keyLabel != null) keyLabel.text = keyLabelText;
        if (valueLabel != null) valueLabel.text = valueLabelText;
        if (keyField != null) keyField.text = "";
        if (valueField != null) valueField.text = "";
        
        if (confirmButtonText != null) confirmButtonText.text = confirmText;
    }
    
    private void SetupOutcomeDialog()
    {
        if (titleText != null)
        {
            titleText.text = "";
            titleText.gameObject.SetActive(false);
        }
        
        if (keyFieldContainer != null) keyFieldContainer.SetActive(true);
        if (dropdownContainer != null) dropdownContainer.SetActive(true);
        
        if (keyLabel != null) keyLabel.text = "Name";
        if (keyField != null) keyField.text = "";
        
        if (outcomeTypeDropdown != null)
        {
            outcomeTypeDropdown.value = 0;
        }
        
        // Show/hide value field based on outcome type
        OnOutcomeTypeChanged(0);
        
        if (confirmButtonText != null) confirmButtonText.text = "SEND";
    }
    
    private void OnOutcomeTypeChanged(int index)
    {
        // Show value field only for "Outcome with Value" (index 2)
        bool showValue = index == 2;
        
        if (valueFieldContainer != null)
        {
            valueFieldContainer.SetActive(showValue);
        }
        
        if (showValue && valueLabel != null)
        {
            valueLabel.text = "Value";
        }
        
        if (valueField != null)
        {
            valueField.text = "";
        }
    }
    
    private void ShowDialogOverlay()
    {
        if (dialogOverlay != null)
        {
            dialogOverlay.SetActive(true);
        }
    }
    
    /// <summary>
    /// Hide the dialog overlay
    /// </summary>
    public void HideDialog()
    {
        if (dialogOverlay != null)
        {
            dialogOverlay.SetActive(false);
        }
        
        // Clear callbacks
        onSingleFieldConfirm = null;
        onDoubleFieldConfirm = null;
        onOutcomeConfirm = null;
    }
    
    private void OnConfirmClicked()
    {
        switch (currentDialogType)
        {
            case DialogType.Login:
            case DialogType.Email:
            case DialogType.SMS:
                string singleValue = keyField != null ? keyField.text : "";
                onSingleFieldConfirm?.Invoke(singleValue);
                break;
                
            case DialogType.Alias:
            case DialogType.Tag:
            case DialogType.Trigger:
                string key = keyField != null ? keyField.text : "";
                string value = valueField != null ? valueField.text : "";
                onDoubleFieldConfirm?.Invoke(key, value);
                break;
                
            case DialogType.Outcome:
                string outcomeName = keyField != null ? keyField.text : "";
                float outcomeValue = 0f;
                
                if (valueField != null && !string.IsNullOrEmpty(valueField.text))
                {
                    float.TryParse(valueField.text, out outcomeValue);
                }
                
                OutcomeType outcomeType = (OutcomeType)(outcomeTypeDropdown != null ? outcomeTypeDropdown.value : 0);
                onOutcomeConfirm?.Invoke(outcomeName, outcomeValue, outcomeType);
                break;
        }
        
        HideDialog();
    }
}
