using System;
using UnityEditor;
using UnityEngine;

namespace Vulpes.Editor.EnhancedTransformInspector
{
    [CustomEditor(typeof(Transform)), CanEditMultipleObjects]
    public sealed class EnhancedTransformInspector : UnityEditor.Editor
    {
        private const float EPSILON = 0.00001f;
        private const int LABEL_WIDTH = 16;
        private const int ANGLE_LIMIT = 180;
        private const int ANGLE_CORRECTION = 360;
        private const int RESET_BUTTON_SIZE = 22;
        private const int FIELD_MIN_WIDTH = 32;
        private const int ROW_SPACING = 4;

        private readonly Color FADE_COLOR_PRO = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        private readonly Color FADE_COLOR_PERSONAL = new Color(0.0f, 0.0f, 0.0f, 0.5f);

        private SerializedProperty localPosition;
        private SerializedProperty localRotation;
        private SerializedProperty localScale;

        private static bool debugMode;
        private static bool quaternionMode;
        private static bool uniformScalingMode;

        [Flags]
        public enum Axes
        {
            X = 1 << 0,
            Y = 1 << 1,
            Z = 1 << 2,
        }

        private GUIStyle ButtonStyle
        {
            get
            {
                var style = EditorStyles.miniButton;
                style.contentOffset = new Vector2(0, 0);
                style.padding = style.overflow = style.border = style.margin = new RectOffset();
                style.fixedHeight = style.fixedWidth = 0;
                return style;
            }
        }

        private void OnEnable()
        {
            debugMode = false; // This isn't saved and is for debug purposes only.
            quaternionMode = EditorPrefs.GetBool("VulpesTransformInspectorQuaternionMode", false);
            uniformScalingMode = EditorPrefs.GetBool("VulpesTransformInspectorUniformScalingMode", false);

            localPosition = serializedObject.FindProperty("m_LocalPosition");
            localRotation = serializedObject.FindProperty("m_LocalRotation");
            localScale = serializedObject.FindProperty("m_LocalScale");
        }

        private void SetGUIColors(in Color backgroundColor, in Color contentColor)
        {
            GUI.backgroundColor = backgroundColor;
            GUI.contentColor = contentColor;
        }

        private void DrawFloatTransformField(in SerializedProperty serializedProperty, in string toolTexture, in string tooltip, in float resetValue)
        {
            EditorGUILayout.BeginHorizontal();
            // Check Editor Theme
            var defaultColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var fadeColor = EditorGUIUtility.isProSkin ? FADE_COLOR_PRO : FADE_COLOR_PERSONAL;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;
            // Reset Button
            var tex = EditorGUIUtility.FindTexture(toolTexture) as Texture2D;
            GUI.enabled = serializedProperty.vector3Value != (Vector3.one * resetValue);
            SetGUIColors(Color.white, defaultColor);
            var reset = GUILayout.Button(new GUIContent(tex, tooltip), ButtonStyle, GUILayout.Width(RESET_BUTTON_SIZE), GUILayout.Height(RESET_BUTTON_SIZE));
            GUI.enabled = true;
            // All Axes
            var xProperty = serializedProperty.FindPropertyRelative("x");
            var yProperty = serializedProperty.FindPropertyRelative("y");
            var zProperty = serializedProperty.FindPropertyRelative("z");
            var scale = (xProperty.floatValue + yProperty.floatValue + zProperty.floatValue) / 3;
            SetGUIColors(Color.white, xProperty.floatValue == resetValue ? fadeColor : defaultColor);
            scale = EditorGUILayout.FloatField(new GUIContent("S"), scale, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            serializedProperty.vector3Value = (Vector3.one * scale);
            // Reset Colors
            SetGUIColors(Color.white, defaultColor);
            // Reset Behaviour
            if (reset)
            {
                serializedProperty.vector3Value = (Vector3.one * resetValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawVector3TransformField(in SerializedProperty serializedProperty, in string toolTexture, in string tooltip, in Vector3 resetValue)
        {
            EditorGUILayout.BeginHorizontal();
            // Check Editor Theme
            var defaultColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var fadeColor = EditorGUIUtility.isProSkin ? FADE_COLOR_PRO : FADE_COLOR_PERSONAL;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;
            // Reset Button
            var tex = EditorGUIUtility.FindTexture(toolTexture) as Texture2D;
            GUI.enabled = serializedProperty.vector3Value != resetValue;
            SetGUIColors(Color.white, defaultColor);
            var reset = GUILayout.Button(new GUIContent(tex, tooltip), ButtonStyle, GUILayout.Width(RESET_BUTTON_SIZE), GUILayout.Height(RESET_BUTTON_SIZE));
            GUI.enabled = true;
            // X Axis
            var xProperty = serializedProperty.FindPropertyRelative("x");
            SetGUIColors(Handles.xAxisColor, xProperty.floatValue == resetValue.x ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(xProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Y Axis
            var yProperty = serializedProperty.FindPropertyRelative("y");
            SetGUIColors(Handles.yAxisColor, yProperty.floatValue == resetValue.y ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(yProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Z Axis
            var zProperty = serializedProperty.FindPropertyRelative("z");
            SetGUIColors(Handles.zAxisColor, zProperty.floatValue == resetValue.z ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(zProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Reset Colors
            SetGUIColors(Color.white, defaultColor);
            // Reset Behaviour
            if (reset)
            {
                serializedProperty.vector3Value = resetValue;
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawQuaternionTransformField(in SerializedProperty serializedProperty, in string toolTexture, in string tooltip, in Quaternion resetValue)
        {
            EditorGUILayout.BeginHorizontal();
            // Check Editor Theme
            var defaultColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var fadeColor = EditorGUIUtility.isProSkin ? FADE_COLOR_PRO : FADE_COLOR_PERSONAL;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;
            // Reset Button
            var tex = EditorGUIUtility.FindTexture(toolTexture) as Texture2D;
            GUI.enabled = serializedProperty.quaternionValue != resetValue;
            SetGUIColors(Color.white, defaultColor);
            var reset = GUILayout.Button(new GUIContent(tex, tooltip), ButtonStyle, GUILayout.Width(RESET_BUTTON_SIZE), GUILayout.Height(RESET_BUTTON_SIZE));
            GUI.enabled = true;
            // X Value
            var xProperty = serializedProperty.FindPropertyRelative("x");
            SetGUIColors(Handles.xAxisColor, xProperty.floatValue == resetValue.x ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(xProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Y Value
            var yProperty = serializedProperty.FindPropertyRelative("y");
            SetGUIColors(Handles.yAxisColor, yProperty.floatValue == resetValue.y ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(yProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Z Value
            var zProperty = serializedProperty.FindPropertyRelative("z");
            SetGUIColors(Handles.zAxisColor, zProperty.floatValue == resetValue.z ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(zProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // W Value
            var wProperty = serializedProperty.FindPropertyRelative("w");
            SetGUIColors(Handles.selectedColor, wProperty.floatValue == resetValue.w ? fadeColor : defaultColor);
            EditorGUILayout.PropertyField(wProperty, GUILayout.MinWidth(FIELD_MIN_WIDTH));
            // Reset Colors
            SetGUIColors(Color.white, defaultColor);
            // Reset Behaviour
            if (reset)
            {
                serializedProperty.quaternionValue = resetValue;
            }
            EditorGUILayout.EndHorizontal();
        }

        private Axes CheckDifference(in SerializedProperty property)
        {
            Axes axes = 0;
            if (property.hasMultipleDifferentValues)
            {
                Vector3 original = property.quaternionValue.eulerAngles;
                foreach (UnityEngine.Object obj in serializedObject.targetObjects)
                {
                    axes |= CheckDifference(obj as Transform, original);
                    if (axes.HasFlag(Axes.X) || axes.HasFlag(Axes.Y) || axes.HasFlag(Axes.Z))
                    {
                        break;
                    }
                }
            }
            return axes;
        }

        private Axes CheckDifference(in Transform transform, in Vector3 original)
        {
            var next = transform.localEulerAngles;
            Axes axes = 0;
            if (Mathf.Abs(next.x - original.x) > EPSILON)
            {
                axes |= Axes.X;
            }
            if (Mathf.Abs(next.y - original.y) > EPSILON)
            {
                axes |= Axes.Y;
            }
            if (Mathf.Abs(next.z - original.z) > EPSILON)
            {
                axes |= Axes.Z;
            }
            return axes;
        }

        private static bool FloatField(in string name, ref float value, in bool hidden, in bool greyedOut, in GUILayoutOption options)
        {
            var newValue = value;
            GUI.changed = false;
            if (!hidden)
            {
                if (greyedOut)
                {
                    GUI.contentColor = Color.grey;
                    newValue = EditorGUILayout.FloatField(name, newValue, options, GUILayout.MinWidth(FIELD_MIN_WIDTH));
                    GUI.contentColor = Color.white;
                } else
                {
                    newValue = EditorGUILayout.FloatField(name, newValue, options, GUILayout.MinWidth(FIELD_MIN_WIDTH));
                }
            } else if (greyedOut)
            {
                GUI.contentColor = Color.grey;
                float.TryParse(EditorGUILayout.TextField(name, "--", options), out newValue);
                GUI.contentColor = Color.white;
            } else
            {
                float.TryParse(EditorGUILayout.TextField(name, "--", options), out newValue);
            }
            if (GUI.changed && Mathf.Abs(newValue - value) > EPSILON)
            {
                value = newValue;
                return true;
            }
            return false;
        }

        private float WrapAngle(float angle)
        {
            while (angle > ANGLE_LIMIT)
            {
                angle -= ANGLE_CORRECTION;
            }
            while (angle < -ANGLE_LIMIT)
            {
                angle += ANGLE_CORRECTION;
            }
            return angle;
        }

        private void DrawEulerAnglesTransformField(in SerializedProperty serializedProperty, in string toolTexture, in string tooltip)
        {
            EditorGUILayout.BeginHorizontal();
            // Check Editor Theme
            var defaultColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            var fadeColor = EditorGUIUtility.isProSkin ? FADE_COLOR_PRO : FADE_COLOR_PERSONAL;
            var visible = (serializedObject.targetObject as Transform).localEulerAngles;
            visible.x = WrapAngle(visible.x);
            visible.y = WrapAngle(visible.y);
            visible.z = WrapAngle(visible.z);
            Axes changed = CheckDifference(serializedProperty);
            Axes altered = 0;
            EditorGUIUtility.labelWidth = LABEL_WIDTH;
            // Reset Button
            var tex = EditorGUIUtility.FindTexture(toolTexture) as Texture2D;
            GUI.enabled = visible != Vector3.zero;
            SetGUIColors(Color.white, defaultColor);
            var reset = GUILayout.Button(new GUIContent(tex, tooltip), ButtonStyle, GUILayout.Width(RESET_BUTTON_SIZE), GUILayout.Height(RESET_BUTTON_SIZE));
            GUI.enabled = true;
            // X Axis
            SetGUIColors(Handles.xAxisColor, visible.x == 0.0f ? fadeColor : defaultColor);
            if (FloatField("X", ref visible.x, changed.HasFlag(Axes.X), false, GUILayout.MinWidth(FIELD_MIN_WIDTH)))
            {
                altered |= Axes.X;
            }
            // Y Axis
            SetGUIColors(Handles.yAxisColor, visible.y == 0.0f ? fadeColor : defaultColor);
            if (FloatField("Y", ref visible.y, changed.HasFlag(Axes.Y), false, GUILayout.MinWidth(FIELD_MIN_WIDTH)))
            {
                altered |= Axes.Y;
            }
            // Z Axis
            SetGUIColors(Handles.zAxisColor, visible.z == 0.0f ? fadeColor : defaultColor);
            if (FloatField("Z", ref visible.z, changed.HasFlag(Axes.Z), false, GUILayout.MinWidth(FIELD_MIN_WIDTH)))
            {
                altered |= Axes.Z;
            }
            // Reset Colors
            SetGUIColors(Color.white, defaultColor);
            // Reset Behaviour
            if (reset)
            {
                serializedProperty.quaternionValue = Quaternion.identity;
            } else if (altered.HasFlag(Axes.X) || altered.HasFlag(Axes.Y) || altered.HasFlag(Axes.Z))
            {
                Undo.RegisterCompleteObjectUndo(serializedObject.targetObjects, "Inspector");
                foreach (UnityEngine.Object obj in serializedObject.targetObjects)
                {
                    var t = obj as Transform;
                    var v = t.localEulerAngles;
                    if (altered.HasFlag(Axes.X))
                    {
                        v.x = visible.x;
                    }
                    if (altered.HasFlag(Axes.Y))
                    {
                        v.y = visible.y;
                    }
                    if (altered.HasFlag(Axes.Z))
                    {
                        v.z = visible.z;
                    }
                    t.localRotation = Quaternion.Euler(v);
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        public override void OnInspectorGUI()
        {
            if (debugMode)
            {
                DrawDefaultInspector();
                return;
            }
            serializedObject.Update();
            GUILayout.Space(ROW_SPACING);
            DrawVector3TransformField(localPosition, "d_MoveTool", "Reset Position", Vector3.zero);
            GUILayout.Space(ROW_SPACING);
            // We can optionally view rotations as quaternions via a context menu item.
            if (quaternionMode)
            {
                DrawQuaternionTransformField(localRotation, "d_RotateTool", "Reset Position", Quaternion.identity);
            } else
            {
                DrawEulerAnglesTransformField(localRotation, "d_RotateTool", "Reset Rotation");
            }
            GUILayout.Space(ROW_SPACING);
            if (uniformScalingMode)
            {
                DrawFloatTransformField(localScale, "d_ScaleTool", "Reset Scale", 1.0f);
            } else
            {
                DrawVector3TransformField(localScale, "d_ScaleTool", "Reset Scale", Vector3.one);
            }
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("CONTEXT/Transform/Toggle Debug Mode", priority = 1000000)]
        private static void ToggleDebugMode()
        {
            debugMode = !debugMode;
        }

        [MenuItem("CONTEXT/Transform/Toggle Quaternion Mode", priority = 1000000)]
        private static void ToggleQuaternionMode()
        {
            quaternionMode = !quaternionMode;
            EditorPrefs.SetBool("VulpesTransformInspectorQuaternionMode", quaternionMode);
        }

        [MenuItem("CONTEXT/Transform/Toggle Uniform Scaling Mode", priority = 1000000)]
        private static void ToggleUniformScalingMode()
        {
            uniformScalingMode = !uniformScalingMode;
            EditorPrefs.SetBool("VulpesTransformInspectorUniformScalingMode", uniformScalingMode);
        }
    }
}