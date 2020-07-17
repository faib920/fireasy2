// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Extensions;
using Fireasy.Common.Serialization;
using System;
using System.Collections.Generic;
using System.IO;

namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 使用本地存储来持久化订阅主题。
    /// </summary>
    public class LocalFilePersistance : ISubjectPersistance
    {
        public readonly static LocalFilePersistance Default = new LocalFilePersistance();

        private const string ROOT_DIRECTORY = "_subpersis";
        private static readonly object _locker = new object();
        private bool _isInitialized = false;

        void ISubjectPersistance.ReadSubjects(string provider, Func<StoredSubject, bool> readAndAccept)
        {
            Guard.ArgumentNull(readAndAccept, nameof(readAndAccept));

            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ROOT_DIRECTORY, provider);
            if (!Directory.Exists(root))
            {
                return;
            }


            foreach (var subPath in Directory.GetDirectories(root))
            {
                var dirName = Path.GetFileName(subPath);
                var deleted = new List<string>();

                foreach (var filePath in Directory.GetFiles(subPath))
                {
                    var fileName = Path.GetFileName(filePath);
                    var key = fileName.Substring(0, fileName.LastIndexOf("."));

                    try
                    {
                        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.ReadWrite);
                        using var memory = stream.CopyToMemory();
                        var serializer = new BinaryCompressSerializer();
                        var subject = serializer.Deserialize<StoredSubject>(memory.ToArray());
                        if (subject.ExpiresAt < DateTime.Now)
                        {
                            deleted.Add(filePath);
                            continue;
                        }

                        subject.PublishRetries++;

                        if (readAndAccept(subject))
                        {
                            deleted.Add(filePath);
                        }
                        else
                        {
                            var bytes = serializer.Serialize(subject);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (var fileName in deleted)
                {
                    File.Delete(fileName);
                }
            }
        }

        bool ISubjectPersistance.SaveSubject(string provider, StoredSubject subject)
        {
            Guard.ArgumentNull(subject, nameof(subject));

            var root = InitiaizeRootDirectory();
            var path = Path.Combine(root, provider, subject.Name);

            InitializeSubDirectory(path);

            var filePath = Path.Combine(path, string.Concat(subject.Key, ".dat"));

            try
            {
                using var stream = new FileStream(filePath, FileMode.Create);
                var serializer = new BinaryCompressSerializer();
                var bytes = serializer.Serialize(subject);
                stream.Write(bytes, 0, bytes.Length);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string InitiaizeRootDirectory()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ROOT_DIRECTORY);

            if (_isInitialized)
            {
                return root;
            }

            lock (_locker)
            {
                if (!Directory.Exists(root))
                {
                    Directory.CreateDirectory(root);
                    _isInitialized = true;
                }
            }

            return root;
        }

        private void InitializeSubDirectory(string path)
        {
            lock (_locker)
            {
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
            }
        }
    }
}
