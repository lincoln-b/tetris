﻿using UnityEngine;
using System.Collections;

namespace DigitalRubyShared
{
    public class DemoScriptZoomableScrollView : MonoBehaviour
    {
        public FingersScript FingersScript;
        public UnityEngine.UI.ScrollRect ScrollView;
        public Canvas Canvas;

        private float scaleStart;
        private float scaleEnd;
        private float scaleTime;
        private float elapsedScaleTime;
        private Vector2 scalePosStart;
        private Vector2 scalePosEnd;

        private void Start()
        {
            ScaleGestureRecognizer scale = new ScaleGestureRecognizer();
            scale.StateUpdated += Scale_Updated;
            scale.PlatformSpecificView = ScrollView.gameObject;
            scale.ThresholdUnits = 0.0f;
            FingersScript.AddGesture(scale);

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.NumberOfTapsRequired = 2;
            tap.StateUpdated += Tap_Updated;
            tap.PlatformSpecificView = ScrollView.gameObject;
            FingersScript.AddGesture(tap);
        }

        private void Update()
        {
            if (scaleEnd != 0.0f)
            {
                elapsedScaleTime += Time.deltaTime;
                float lerp = Mathf.Min(1.0f, elapsedScaleTime / scaleTime);
                float scaleValue = Mathf.Lerp(scaleStart, scaleEnd, lerp);
                ScrollView.content.transform.localScale = new Vector3(scaleValue, scaleValue, 1.0f);
                ScrollView.normalizedPosition = Vector2.Lerp(scalePosStart, scalePosEnd, lerp);
                if (lerp >= 0.99f)
                {
                    scaleEnd = 0.0f;
                }
            }
        }

        private void Tap_Updated(GestureRecognizer gesture)
        {
            if (scaleEnd == 0.0f && gesture.State == GestureRecognizerState.Ended)
            {
                scaleStart = ScrollView.content.transform.localScale.x;
                scaleTime = 0.5f;
                elapsedScaleTime = 0.0f;
                Vector2 screenPos = new Vector2(gesture.FocusX, gesture.FocusY);
                Vector2 guiPos;
                // find out where in the content view we double tapped, use that to try and center a new zoom position
                RectTransformUtility.ScreenPointToLocalPointInRectangle(ScrollView.content, screenPos, Canvas.worldCamera, out guiPos);
                scalePosStart = ScrollView.normalizedPosition;
                float w = ScrollView.content.offsetMax.x - ScrollView.content.offsetMin.x;
                float h = ScrollView.content.offsetMax.y - ScrollView.content.offsetMin.y;
                scalePosEnd.x = Mathf.Clamp((guiPos.x - ScrollView.content.rect.xMin) / w, 0.0f, 1.0f);
                scalePosEnd.y = Mathf.Clamp((guiPos.y - ScrollView.content.rect.yMin) / h, 0.0f, 1.0f);
                if (ScrollView.content.transform.localScale.x >= 4.0f)
                {
                    // zoom out
                    scaleEnd = 1.0f;
                }
                else
                {
                    // zoom in
                    scaleEnd = 4.0f;
                }
            }
        }

        private void Scale_Updated(GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                float scale = (gesture as ScaleGestureRecognizer).ScaleMultiplier;
                Debug.LogFormat("Scale: {0}", scale);
                Vector3 newScale = ScrollView.content.transform.localScale *= scale;
                newScale.z = 1.0f;
                ScrollView.content.transform.localScale = Vector3.Slerp(ScrollView.content.transform.localScale, Vector3.one * scale, Time.deltaTime);
                ScrollView.content.transform.position = new Vector3(gesture.FocusX, gesture.FocusY, ScrollView.content.transform.position.z);
            }
        }
    }
}