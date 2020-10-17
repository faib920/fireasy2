using System;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    public class Selection
    {
        private readonly TextBoxBase _textBox;

        /// <summary>
        /// �ı����ڱ��ı���¼���
        /// </summary>
        public event EventHandler TextChanging;

        public Selection(TextBoxBase textBox)
        {
            _textBox = textBox;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textBox"></param>
        /// <param name="startPos">ѡ������ʼ�㡣</param>
        /// <param name="endPos">ѡ���Ľ����㡣</param>
        public Selection(TextBoxBase textBox, int startPos, int endPos)
        {
            _textBox = textBox;
            Set(startPos, endPos);
        }

        public void Set(int startPos, int endPos)
        {
            _textBox.SelectionStart = startPos;
            _textBox.SelectionLength = endPos - startPos;
        }

        public void Get(out int startPos, out int endPos)
        {
            startPos = _textBox.SelectionStart;
            endPos = startPos + _textBox.SelectionLength;

            if (startPos < 0) startPos = 0;
            if (endPos < startPos) endPos = startPos;
        }

        /// <summary>
        /// �滻�ı���
        /// </summary>
        /// <param name="text"></param>
        public void Replace(string text)
        {
            if (TextChanging != null)
                TextChanging(this, null);

            _textBox.SelectedText = text;
        }

        /// <summary>
        /// �趨ѡ���ı����滻��
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

        public static Selection operator +(Selection selection, int pos)
        {
            return new Selection(selection._textBox, selection.Start + pos, selection.End + pos);
        }

        public TextBoxBase TextBox
        {
            get { return _textBox; }
        }

        /// <summary>
        /// ѡ�����ı���ʼ�㡣
        /// </summary>
        public int Start
        {
            get { return _textBox.SelectionStart; }
            set { _textBox.SelectionStart = value; }
        }

        /// <summary>
        /// ѡ�����ı������㡣
        /// </summary>
        public int End
        {
            get { return _textBox.SelectionStart + _textBox.SelectionLength; }
            set { _textBox.SelectionLength = value - _textBox.SelectionStart; }
        }

        /// <summary>
        /// ѡ�����ı����ȡ�
        /// </summary>
        public int Length
        {
            get { return _textBox.SelectionLength; }
            set { _textBox.SelectionLength = value; }
        }

        #region Saver
        /// <summary>
        /// �洢����
        /// </summary>
        public class Saver : IDisposable
        {
            // Fields
            private TextBoxBase _textBox;
            private readonly Selection _selection;
            private int _start, _end;

            public Saver(TextBoxBase textBox)
            {
                _textBox = textBox;
                _selection = new Selection(textBox);
                _selection.Get(out _start, out _end);
            }

            public Saver(TextBoxBase textBox, int startPos, int endPos)
            {
                _textBox = textBox;
                _selection = new Selection(textBox);

                _start = startPos;
                _end = endPos;
            }

            public void Restore()
            {
                if (_textBox == null)
                    return;

                _selection.Set(_start, _end);
                _textBox = null;
            }

            public void Dispose()
            {
                Restore();
            }

            public void MoveTo(int startPos, int endPos)
            {
                _start = startPos;
                _end = endPos;
            }

            public void MoveBy(int startPos, int endPos)
            {
                _start += startPos;
                _end += endPos;
            }

            public void MoveBy(int pos)
            {
                _start += pos;
                _end += pos;
            }

            public static Saver operator +(Saver saver, int pos)
            {
                return new Saver(saver._textBox, saver._start + pos, saver._end + pos);
            }

            public TextBoxBase TextBox
            {
                get { return _textBox; }
            }

            public int Start
            {
                get { return _start; }
                set { _start = value; }
            }

            public int End
            {
                get { return _end; }
                set { _end = value; }
            }

            public void Update()
            {
                if (_textBox != null)
                    _selection.Get(out _start, out _end);
            }

            public void Disable()
            {
                _textBox = null;
            }
        }
        #endregion
    }
}
