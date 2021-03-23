// -----------------------------------------------------------------------
// <copyright company="Fireasy"
//      email="faib920@126.com"
//      qq="55570729">
//   (c) Copyright Fireasy. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
using Fireasy.Common.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using System.Text;

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

        void ISubjectPersistance.ReadSubjects(string provider, Func<StoredSubject, SubjectRetryStatus> readAndAccept)
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
                var disabled = new List<string>();

                foreach (var fileName in file.GetFileNames(Path.Combine(subPath, "*")))
                {
                    var filePath = Path.Combine(subPath, fileName);

                    var key = fileName.Substring(0, fileName.LastIndexOf("."));

                    try
                    {
                        var content = string.Empty;
                        using (var stream = new StreamReader(filePath, Encoding.UTF8))
                        {
                            content = stream.ReadToEnd();
                        }

                        var option = new JsonSerializeOption();
                        option.Converters.Add(new FullDateTimeJsonConverter());
                        var serializer = new JsonSerializer(option);
                        var subject = serializer.Deserialize<StoredSubject>(content);
                        if (subject.ExpiresAt < DateTime.Now)
                        {
                            disabled.Add(filePath);
                            continue;
                        }

                        subject.PublishRetries++;

                        var status = readAndAccept(subject);
                        if (status == SubjectRetryStatus.Success)
                        {
                            deleted.Add(filePath);
                        }
                        else if (status == SubjectRetryStatus.OutOfTimes)
                        {
                            disabled.Add(filePath);
                        }
                        else
                        {
                            using var stream = new StreamWriter(filePath, false, Encoding.UTF8);
                            content = serializer.Serialize(subject);
                            stream.Write(content);
                        }
                    }
                    catch (Exception exp)
                    {
                        Tracer.Error($"Throw exception when read subject-persistance of '{filePath}': {exp.Message}");
                    }
                }

                foreach (var fileName in deleted)
                {
                    file.DeleteFile(fileName);
                }

                if (disabled.Count > 0)
                {
                    var dir = Path.Combine(root, dirName, "disabled", DateTime.Today.ToString("yyyy-MM-dd"));
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    foreach (var filePath in disabled)
                    {
                        var fileName = Path.GetFileName(filePath);
                        File.Move(filePath, Path.Combine(dir, fileName));
                    }
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
                using var stream = new StreamWriter(filePath, false, Encoding.UTF8);
                var option = new JsonSerializeOption();
                option.Converters.Add(new FullDateTimeJsonConverter());
                var serializer = new JsonSerializer(option);
                var content = serializer.Serialize(subject);
                stream.Write(content);
                return true;
            }
            catch (Exception exp)
            {
                throw new SubjectPersistentException(string.Empty, exp);
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
