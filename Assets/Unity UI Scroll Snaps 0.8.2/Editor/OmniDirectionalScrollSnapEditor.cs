//Dependencies:
// - OmniDirectionalScrollSnap: Source > Scripts > Components

//Contributors:
//BeksOmega

using UnityEditor;
using UnityEditor.AnimatedValues;

namespace UnityEngine.UI.ScrollSnaps
{
    [CustomEditor(typeof(OmniDirectionalScrollSnap))]
    public class OmniDirectionalScrollSnapEditor : Editor
    {
        private AnimBool
            showFriction,
            showTension,
            showScrollBarTension,
            showScrollTension,
            showScrollInfo,
            showCalculateError,
            showSnapError,
            showDrawGizmos;

        private bool
            showFilters,
            showCalculateFilter,
            showSnapFilter,
            showEvents,
            showAnimationSettings,
            showAdvancedScrollSettings,
            showAdvancedScrollBarSettings;

        private bool
            dontMatchScrollBarFriction,
            dontMatchScrollBarInterpolator,
            dontMatchScrollBarTension,
            dontMatchScrollFriction,
            dontMatchScrollInterpolator,
            dontMatchScrollTension = true;

        private SerializedProperty
            content,
            movementType,
            simulateFlings,
            friction,
            interpolator,
            tension,
            scrollSensitivity,
            scrollWheelDirection,
            scrollDelay,
            minDuration,
            maxDuration,
            addToCalculateFilter,
            calculateFilterMode,
            calculateFilter,
            addToSnapFilter,
            snapFilterMode,
            snapFilter,
            viewPort,
            horizontalScrollBar,
            verticalScrollBar,
            onValueChanged,
            startMovement,
            closestSnapChanged,
            snappedToItem,
            targetItemSelected,
            drawGizmos,
            loop,
            endSpacing,
            buttonAnimationDuration,
            buttonItemsToMoveBy,
            buttonInterpolator,
            scrollFriction,
            scrollInterpolator,
            scrollBarFriction,
            scrollBarInterpolator,
            buttonTension,
            scrollBarTension,
            scrollTension,
            allowTouch,
            startItem;

        OmniDirectionalScrollSnap scrollSnap;

        private void OnEnable()
        {
            scrollSnap = (OmniDirectionalScrollSnap)target;

            content = serializedObject.FindProperty("m_Content");
            movementType = serializedObject.FindProperty("m_MovementType");
            simulateFlings = serializedObject.FindProperty("m_SimulateFlings");
            friction = serializedObject.FindProperty("m_Friction");
            interpolator = serializedObject.FindProperty("m_InterpolatorType");
            tension = serializedObject.FindProperty("m_Tension");
            scrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
            scrollWheelDirection = serializedObject.FindProperty("m_ScrollWheelDirection");
            scrollDelay = serializedObject.FindProperty("m_ScrollDelay");
            minDuration = serializedObject.FindProperty("m_MinDuration");
            maxDuration = serializedObject.FindProperty("m_MaxDuration");
            addToCalculateFilter = serializedObject.FindProperty("m_AddInactiveChildrenToCalculatingFilter");
            calculateFilterMode = serializedObject.FindProperty("m_FilterModeForCalculatingSize");
            calculateFilter = serializedObject.FindProperty("m_CalculatingFilter");
            addToSnapFilter = serializedObject.FindProperty("m_AddInactiveChildrenToSnapPositionsFilter");
            snapFilterMode = serializedObject.FindProperty("m_FilterModeForSnapPositions");
            snapFilter = serializedObject.FindProperty("m_SnapPositionsFilter");
            viewPort = serializedObject.FindProperty("m_Viewport");
            horizontalScrollBar = serializedObject.FindProperty("m_HorizontalScrollbar");
            verticalScrollBar = serializedObject.FindProperty("m_VerticalScrollbar");
            onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            startMovement = serializedObject.FindProperty("m_StartMovementEvent");
            closestSnapChanged = serializedObject.FindProperty("m_ClosestSnapPositionChanged");
            snappedToItem = serializedObject.FindProperty("m_SnappedToItem");
            targetItemSelected = serializedObject.FindProperty("m_TargetItemSelected");
            drawGizmos = serializedObject.FindProperty("m_DrawGizmos");
            scrollFriction = serializedObject.FindProperty("m_ScrollFriction");
            scrollInterpolator = serializedObject.FindProperty("m_ScrollInterpolator");
            scrollBarFriction = serializedObject.FindProperty("m_ScrollBarFriction");
            scrollBarInterpolator = serializedObject.FindProperty("m_ScrollBarInterpolator");
            scrollBarTension = serializedObject.FindProperty("m_ScrollBarTension");
            scrollTension = serializedObject.FindProperty("m_ScrollTension");
            allowTouch = serializedObject.FindProperty("m_AllowTouchInput");
            startItem = serializedObject.FindProperty("m_StartItem");

            showFriction = new AnimBool(simulateFlings.boolValue);
            showFriction.valueChanged.AddListener(Repaint);
            showTension = new AnimBool(interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Anticipate || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.AnticipateOvershoot || interpolator.enumValueIndex == (int)OmniDirectionalScrollSnap.InterpolatorType.Overshoot);
            showTension.valueChanged.AddListener(Repaint);
            showScrollBarTension = new AnimBool(ShowTension(scrollBarInterpolator.enumValueIndex));
            showScrollBarTension.valueChanged.AddListener(Repaint);
            showScrollTension = new AnimBool(ShowTension(scrollInterpolator.enumValueIndex));
            showScrollTension.valueChanged.AddListener(Repaint);
            showScrollInfo = new AnimBool(scrollSensitivity.floatValue > 0);
            showScrollInfo.valueChanged.AddListener(Repaint);
            showCalculateError = new AnimBool(calculateFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
            showCalculateError.valueChanged.AddListener(Repaint);
            showSnapError = new AnimBool(snapFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
            showSnapError.valueChanged.AddListener(Repaint);
            showDrawGizmos = new AnimBool(scrollSnap.content != null);
            showDrawGizmos.valueChanged.AddListener(Repaint);

            CheckMatching();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(scrollSnap), typeof(OmniDirectionalScrollSnap), false);
            GUI.enabled = true;

            EditorGUILayout.Space();

            DrawLayoutVariables();

            EditorGUILayout.Space();

            DrawAnimationVariables();
            DrawItemFilters();

            EditorGUILayout.Space();

            DrawObjectVariables();

            EditorGUILayout.Space();

            DrawEvents();

            EditorGUILayout.Space();

            showDrawGizmos.target = scrollSnap.content != null;
            if (EditorGUILayout.BeginFadeGroup(showDrawGizmos.faded))
                EditorGUILayout.PropertyField(drawGizmos);
            EditorGUILayout.EndFadeGroup();

            if (GUILayout.Button("Update"))
            {
                scrollSnap.UpdateLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }


        private void DrawLayoutVariables()
        {
            EditorGUILayout.PropertyField(content);
            EditorGUILayout.PropertyField(movementType, new GUIContent("Movement Type", "Clamped mode keeps the content within the bounds of the Scroll Snap. Elastic bounces the content when it gets to the edge of the Scroll Snap."));
        }

        private void DrawAnimationVariables()
        {
            showAnimationSettings = EditorGUILayout.Foldout(showAnimationSettings, "Animation Settings", true, EditorStyles.foldout);
            if (showAnimationSettings)
            {
                DrawDefaultAnimationVariables();

                EditorGUILayout.Space();
                
                EditorGUI.indentLevel++;
                DrawScrollBarAnimationVariables();
                DrawScrollAnimationVariables();
                EditorGUI.indentLevel--;

                ApplyAnimationValues();

                EditorGUILayout.Space();
            }
        }

        private void DrawDefaultAnimationVariables()
        {
            EditorGUILayout.PropertyField(simulateFlings, new GUIContent("Simulate Flings", "When enabled selects the snap position based on where the Scroll Snap would land if it was flung. When disabled selects the snap position based on where the Scroll Snap currently is."));
            EditorGUILayout.PropertyField(friction);

            EditorGUILayout.PropertyField(interpolator, new GUIContent("Interpolator", "Changes how the scroll snap animates."));
            showTension.target = ShowTension(interpolator.enumValueIndex);
            if (EditorGUILayout.BeginFadeGroup(showTension.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(tension, new GUIContent("Tension", "Modifies the interpolator"));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();

            EditorGUILayout.PropertyField(minDuration, new GUIContent("Min Duration", "The minimum duration in milliseconds for any snapping or scrolling animations"));
            EditorGUILayout.PropertyField(maxDuration, new GUIContent("Max Duration", "The maximum duration in milliseconds for any snapping or scrolling animations"));

            EditorGUILayout.Space();

            EditorGUILayout.PropertyField(allowTouch);
        }

        private void DrawScrollBarAnimationVariables()
        {
            showAdvancedScrollBarSettings = EditorGUILayout.Foldout(showAdvancedScrollBarSettings, "Scroll Bar Settings", true, EditorStyles.foldout);
            if (showAdvancedScrollBarSettings)
            {
                dontMatchScrollBarFriction = BoolProperty(scrollBarFriction, new GUIContent("Friction"), dontMatchScrollBarFriction);
                dontMatchScrollBarInterpolator = BoolProperty(scrollBarInterpolator, new GUIContent("Interpolator"), dontMatchScrollBarInterpolator);
                showScrollBarTension.target = ShowTension(scrollBarInterpolator.enumValueIndex);
                if (EditorGUILayout.BeginFadeGroup(showScrollBarTension.faded))
                {
                    EditorGUI.indentLevel++;
                    dontMatchScrollBarTension = BoolProperty(scrollBarTension, new GUIContent("Tension", "Modifies the interpolator"), dontMatchScrollBarTension);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                if (Button("Match Defaults"))
                {
                    Undo.RecordObject(scrollSnap, "Resetting Advanced Scroll Bar Settings");
                    scrollBarFriction.floatValue = friction.floatValue;
                    scrollBarInterpolator.enumValueIndex = interpolator.enumValueIndex;
                    scrollBarTension.floatValue = tension.floatValue;
                    dontMatchScrollBarFriction = false;
                    dontMatchScrollBarInterpolator = false;
                    dontMatchScrollBarTension = false;
                }
                EditorGUILayout.Space();
            }
        }

        private void DrawScrollAnimationVariables()
        {
            showAdvancedScrollSettings = EditorGUILayout.Foldout(showAdvancedScrollSettings, "Scroll Settings", true, EditorStyles.foldout);
            if (showAdvancedScrollSettings)
            {
                EditorGUILayout.PropertyField(scrollSensitivity, new GUIContent("Scroll Sensitivity", "How sensative the scroll snap is to touch pad and scroll wheel events"));
                showScrollInfo.target = scrollSensitivity.floatValue != 0;
                if (EditorGUILayout.BeginFadeGroup(showScrollInfo.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(scrollDelay, new GUIContent("Scroll Delay", "The time in seconds between the last touch pad/scroll wheel event and when the scroll snap starts snapping"));
                    EditorGUILayout.PropertyField(scrollWheelDirection, new GUIContent("Scroll Direction", "What direction scrollwheel/touchpad events will move the content."));
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();
                EditorGUILayout.Space();

                dontMatchScrollFriction = BoolProperty(scrollFriction, new GUIContent("Friction"), dontMatchScrollFriction);
                dontMatchScrollInterpolator = BoolProperty(scrollInterpolator, new GUIContent("Interpolator"), dontMatchScrollInterpolator);
                showScrollTension.target = ShowTension(scrollInterpolator.enumValueIndex);
                if (EditorGUILayout.BeginFadeGroup(showScrollTension.faded))
                {
                    EditorGUI.indentLevel++;
                    dontMatchScrollTension = BoolProperty(scrollTension, new GUIContent("Tension", "Modifies the interpolator"), dontMatchScrollTension);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                if (Button("Match Defaults / Reset"))
                {
                    Undo.RecordObject(scrollSnap, "Resetting Advanced Scroll Settings");
                    scrollSensitivity.floatValue = 1;
                    scrollDelay.floatValue = .02f;
                    scrollFriction.floatValue = friction.floatValue;
                    scrollInterpolator.enumValueIndex = interpolator.enumValueIndex;
                    scrollTension.floatValue = tension.floatValue;
                    dontMatchScrollFriction = false;
                    dontMatchScrollInterpolator = false;
                    dontMatchScrollTension = false;
                }
                EditorGUILayout.Space();
            }
        }

        private void DrawItemFilters()
        {
            showFilters = EditorGUILayout.Foldout(showFilters, "Item Filters", true, EditorStyles.foldout);
            if (showFilters)
            {
                EditorGUI.indentLevel++;
                showCalculateFilter = EditorGUILayout.Foldout(showCalculateFilter, new GUIContent("Calclulate Filter", "Used to filter out any RectTransforms you don't want used in the Content's size calculation and you don't want to be able to snap to."), true, EditorStyles.foldout);
                if (showCalculateFilter)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(calculateFilterMode, new GUIContent());
                    EditorGUI.indentLevel--;
                    addToCalculateFilter.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Add Inactive Children", "Adds inactive/disabled children to the filter."), addToCalculateFilter.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();

                    showCalculateError.target = (calculateFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
                    if (EditorGUILayout.BeginFadeGroup(showCalculateError.faded))
                        EditorGUILayout.HelpBox("An empty whitelist will render the Scroll Snap unable to calculate its size correctly.", MessageType.Error);
                    EditorGUILayout.EndFadeGroup();

                    for (int i = 0; i < calculateFilter.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(calculateFilter.GetArrayElementAtIndex(i), new GUIContent());
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            calculateFilter.DeleteArrayElementAtIndex(i);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 14);
                    if (GUILayout.Button("Add Child"))
                    {
                        calculateFilter.InsertArrayElementAtIndex(calculateFilter.arraySize);
                    }
                    GUILayout.EndHorizontal();
                }
                showSnapFilter = EditorGUILayout.Foldout(showSnapFilter, new GUIContent("Available Snaps Filter", "Used to filter out any RectTransforms you don't want to be able to snap to. If a RectTransform is filtered out in the Calculate Size Filter you cannot snap to it here even if you whitelist it."), true);
                if (showSnapFilter)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(snapFilterMode, new GUIContent());
                    EditorGUI.indentLevel--;
                    addToSnapFilter.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Add Inactive Children", "Adds inactive/disabled children to the filter."), addToSnapFilter.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();

                    showSnapError.target = (snapFilterMode.enumValueIndex == (int)OmniDirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
                    if (EditorGUILayout.BeginFadeGroup(showSnapError.faded))
                        EditorGUILayout.HelpBox("An empty whitelist will render the Scroll Snap unable to snap to items.", MessageType.Error);
                    EditorGUILayout.EndFadeGroup();

                    for (int i = 0; i < snapFilter.arraySize; i++)
                    {
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(snapFilter.GetArrayElementAtIndex(i), new GUIContent());
                        if (GUILayout.Button("-", GUILayout.Width(20)))
                        {
                            snapFilter.DeleteArrayElementAtIndex(i);
                        }
                        EditorGUILayout.EndHorizontal();
                    }

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(EditorGUI.indentLevel * 14);
                    if (GUILayout.Button("Add Child"))
                    {
                        snapFilter.InsertArrayElementAtIndex(snapFilter.arraySize);
                    }
                    GUILayout.EndHorizontal();
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawObjectVariables()
        {
            EditorGUILayout.PropertyField(startItem);
            EditorGUILayout.PropertyField(viewPort);
            EditorGUILayout.PropertyField(horizontalScrollBar);
            EditorGUILayout.PropertyField(verticalScrollBar);
        }

        public void DrawEvents()
        {
            showEvents = EditorGUILayout.Foldout(showEvents, "Events", true);
            if (showEvents)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(onValueChanged);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(startMovement);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(closestSnapChanged);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(snappedToItem);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(targetItemSelected);
                EditorGUI.indentLevel--;
            }
        }

        
        private void ApplyAnimationValues()
        {
            if (!dontMatchScrollBarFriction)
            {
                scrollBarFriction.floatValue = friction.floatValue;
            }
            if (!dontMatchScrollBarInterpolator)
            {
                scrollBarInterpolator.enumValueIndex = interpolator.enumValueIndex;
            }
            if (!dontMatchScrollBarTension)
            {
                scrollBarTension.floatValue = tension.floatValue;
            }

            if (!dontMatchScrollFriction)
            {
                scrollFriction.floatValue = friction.floatValue;
            }
            if (!dontMatchScrollInterpolator)
            {
                scrollInterpolator.enumValueIndex = interpolator.enumValueIndex;
            }
            if (!dontMatchScrollTension)
            {
                scrollTension.floatValue = tension.floatValue;
            }
        }

        private void CheckMatching()
        {
            dontMatchScrollBarFriction = dontMatchScrollBarFriction || scrollBarFriction.floatValue != friction.floatValue;
            dontMatchScrollBarInterpolator = dontMatchScrollBarInterpolator || scrollBarInterpolator.enumValueIndex != interpolator.enumValueIndex;
            dontMatchScrollBarTension = dontMatchScrollBarTension || scrollBarTension.floatValue != tension.floatValue;
            dontMatchScrollFriction = dontMatchScrollFriction || scrollFriction.floatValue != friction.floatValue;
            dontMatchScrollInterpolator = dontMatchScrollInterpolator || scrollInterpolator.enumValueIndex != interpolator.enumValueIndex;
            dontMatchScrollTension = dontMatchScrollTension || scrollTension.floatValue != tension.floatValue;
        }

        private bool ShowTension(int enumValue)
        {
            return enumValue == (int)DirectionalScrollSnap.InterpolatorType.Anticipate || enumValue == (int)DirectionalScrollSnap.InterpolatorType.AnticipateOvershoot || enumValue == (int)DirectionalScrollSnap.InterpolatorType.Overshoot;
        }

        private bool Button(string label)
        {
            var rect = EditorGUI.IndentedRect(
                EditorGUILayout.GetControlRect(new GUILayoutOption[] { }));
            return GUI.Button(rect, label);
        }

        private bool BoolProperty(SerializedProperty property, GUIContent label, bool toggleValue)
        {
            Rect controlRect = EditorGUILayout.GetControlRect();
            Rect indentedRect = EditorGUI.IndentedRect(controlRect);
            float labelWidth = EditorGUIUtility.labelWidth - Mathf.Abs(controlRect.position.x - indentedRect.position.x);

            Rect labelRect = new Rect(indentedRect.position.x, indentedRect.position.y, labelWidth, controlRect.size.y);
            Rect propertyRect = new Rect(controlRect.position.x + labelWidth, controlRect.position.y, controlRect.size.x - (labelWidth + controlRect.size.y + 2), controlRect.size.y);
            Rect toggleRect = new Rect(controlRect.position.x + (controlRect.size.x - controlRect.size.y), controlRect.position.y, controlRect.size.y, controlRect.size.y);

            GUI.enabled = toggleValue;
            GUI.Label(labelRect, label);
            EditorGUI.PropertyField(propertyRect, property, new GUIContent());
            GUI.enabled = true;
            return GUI.Toggle(toggleRect, toggleValue, new GUIContent("", "Override Default Property"));
        }
    }
}
