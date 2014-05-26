﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace Vizr
{
    public class Repository
    {
        private Dictionary<string, Package> Items;

        #region Serialization

        public Repository()
        {
            Items = new Dictionary<string, Package>();
            Load();
        }

        public void Save(Package package)
        {
            var settings = new XmlWriterSettings()
            {
                IndentChars = "\t",
                Indent = true,
                OmitXmlDeclaration = true
            };

            using (var stream = RepositoryHelper.GetPathFromPackageName(package.Name).OpenWrite())
            using (var writer = XmlWriter.Create(stream, settings))
            {
                var serializer = new XmlSerializer(typeof(Package));
                var namespaces = new XmlSerializerNamespaces();
                namespaces.Add("", "");

                if (package.Name == RepositoryHelper.DefaultPackageName)
                    writer.WriteComment(RepositoryHelper.DefaultXMLWarning);

                serializer.Serialize(writer, package, namespaces);
            }
        }

        public void Load()
        {
            string lastItemTried = "";

            // generate default package if missing
            if (!RepositoryHelper.DefaultPackage.Exists)
                Save(FactoryPackages.CreateDefaultPackage());

            Items.Clear();

            try
            {
                foreach (var item in RepositoryHelper.Packages)
                {
                    lastItemTried = item.GetNameWithoutExtension();

                    using (var stream = File.OpenRead(item.FullName))
                    {
                        var package = new XmlSerializer(typeof(Package)).Deserialize(stream) as Package;
                        package.Name = item.GetNameWithoutExtension();

                        Items.Add(package.Name, package);
                    }
                }
            }
            catch (Exception ex)
            {
                var message = ex.GetBaseException().Message;

                System.Windows.MessageBox.Show(string.Format("Package '{0}' could not be loaded. This is not unusual as this software is pre-alpha. " +
                                               "Try editing the file and opening Vizr again or delete the file to restore default commands.\n\n" +
                                               "Exception information: \"{1}\"", lastItemTried, message),
                                               "Vizr",
                                               System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);

                System.Diagnostics.Process.Start("explorer.exe", RepositoryHelper.PackagesPath.FullName);

                Environment.Exit(1);
            }

            // last thing: add meta commands
            Items.Add(RepositoryHelper.MetaPackageName, FactoryPackages.CreateMetaPackage());
        }

        #endregion

        public IEnumerable<Entry> Query(string text)
        {
            var allCommands = Items.Values.Where(package => package.Enabled)
                                          .OrderByDescending(package => package.Priority)
                                          .SelectMany(package => package.Items);

            return allCommands.Where(commands => commands.Enabled && commands.Match(text));
        }
    }
}