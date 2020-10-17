// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using System.Drawing;
using System.Windows.Forms;

namespace Fireasy.Windows.Forms
{
    /// <summary>
    /// 一个抽象类，提供一个包装容器，真实的编辑器位于此容器的中心位置。
    /// </summary>
    public abstract class TreeListWrapEditor<T> : Panel, ITreeListEditor where T : Control
    {
        /// <summary>
        /// 初始化 <see cref="TreeListWrapEditor"/> 类的新实例。
        /// </summary>
        /// <param name="editor">真实的编辑器。</param>
        protected TreeListWrapEditor(T editor)
        {
            Size = Size.Empty;
            Inner = editor;
            Inner.LostFocus += EditorLostFocus;
            Inner.KeyDown += EditorKeyDown;
            Controls.Add(editor);
        }

        /// <summary>
        /// 获取真实的编辑器控件。
        /// </summary>
        public T Inner { get; private set; }

        /// <summary>
        /// 获取或设置编辑器的控制器。
        /// </summary>
        public TreeListEditController Controller { get; set; }

        /// <summary>
        /// 将编辑器显示在指定的范围内。
        /// </summary>
        /// <param name="rect">编辑器放置的范围。</param>
        public virtual void Show(Rectangle rect)
        {
            Location = new Point(rect.X, rect.Y);
            Size = new Size(rect.Width - 1, rect.Height);
        }

        /// <summary>
        /// 隐藏编辑器。
        /// </summary>
        public virtual new void Hide()
        {
        }

        /// <summary>
        /// 设置编辑器的值。
        /// </summary>
        /// <param name="value"></param>
        public abstract void SetValue(object value);

        /// <summary>
        /// 获取编辑器的值。
        /// </summary>
        /// <returns></returns>
        public abstract object GetValue();

        /// <summary>
        /// 获取值是否有效。
        /// </summary>
        /// <returns></returns>
        public virtual bool IsValid()
        {
            return true;
        }

        protected virtual void EditorKeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                Controller.AcceptEdit(true);
            }
            else if (e.KeyCode == Keys.Escape)
            {
                Controller.CancelEdit();
            }
        }

        protected virtual void EditorLostFocus(object sender, System.EventArgs e)
        {
            Controller.AcceptEdit();
            base.OnLostFocus(e);
        }
    }
}
