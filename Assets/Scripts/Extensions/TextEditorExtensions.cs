using UnityEngine;

namespace Assets.Scripts.Extensions
{
    public static class TextEditorExtensions
    {
        public static int GetCaretIndex(this TextEditor textEditor)
        {
            return textEditor.cursorIndex;
        }

        public static void SetCaretIndex(this TextEditor textEditor, int index, bool selectNone = true)
        {
            textEditor.cursorIndex = index;

            if (selectNone)
            {
                textEditor.SelectNone();
            }
        }
    }
}
