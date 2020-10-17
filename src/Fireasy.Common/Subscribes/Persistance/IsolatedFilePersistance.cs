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
using System.IO.IsolatedStorage;

namespace Fireasy.Common.Subscribes.Persistance
{
    /// <summary>
    /// 使用本地用户隔离存储来持久化订阅主题。
    /// </summary>
    public class IsolatedFilePersistance : ISubjectPersistance
    {
        public readonly static IsolatedFilePersistance Default = new IsolatedFilePersistance();

        private const string ROOT_DIRECTORY = "_subpersis";
        private static readonly object _locker = new object();
        private bool _isInitialized = false;

        /// <summary>
        /// 获取或设置存储的范围。
        /// </summary>
        public virtual IsolatedStorageScope Scope { get; set; } = IsolatedStorageScope.User | IsolatedStorageScope.Application;

        void ISubjectPersistance.ReadSubjects(string provider, Func<StoredSubject, bool> readAndAccept)
        {
            Guard.ArgumentNull(readAndAccept, nameof(readAndAccept));

            var root = Path.Combine(ROOT_DIRECTORY, provider);
            var file = IsolatedStorageFile.GetStore(Scope, null);
            if (!file.DirectoryExists(root))
            {
                return;
            }


            foreach (var dirName in file.GetDirectoryNames(Path.Combine(root, "*")))
            {
                var subPath = Path.Combine(root, dirName);
                var deleted = new List<string>();
                foreach (var fileName in file.GetFileNames(Path.Combine(subPath, "*")))
                {
                    var filePath = Path.Combine(subPath, fileName);

                    var key = fileName.Substring(0, fileName.LastIndexOf("."));

                    try
                    {
                        using var stream = new IsolatedStorageFileStream(filePath, FileMode.Open, FileAccess.ReadWrite, file);
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
                            stream.Seek(0, SeekOrigin.Begin);
                            stream.Write(bytes, 0, bytes.Length);
                        }
                    }
                    catch
                    {
                    }
                }

                foreach (var fileName in deleted)
                {
                    file.DeleteFile(fileName);
                }
            }
        }

        bool ISubjectPersistance.SaveSubject(string provider, StoredSubject subject)
        {
            Guard.ArgumentNull(subject, nameof(subject));

            var file = Initiaize(IsolatedStorageFile.GetStore(Scope, null));
            var root = Path.Combine(ROOT_DIRECTORY, provider);
            var path = Path.Combine(root, subject.Name);

            InitializeSubDirectory(file, path);

            var filePath = Path.Combine(path, string.Concat(subject.Key, ".dat"));

            try
            {
                using var stream = new IsolatedStorageFileStream(filePath, FileMode.Create, file);
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

        private IsolatedStorageFile Initiaize(IsolatedStorageFile file)
        {
            if (_isInitialized)
            {
                return file;
            }

            lock (_locker)
            {
                if (!file.DirectoryExists(ROOT_DIRECTORY))
                {
                    file.CreateDirectory(ROOT_DIRECTORY);
                    _isInitialized = true;
                }
            }

            return file;
        }

        private void InitializeSubDirectory(IsolatedStorageFile file, string path)
        {
            lock (_locker)
            {
                if (!file.DirectoryExists(path))
                {
                    file.CreateDirectory(path);
                }
            }
        }
    }
}
