namespace FewClicksDev.Core.ReorderableList
{
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEditorInternal;
    using UnityEngine;
    using UnityEngine.Events;

    public abstract class ReorderableList<T> : ScriptableObject
    {
        private const float INDEX_WIDTH = 30f;

        private const string OBJECTS_LIST = "ObjectsList";

        public event UnityAction OnReorder = null;

        public List<T> ObjectsList = new List<T>();

        protected Object mainObject = null;
        protected ReorderableList reorderables = null;
        protected string header = string.Empty;

        public void Init(Object _mainObject, List<T> _objects, string _header = "", bool _displayAdd = true, bool _displayRemove = true)
        {
            mainObject = _mainObject;
            ObjectsList = _objects;
            header = _header;

            bool _showHeader = header.IsNullEmptyOrWhitespace() == false;

            reorderables = new ReorderableList(ObjectsList, typeof(T), true, _showHeader, _displayAdd, _displayRemove);
            reorderables.drawHeaderCallback += drawHeader;
            reorderables.drawElementCallback += drawElement;
            reorderables.onReorderCallback += onReorder;
            reorderables.onAddCallback += onAdd;
            reorderables.onRemoveCallback += onRemove;
        }

        public void Destroy()
        {
            reorderables.drawHeaderCallback -= drawHeader;
            reorderables.drawElementCallback -= drawElement;
            reorderables.onReorderCallback -= onReorder;
            reorderables.onAddCallback -= onAdd;
            reorderables.onRemoveCallback -= onRemove;

            DestroyImmediate(this);
        }

        public void Draw()
        {
            reorderables.DoLayoutList();
        }

        protected virtual void drawHeader(Rect _rect)
        {
            EditorGUI.LabelField(_rect, header);
        }

        private void drawElement(Rect _rect, int _index, bool _active, bool _focused)
        {
            SerializedObject _serializedObject = new SerializedObject(this);
            SerializedProperty _arrayProperty = _serializedObject.FindProperty(OBJECTS_LIST);
            SerializedProperty _singleProperty = _arrayProperty.GetArrayElementAtIndex(_index);

            if (_singleProperty == null)
            {
                return;
            }

            _rect = adjustRectSize(_rect);

            Rect _labelRect = _rect;
            _labelRect.width = INDEX_WIDTH;

            Rect _propertyRect = _rect;
            _propertyRect.x += INDEX_WIDTH;
            _propertyRect.width -= INDEX_WIDTH;

            EditorGUI.LabelField(_labelRect, _index.NumberToString(2));
            EditorGUI.PropertyField(_propertyRect, _singleProperty, GUIContent.none);

            _serializedObject.ApplyModifiedProperties();
        }

        private Rect adjustRectSize(Rect _rect)
        {
            _rect.y += 1;
            _rect.height -= 2;

            return _rect;
        }

        private void onReorder(ReorderableList _list)
        {
            setMainObjectDirty();
            OnReorder?.Invoke();
        }

        protected virtual void onAdd(ReorderableList _list)
        {
            ObjectsList.Add(default);
            setMainObjectDirty();
        }

        private void onRemove(ReorderableList _list)
        {
#if UNITY_2021_3_OR_NEWER

            for (int i = 0; i < _list.selectedIndices.Count; i++)
            {
                ObjectsList.RemoveAt(_list.selectedIndices[i]);
            }

#else
            ObjectsList.RemoveAt(_list.index);
#endif
        }

        private void setMainObjectDirty()
        {
            if (mainObject == null)
            {
                return;
            }

            mainObject.SetAsDirty();
        }

        protected abstract SerializedObject getSerializedObject();
    }
}