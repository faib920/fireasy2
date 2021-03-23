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
using System.Text;

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

        void ISubjectPersistance.ReadSubjects(string provider, Func<StoredSubject, SubjectRetryStatus> readAndAccept)
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
                var disabled = new List<string>();

                foreach (var filePath in Directory.GetFiles(subPath))
                {
                    var fileName = Path.GetFileName(filePath);
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
                    File.Delete(fileName);
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

            var root = InitiaizeRootDirectory();
            var path = Path.Combine(root, provider, subject.Name);

            InitializeSubDirectory(path);

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
