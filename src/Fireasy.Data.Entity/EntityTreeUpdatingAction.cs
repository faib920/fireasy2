// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace Fireasy.Data.Entity
{
    /// <summary>
    /// ʵ�������µĶ������͡�
    /// </summary>
    public enum EntityTreeUpdatingAction
    {
        /// <summary>
        /// ��ǰ�ڵ��ƶ���Ŀ��ڵ����Ӧλ�á�
        /// </summary>
        Move,
        /// <summary>
        /// ��ǰ�ڵ㱻�Ƴ���
        /// </summary>
        Remove,
        /// <summary>
        /// ��ǰ�ڵ�����Ʊ��ı䡣
        /// </summary>
        Rename
    }
}