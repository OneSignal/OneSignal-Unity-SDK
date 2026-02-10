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

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Handles toggle visual state changes - updates background color and thumb position
/// with smooth sliding animation.
/// </summary>
[RequireComponent(typeof(Toggle))]
public class ToggleColorHandler : MonoBehaviour
{
    private static readonly Color ToggleGreen = new Color(0.298f, 0.686f, 0.314f, 1f);
    private static readonly Color ToggleGray = new Color(0.75f, 0.75f, 0.75f, 1f);
    
    [SerializeField] private float animationDuration = 0.15f;
    
    private Toggle toggle;
    private Image backgroundImage;
    private RectTransform thumbRect;
    private Coroutine animationCoroutine;
    
    // Thumb positions: left (off) and right (on)
    private const float OffPosition = 4f;
    private const float OnPosition = 44f;
    
    private void Awake()
    {
        toggle = GetComponent<Toggle>();
        
        // Find background (first child named "Background")
        Transform bgTransform = transform.Find("Background");
        if (bgTransform != null)
        {
            backgroundImage = bgTransform.GetComponent<Image>();
        }
        
        // Find thumb (child named "Checkmark")
        Transform thumbTransform = transform.Find("Checkmark");
        if (thumbTransform != null)
        {
            thumbRect = thumbTransform.GetComponent<RectTransform>();
        }
        
        // Subscribe to toggle changes
        toggle.onValueChanged.AddListener(OnToggleChanged);
        
        // Set initial state immediately (no animation)
        SetToggleState(toggle.isOn, false);
    }
    
    private void OnToggleChanged(bool isOn)
    {
        // Animate to new state
        SetToggleState(isOn, true);
    }
    
    private void SetToggleState(bool isOn, bool animate)
    {
        float targetX = isOn ? OnPosition : OffPosition;
        Color targetColor = isOn ? ToggleGreen : ToggleGray;
        
        if (animate && gameObject.activeInHierarchy)
        {
            // Stop any existing animation
            if (animationCoroutine != null)
            {
                StopCoroutine(animationCoroutine);
            }
            animationCoroutine = StartCoroutine(AnimateToggle(targetX, targetColor));
        }
        else
        {
            // Set immediately without animation
            if (thumbRect != null)
            {
                thumbRect.anchoredPosition = new Vector2(targetX, 0);
            }
            if (backgroundImage != null)
            {
                backgroundImage.color = targetColor;
            }
        }
    }
    
    private IEnumerator AnimateToggle(float targetX, Color targetColor)
    {
        float startX = thumbRect != null ? thumbRect.anchoredPosition.x : OffPosition;
        Color startColor = backgroundImage != null ? backgroundImage.color : ToggleGray;
        
        float elapsed = 0f;
        
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);
            
            // Use smooth step for easing
            float smoothT = t * t * (3f - 2f * t);
            
            // Animate thumb position
            if (thumbRect != null)
            {
                float currentX = Mathf.Lerp(startX, targetX, smoothT);
                thumbRect.anchoredPosition = new Vector2(currentX, 0);
            }
            
            // Animate background color
            if (backgroundImage != null)
            {
                backgroundImage.color = Color.Lerp(startColor, targetColor, smoothT);
            }
            
            yield return null;
        }
        
        // Ensure final state
        if (thumbRect != null)
        {
            thumbRect.anchoredPosition = new Vector2(targetX, 0);
        }
        if (backgroundImage != null)
        {
            backgroundImage.color = targetColor;
        }
        
        animationCoroutine = null;
    }
    
    private void OnDestroy()
    {
        if (toggle != null)
        {
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }
}
