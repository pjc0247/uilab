//Dependencies:
// - DirectionalScrollSnap: Source > Scripts > Components

//Contributors:
//BeksOmega

using UnityEditor;
using UnityEditor.AnimatedValues;

namespace UnityEngine.UI.ScrollSnaps
{
    [CustomEditor(typeof(DirectionalScrollSnap))]
    public class DirectionalScrollSnapEditor : Editor
    {

        private bool showFilters,
            showAnimationSettings,
            showCalculateFilter,
            showSnapFilter,
            showEvents,
            showAdvancedButtonSettings,
            showAdvancedScrollSettings,
            showAdvancedScrollBarSettings;

        private bool
            dontMatchButtonInterpolator,
            dontMatchButtonTension,
            dontMatchScrollBarFriction,
            dontMatchScrollBarInterpolator,
            dontMatchScrollBarTension,
            dontMatchScrollFriction,
            dontMatchScrollInterpolator,
            dontMatchScrollTension = true;

        private AnimBool showScrollDelay,
            showCalculateError,
            showSnapError,
            showTension,
            showButtonTension,
            showScrollBarTension,
            showScrollTension,
            showFriction,
            showDrawGizmos,
            showEndSpacing;

        private SerializedProperty
            content,
            movementDirection,
            lockOtherDirection,
            movementType,
            simulateFlings,
            snapType,
            interpolator,
            tension,
            scrollSensitivity,
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
            horizScrollBar,
            vertScrollBar,
            backButton,
            forwardButton,
            onValueChanged,
            startMovement,
            closestItemChanged,
            snappedToItem,
            targetItemSelected,
            drawGizmos,
            friction,
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
            startItem,
            alwaysGoToEnd;

        DirectionalScrollSnap scrollSnap;

        private void OnEnable()
        {
            scrollSnap = (DirectionalScrollSnap)target;

            content = serializedObject.FindProperty("m_Content");
            movementDirection = serializedObject.FindProperty("m_MovementDirection");
            lockOtherDirection = serializedObject.FindProperty("m_LockOtherDirection");
            movementType = serializedObject.FindProperty("m_MovementType");
            simulateFlings = serializedObject.FindProperty("m_SimulateFlings");
            friction = serializedObject.FindProperty("m_Friction");
            snapType = serializedObject.FindProperty("m_SnapType");
            interpolator = serializedObject.FindProperty("m_InterpolatorType");
            tension = serializedObject.FindProperty("m_Tension");
            scrollSensitivity = serializedObject.FindProperty("m_ScrollSensitivity");
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
            horizScrollBar = serializedObject.FindProperty("m_HorizontalScrollbar");
            vertScrollBar = serializedObject.FindProperty("m_VerticalScrollbar");
            backButton = serializedObject.FindProperty("m_BackButton");
            forwardButton = serializedObject.FindProperty("m_ForwardButton");
            loop = serializedObject.FindProperty("m_Loop");
            endSpacing = serializedObject.FindProperty("m_EndSpacing");
            onValueChanged = serializedObject.FindProperty("m_OnValueChanged");
            startMovement = serializedObject.FindProperty("m_StartMovementEvent");
            closestItemChanged = serializedObject.FindProperty("m_ClosestSnapPositionChanged");
            snappedToItem = serializedObject.FindProperty("m_SnappedToItem");
            targetItemSelected = serializedObject.FindProperty("m_TargetItemSelected");
            drawGizmos = serializedObject.FindProperty("m_DrawGizmos");
            buttonAnimationDuration = serializedObject.FindProperty("m_ButtonAnimationDuration");
            buttonItemsToMoveBy = serializedObject.FindProperty("m_ButtonItemsToMoveBy");
            buttonInterpolator = serializedObject.FindProperty("m_ButtonInterpolator");
            scrollFriction = serializedObject.FindProperty("m_ScrollFriction");
            scrollInterpolator = serializedObject.FindProperty("m_ScrollInterpolator");
            scrollBarFriction = serializedObject.FindProperty("m_ScrollBarFriction");
            scrollBarInterpolator = serializedObject.FindProperty("m_ScrollBarInterpolator");
            buttonTension = serializedObject.FindProperty("m_ButtonTension");
            scrollBarTension = serializedObject.FindProperty("m_ScrollBarTension");
            scrollTension = serializedObject.FindProperty("m_ScrollTension");
            allowTouch = serializedObject.FindProperty("m_AllowTouchInput");
            startItem = serializedObject.FindProperty("m_StartItem");
            alwaysGoToEnd = serializedObject.FindProperty("m_ButtonAlwaysGoToEnd");

            showScrollDelay = new AnimBool(scrollSensitivity.floatValue != 0);
            showScrollDelay.valueChanged.AddListener(Repaint);
            showCalculateError = new AnimBool(calculateFilterMode.enumValueIndex == (int)DirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
            showCalculateError.valueChanged.AddListener(Repaint);
            showSnapError = new AnimBool(snapFilterMode.enumValueIndex == (int)DirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
            showSnapError.valueChanged.AddListener(Repaint);
            showTension = new AnimBool(ShowTension(interpolator.enumValueIndex));
            showTension.valueChanged.AddListener(Repaint);
            showButtonTension = new AnimBool(ShowTension(buttonInterpolator.enumValueIndex));
            showButtonTension.valueChanged.AddListener(Repaint);
            showScrollBarTension = new AnimBool(ShowTension(scrollBarInterpolator.enumValueIndex));
            showScrollBarTension.valueChanged.AddListener(Repaint);
            showScrollTension = new AnimBool(ShowTension(scrollInterpolator.enumValueIndex));
            showScrollTension.valueChanged.AddListener(Repaint);
            showDrawGizmos = new AnimBool(scrollSnap.content != null);
            showDrawGizmos.valueChanged.AddListener(Repaint);
            showEndSpacing = new AnimBool(loop.boolValue);
            showEndSpacing.valueChanged.AddListener(Repaint);

            CheckMatching();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            //DrawDefaultInspector();

            GUI.enabled = false;
            EditorGUILayout.ObjectField("Script", MonoScript.FromMonoBehaviour(scrollSnap), typeof(DirectionalScrollSnap), false);
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
            EditorGUILayout.PropertyField(movementDirection);
            EditorGUILayout.PropertyField(lockOtherDirection);
            EditorGUILayout.PropertyField(loop);
            showEndSpacing.target = loop.boolValue;
            if (EditorGUILayout.BeginFadeGroup(showEndSpacing.faded))
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(endSpacing, new GUIContent("End Spacing", "The distance between the end edge of the last item and the start edge of the first item, after the first or last item has looped."));
                EditorGUI.indentLevel--;
            }
            EditorGUILayout.EndFadeGroup();
        }

        private void DrawAnimationVariables()
        {
            showAnimationSettings = EditorGUILayout.Foldout(showAnimationSettings, "Animation Settings", true, EditorStyles.foldout);
            if (showAnimationSettings)
            {
                DrawDefaultAnimationVariables();

                EditorGUILayout.Space();

                EditorGUI.indentLevel++;
                DrawButtonAnimationVariables();
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

            EditorGUILayout.PropertyField(snapType, new GUIContent("Snap Type", "Determines how the scroll snap will decide which item to snap to. If Use Velocity is true it will calculate where the scroll snap would land and then choose based on that position."));

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

        private void DrawButtonAnimationVariables()
        {
            showAdvancedButtonSettings = EditorGUILayout.Foldout(showAdvancedButtonSettings, "Button Settings", true, EditorStyles.foldout);
            if (showAdvancedButtonSettings)
            {
                EditorGUILayout.PropertyField(buttonAnimationDuration, new GUIContent("Duration"));
                EditorGUILayout.PropertyField(buttonItemsToMoveBy, new GUIContent("Items To Move"));
                EditorGUILayout.PropertyField(alwaysGoToEnd, new GUIContent("Go to End", "When enabled the the Scroll Snap will move to the last/first item even if the Items to Move would normally send it past that item. When disabled the Scroll Snap will not move if the movement would send it past."));
                EditorGUILayout.Space();

                dontMatchButtonInterpolator = BoolProperty(buttonInterpolator, new GUIContent("Interpolator"), dontMatchButtonInterpolator);
                showButtonTension.target = ShowTension(buttonInterpolator.enumValueIndex);
                if (EditorGUILayout.BeginFadeGroup(showButtonTension.faded))
                {
                    EditorGUI.indentLevel++;
                    dontMatchButtonTension = BoolProperty(buttonTension, new GUIContent("Tension", "Modifies the interpolator"), dontMatchButtonTension);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.EndFadeGroup();

                if (Button("Match Defaults / Reset"))
                {
                    Undo.RecordObject(scrollSnap, "Resetting Advanced Button Settings");
                    buttonAnimationDuration.floatValue = 1;
                    buttonItemsToMoveBy.intValue = 1;
                    alwaysGoToEnd.boolValue = true;
                    buttonInterpolator.enumValueIndex = interpolator.enumValueIndex;
                    buttonTension.floatValue = tension.floatValue;
                    dontMatchButtonInterpolator = false;
                    dontMatchButtonTension = false;
                }
                EditorGUILayout.Space();
            }
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
                showScrollDelay.target = scrollSensitivity.floatValue != 0;
                if (EditorGUILayout.BeginFadeGroup(showScrollDelay.faded))
                {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(scrollDelay, new GUIContent("Scroll Delay", "The time in seconds between the last touch pad/scroll wheel event and when the scroll snap starts snapping"));
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
                showCalculateFilter = EditorGUILayout.Foldout(showCalculateFilter, new GUIContent("Calculate Size Filter", "Used to filter out any RectTransforms you don't want used in the Content's size calculation and you don't want to be able to snap to."), true);
                if (showCalculateFilter)
                {
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.PropertyField(calculateFilterMode, new GUIContent());
                    EditorGUI.indentLevel--;
                    addToCalculateFilter.boolValue = EditorGUILayout.ToggleLeft(new GUIContent("Add Inactive Children", "Adds inactive/disabled children to the filter."), addToCalculateFilter.boolValue);
                    EditorGUI.indentLevel++;
                    EditorGUILayout.EndHorizontal();

                    showCalculateError.target = (calculateFilterMode.enumValueIndex == (int)DirectionalScrollSnap.FilterMode.WhiteList && calculateFilter.arraySize == 0);
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

                    if (Button("Add Child"))
                    {
                        calculateFilter.InsertArrayElementAtIndex(calculateFilter.arraySize);
                    }

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

                    showSnapError.target = (snapFilterMode.enumValueIndex == (int)DirectionalScrollSnap.FilterMode.WhiteList && snapFilter.arraySize == 0);
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

                    if (Button("Add Child"))
                    {
                        snapFilter.InsertArrayElementAtIndex(snapFilter.arraySize);
                    }
                }
                EditorGUI.indentLevel--;
            }
        }

        private void DrawObjectVariables()
        {
            EditorGUILayout.PropertyField(startItem);
            EditorGUILayout.PropertyField(viewPort);
            EditorGUILayout.PropertyField(horizScrollBar);
            EditorGUILayout.PropertyField(vertScrollBar);
            EditorGUILayout.PropertyField(backButton);
            EditorGUILayout.PropertyField(forwardButton);
        }

        private void DrawEvents()
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
                EditorGUILayout.PropertyField(closestItemChanged);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(snappedToItem);
                EditorGUILayout.Space();
                EditorGUILayout.PropertyField(targetItemSelected);
                EditorGUI.indentLevel--;
            }
        }


        private void ApplyAnimationValues()
        {
            if (!dontMatchButtonInterpolator)
            {
                buttonInterpolator.enumValueIndex = interpolator.enumValueIndex;
            }
            if (!dontMatchButtonTension)
            {
                buttonTension.floatValue = tension.floatValue;
            }

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
            dontMatchButtonInterpolator = dontMatchButtonInterpolator || buttonInterpolator.enumValueIndex != interpolator.enumValueIndex;
            dontMatchButtonTension = dontMatchButtonTension || buttonTension.floatValue != tension.floatValue;
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