using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace Fireasy.Windows.Forms
{
    public class Selection
    {
        private TextBoxBase m_textBox;

        /// <summary>
        /// 文本正在被改变的事件。
        /// </summary>
        public event EventHandler TextChanging;

        public Selection(TextBoxBase textBox)
        {
            m_textBox = textBox;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="startPos">选定的起始点。</param>
        /// <param name="endPos">选定的结束点。</param>
        public Selection(TextBoxBase textBox, int startPos, int endPos)
        {
            m_textBox = textBox;
            Set(startPos, endPos);
        }

        public void Set(int startPos, int endPos)
        {
            m_textBox.SelectionStart = startPos;
            m_textBox.SelectionLength = endPos - startPos;
        }

        public void Get(out int startPos, out int endPos)
        {
            startPos = m_textBox.SelectionStart;
            endPos = startPos + m_textBox.SelectionLength;

            if (startPos < 0) startPos = 0;
            if (endPos < startPos) endPos = startPos;
        }

        /// <summary>
        /// 替换文本。
        /// </summary>
        /// <param name="text"></param>
        public void Replace(string text)
        {
            if (TextChanging != null)
                TextChanging(this, null);

            m_textBox.SelectedText = text;
        }

        /// <summary>
        /// 设定选定文本并替换。
        /// </summary>
        /// <param name="startPos"></param>
        /// <param name="endPos"></param>
        /// <param name="text"></param>
        public void SetAndReplace(int startPos, int endPos, string text)
        {
            Set(startPos, endPos);
            Replace(text);
        }

        public void MoveBy(int startPos, int endPos)
        {
            End += endPos;
            Start += startPos;
        }

        public void MoveBy(int pos)
        {
            MoveBy(pos, pos);
        }

        public static Selection operator + (Selection selection, int pos)
        {
            return new Selection(selection.m_textBox, selection.Start + pos, selection.End + pos);
        }

        public TextBoxBase TextBox
        {
            get { return m_textBox; }
        }

        /// <summary>
        /// 选定的文本起始点。
        /// </summary>
        public int Start
        {
            get { return m_textBox.SelectionStart; }
            set { m_textBox.SelectionStart = value; }
        }

        /// <summary>
        /// 选定的文本结束点。
        /// </summary>
        public int End
        {
            get { return m_textBox.SelectionStart + m_textBox.SelectionLength; }
            set { m_textBox.SelectionLength = value - m_textBox.SelectionStart; }
        }

        /// <summary>
        /// 选定的文本长度。
        /// </summary>
        public int Length
        {
            get { return m_textBox.SelectionLength; }
            set { m_textBox.SelectionLength = value; }
        }

        #region Saver
        /// <summary>
        /// 存储器。
        /// </summary>
        public class Saver : IDisposable
        {
            // Fields
            private TextBoxBase m_textBox;
            private Selection m_selection;
            private int m_start, m_end;

            public Saver(TextBoxBase textBox)
            {
                m_textBox = textBox;
                m_selection = new Selection(textBox);
                m_selection.Get(out m_start, out m_end);
            }

            public Saver(TextBoxBase textBox, int startPos, int endPos)
            {
                m_textBox = textBox;
                m_selection = new Selection(textBox);

                m_start = startPos;
                m_end = endPos;
            }

            public void Restore()
            {
                if (m_textBox == null)
                    return;

                m_selection.Set(m_start, m_end);
                m_textBox = null;
            }

            public void Dispose()
            {
                Restore();
            }

            public void MoveTo(int startPos, int endPos)
            {
                m_start = startPos;
                m_end = endPos;
            }

            public void MoveBy(int startPos, int endPos)
            {
                m_start += startPos;
                m_end += endPos;
            }

            public void MoveBy(int pos)
            {
                m_start += pos;
                m_end += pos;
            }

            public static Saver operator + (Saver saver, int pos)
            {
                return new Saver(saver.m_textBox, saver.m_start + pos, saver.m_end + pos);
            }

            public TextBoxBase TextBox
            {
                get { return m_textBox; }
            }

            public int Start
            {
                get { return m_start; }
                set { m_start = value; }
            }

            public int End
            {
                get { return m_end; }
                set { m_end = value; }
            }

            public void Update()
            {
                if (m_textBox != null)
                    m_selection.Get(out m_start, out m_end);
            }

            public void Disable()
            {
                m_textBox = null;
            }
        }
        #endregion
    }
}
